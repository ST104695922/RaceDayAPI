using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDayAPI.Models;
using Xunit;
namespace RaceDayAPI.Tests
{
    public class RegisterRequestTests
    {
        // Test that a valid registration request passes validation.
        [Fact]
        public void RegisterRequest_ValidDetails_PassesValidation()
        {
            var request = new RegisterRequest
            {
                FullName = "Test Participant",
                Email = "testparticipant@example.com",
                Password = "RaceDayTest123!",
                Role = "Participant"
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);

            bool isValid = Validator.TryValidateObject(
                request,
                validationContext,
                validationResults,
                validateAllProperties: true
            );

            Assert.True(isValid);
        }

        // Test that an invalid email fails validation.
        [Fact]
        public void RegisterRequest_InvalidEmail_FailsValidation()
        {
            var request = new RegisterRequest
            {
                FullName = "Test Participant",
                Email = "not-an-email",
                Password = "RaceDayTest123!",
                Role = "Participant"
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);

            bool isValid = Validator.TryValidateObject(
                request,
                validationContext,
                validationResults,
                validateAllProperties: true
            );

            Assert.False(isValid);
        }
    }
}
