
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay_api_proj.Tests
{
    public class AuthTests
    {
        [Fact]
        public async Task GetEvents_WithoutLogin_ReturnsSuccess()
        {
            using var factory = new RaceDayApiFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/Events");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithValidDetails_ReturnsSuccess()
        {
            using var factory = new RaceDayApiFactory();
            using var client = factory.CreateClient();

            var registration = new
            {
                firstName = "Test",
                lastName = "Participant",
                email = "participant.test@example.com",
                password = "TestPass123!",
                role = "Participant",
                phone = "0712345678",
                dateOfBirth = "2005-05-15"
            };

            var response = await client.PostAsJsonAsync(
                "/api/Auth/register", registration);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected successful registration, but got " +
                $"{(int)response.StatusCode}: " +
                $"{await response.Content.ReadAsStringAsync()}");
        }


        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            using var factory = new RaceDayApiFactory();
            using var client = factory.CreateClient();

            // Register the participant first
            var registration = new
            {
                firstName = "Test",
                lastName = "Participant",
                email = "login.test@example.com",
                password = "TestPass123!",
                role = "Participant",
                phone = "0712345678",
                dateOfBirth = "2005-05-15"
            };

            var registerResponse = await client.PostAsJsonAsync(
                "/api/Auth/register", registration);

            Assert.True(
                registerResponse.IsSuccessStatusCode,
                "Participant registration failed before the login test.");

            // Attempt to log in
            var login = new
            {
                email = "login.test@example.com",
                password = "TestPass123!"
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login", login);

            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                loginResponse.StatusCode);

            var result = await loginResponse.Content
                .ReadFromJsonAsync<LoginTestResponse>();

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal("Participant", result.Role);
        }

        private class LoginTestResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }


        [Fact]
        public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
        {
            using var factory = new RaceDayApiFactory();
            using var client = factory.CreateClient();

            // Register a participant
            var registration = new
            {
                firstName = "Test",
                lastName = "Participant",
                email = "wrong.password@example.com",
                password = "TestPass123!",
                role = "Participant",
                phone = "0712345678",
                dateOfBirth = "2005-05-15"
            };

            var registerResponse = await client.PostAsJsonAsync(
                "/api/Auth/register", registration);

            Assert.True(
                registerResponse.IsSuccessStatusCode,
                "Registration failed before the login test.");

            // Try logging in with the wrong password
            var login = new
            {
                email = "wrong.password@example.com",
                password = "WrongPass123!"
            };

            var response = await client.PostAsJsonAsync(
                "/api/Auth/login", login);

            Assert.Equal(
                System.Net.HttpStatusCode.Unauthorized,
                response.StatusCode);
        }


        [Fact]
        public async Task Participant_CannotCreateEvent_ReturnsForbidden()
        {
            using var factory = new RaceDayApiFactory();
            using var client = factory.CreateClient();

            // Register a participant
            var registration = new
            {
                firstName = "Test",
                lastName = "Participant",
                email = "role.test@example.com",
                password = "TestPass123!",
                role = "Participant",
                phone = "0712345678",
                dateOfBirth = "2005-05-15"
            };

            await client.PostAsJsonAsync(
                "/api/Auth/register", registration);

            // Log in as the participant
            var login = new
            {
                email = "role.test@example.com",
                password = "TestPass123!"
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login", login);

            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                loginResponse.StatusCode);

            var loginResult =
                await loginResponse.Content.ReadFromJsonAsync<LoginTestResponse>();

            Assert.NotNull(loginResult);
            Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));

            // Attach the participant's JWT
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", loginResult.Token);

            // Try creating an event as a participant
            var newEvent = new
            {
                eventName = "Test Race",
                description = "Automated test event",
                eventDate = "2027-06-15",
                location = "Johannesburg",
                distance = 10,
                eventType = "Run",
                status = "Upcoming"
            };

            var response = await client.PostAsJsonAsync(
                "/api/Events", newEvent);

            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotAccessParticipantProfile_ReturnsForbidden()
        {
            using var factory = new RaceDayApiFactory();
            using var client = factory.CreateClient();

            // Register an organiser
            var registration = new
            {
                firstName = "Test",
                lastName = "Organiser",
                email = "organiser.role.test@example.com",
                password = "TestPass123!",
                role = "Organiser",
                phone = "0712345678"
            };

            var registerResponse = await client.PostAsJsonAsync(
                "/api/Auth/register", registration);

            Assert.True(
                registerResponse.IsSuccessStatusCode,
                "Organiser registration failed.");

            // Log in as the organiser
            var login = new
            {
                email = "organiser.role.test@example.com",
                password = "TestPass123!"
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login", login);

            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                loginResponse.StatusCode);

            var loginResult =
                await loginResponse.Content.ReadFromJsonAsync<LoginTestResponse>();

            Assert.NotNull(loginResult);
            Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));

            // Attach the organiser's JWT
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", loginResult.Token);

            // Try accessing the participant-only profile
            var response = await client.GetAsync(
                "/api/Participants/profile");

            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);
        }


        [Fact]
        public async Task GetEnrolments_WithoutLogin_ReturnsUnauthorized()
        {
            using var factory = new RaceDayApiFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/Enrolments");

            Assert.Equal(
                System.Net.HttpStatusCode.Unauthorized,
                response.StatusCode);
        }



    }
}




