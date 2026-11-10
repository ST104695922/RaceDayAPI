using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDayMvc.Services
{
    // The outcome of a call to the API.
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; }

        // True when the API says the user is not logged in (for example the session expired).
        public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
    }

    // The only class in the MVC project that talks to the RaceDay API.
    // Controllers call this class; they never build HTTP requests themselves.
    public class RaceDayApiClient
    {
        // The name of the cookie the API uses for its session.
        private const string ApiSessionCookieName = ".AspNetCore.Session";

        // The session key used to remember the API cookie for this user.
        public const string ApiCookieKey = "ApiCookie";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RaceDayApiClient> _logger;

        public RaceDayApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RaceDayApiClient> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // GET a value (or a list) from the API.
        public Task<ApiResult<T>> GetAsync<T>(string url)
        {
            return SendAsync<T>(HttpMethod.Get, url, null);
        }

        // POST JSON to the API.
        public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null)
        {
            return SendAsync<T>(HttpMethod.Post, url, body);
        }

        // PUT JSON to the API.
        public Task<ApiResult<T>> PutAsync<T>(string url, object? body = null)
        {
            return SendAsync<T>(HttpMethod.Put, url, body);
        }

        // DELETE something through the API.
        public Task<ApiResult<T>> DeleteAsync<T>(string url)
        {
            return SendAsync<T>(HttpMethod.Delete, url, null);
        }

        // Sends an uploaded file to the API as multipart form data.
        // The API stores the file in Azure Blob Storage; the MVC project never does.
        public async Task<ApiResult<T>> PostFileAsync<T>(string url, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType);

            // "file" matches the parameter name on the API action.
            content.Add(fileContent, "file", file.FileName);

            return await SendAsync<T>(HttpMethod.Post, url, content);
        }

        // Builds the request, attaches the user's API session cookie and reads the reply.
        private async Task<ApiResult<T>> SendAsync<T>(
            HttpMethod method,
            string url,
            object? body)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);

                if (body is HttpContent httpContent)
                {
                    request.Content = httpContent;
                }
                else if (body != null)
                {
                    request.Content = new StringContent(
                        JsonSerializer.Serialize(body, JsonOptions),
                        Encoding.UTF8,
                        "application/json");
                }

                // Pass the API session cookie that was saved when the user logged in.
                var session = _httpContextAccessor.HttpContext?.Session;
                string? apiCookie = session?.GetString(ApiCookieKey);

                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.Add("Cookie", apiCookie);
                }

                using var response = await _httpClient.SendAsync(request);

                // Save the API session cookie the first time the API sets one (login).
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    string? newCookie = cookies
                        .Select(c => c.Split(';')[0])
                        .FirstOrDefault(c => c.StartsWith(ApiSessionCookieName + "="));

                    if (newCookie != null && session != null)
                    {
                        session.SetString(ApiCookieKey, newCookie);
                    }
                }

                string json = await response.Content.ReadAsStringAsync();

                var result = new ApiResult<T>
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode
                };

                if (response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        result.Data = JsonSerializer.Deserialize<T>(json, JsonOptions);
                    }
                }
                else
                {
                    result.Message = ReadErrorMessage(response.StatusCode, json);
                }

                return result;
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is JsonException)
            {
                // The API is down, slow or sent something unexpected.
                // Log the detail but only show a friendly message to the user.
                _logger.LogError(ex, "Call to the RaceDay API failed: {Method} {Url}", method, url);

                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    Message = "RaceDay is having trouble reaching its service. Please try again in a moment."
                };
            }
        }

        // Turns an API error reply into a short message that is safe to show.
        private static string ReadErrorMessage(HttpStatusCode status, string json)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        // The API's own errors look like { "message": "..." }.
                        if (root.TryGetProperty("message", out var message) &&
                            message.ValueKind == JsonValueKind.String)
                        {
                            return message.GetString() ?? string.Empty;
                        }

                        // Validation errors look like { "errors": { "Field": ["..."] } }.
                        if (root.TryGetProperty("errors", out var errors) &&
                            errors.ValueKind == JsonValueKind.Object)
                        {
                            var messages = errors.EnumerateObject()
                                .SelectMany(p => p.Value.EnumerateArray())
                                .Select(m => m.GetString())
                                .Where(m => !string.IsNullOrWhiteSpace(m));

                            return string.Join(" ", messages);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Fall through to the generic messages below.
            }

            return status switch
            {
                HttpStatusCode.Unauthorized => "Please log in to continue.",
                HttpStatusCode.Forbidden => "You do not have permission to do that.",
                HttpStatusCode.NotFound => "We could not find what you were looking for.",
                _ => "Something went wrong. Please try again."
            };
        }
    }
}