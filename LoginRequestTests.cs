
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using RaceDayAPI.Models;

namespace RaceDayAPI.Tests
{
    public class LoginRequestTests
    {
        [Fact]
        public void LoginRequest_ValidDetails_PassesValidation()
        {
            var request = new LoginRequest
            {
                Email = "testparticipant@example.com",
                Password = "RaceDayTest123!",
                Role = "Participant"
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);

            bool isValid = Validator.TryValidateObject(
                request,
                context,
                results,
                validateAllProperties: true
            );

            Assert.True(isValid);
        }

        [Fact]
        public void LoginRequest_InvalidEmail_FailsValidation()
        {
            var request = new LoginRequest
            {
                Email = "invalid-email",
                Password = "RaceDayTest123!",
                Role = "Participant"
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);

            bool isValid = Validator.TryValidateObject(
                request,
                context,
                results,
                validateAllProperties: true
            );

            Assert.False(isValid);
        }
    }
}
