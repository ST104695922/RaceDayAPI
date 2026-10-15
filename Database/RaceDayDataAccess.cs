
using Microsoft.Data.SqlClient;

namespace RaceDayAPI.Database
{
    public class RaceDayDataAccess
    {
        private readonly DbConnection dbConnection = new DbConnection();

        public List<T> GetAll<T>(
            string query,
            Func<SqlDataReader, T> map)
        {
            List<T> records = new List<T>();

            using (SqlConnection connection = dbConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(map(reader));
                    }
                }
            }

            return records;
        }

        public T? GetById<T>(
            string query,
            string id,
            Func<SqlDataReader, T> map)
        {
            using (SqlConnection connection = dbConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return map(reader);
                        }
                    }
                }
            }

            return default;
        }

        public int Execute(
            string query,
            Action<SqlCommand> addParameters)
        {
            using (SqlConnection connection = dbConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    addParameters(command);
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}
