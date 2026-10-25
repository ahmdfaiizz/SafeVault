
using System;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using SafeVault;

namespace SafeVault.Tests
{
    public class AuthenticationAuthorizationTests
    {
        private string _connectionString = null!;
        private AuthService _authService = null!;
        private AuthorizationService _authorizationService = null!;

        private string _username = null!;
        private string _email = null!;

        [SetUp]
        public void SetUp()
        {
            string password =
                Environment.GetEnvironmentVariable("SAFEVAULT_DB_PASSWORD")
                ?? throw new InvalidOperationException(
                    "SAFEVAULT_DB_PASSWORD environment variable is not set.");

            _connectionString =
                $"Server=localhost;Port=3306;Database=SafeVaultTestDB;User ID=root;Password={password};";

            _authService = new AuthService(_connectionString);
            _authorizationService = new AuthorizationService();

            string uniqueId = Guid.NewGuid().ToString("N");

            _username = "test_" + uniqueId;
            _email = uniqueId + "@example.com";
        }

        [TearDown]
        public void TearDown()
        {
            using var connection =
                new MySqlConnection(_connectionString);

            connection.Open();

            using var command = new MySqlCommand(
                "DELETE FROM AuthUsers WHERE Username = @username",
                connection);

            command.Parameters.AddWithValue("@username", _username);
            command.ExecuteNonQuery();
        }

        [Test]
        public void Register_NewUser_ReturnsTrue()
        {
            bool result = _authService.Register(
                _username,
                _email,
                "StrongPassword123!");

            Assert.That(result, Is.True);
        }

        [Test]
        public void Register_DuplicateUsername_ReturnsFalse()
        {
            bool first = _authService.Register(
                _username,
                _email,
                "StrongPassword123!");

            bool second = _authService.Register(
                _username,
                "another@example.com",
                "AnotherPassword123!");

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
        }

        [Test]
        public void Login_CorrectPassword_ReturnsUser()
        {
            _authService.Register(
                _username,
                _email,
                "StrongPassword123!");

            AuthenticatedUser? user = _authService.Login(
                _username,
                "StrongPassword123!");

            Assert.That(user, Is.Not.Null);
            Assert.That(user!.Username, Is.EqualTo(_username));
            Assert.That(user.Role, Is.EqualTo("user"));
        }

        [Test]
        public void Login_WrongPassword_ReturnsNull()
        {
            _authService.Register(
                _username,
                _email,
                "StrongPassword123!");

            AuthenticatedUser? user = _authService.Login(
                _username,
                "WrongPassword123!");

            Assert.That(user, Is.Null);
        }

        [Test]
        public void RegularUser_CannotAccessAdminDashboard()
        {
            _authService.Register(
                _username,
                _email,
                "StrongPassword123!");

            AuthenticatedUser? user = _authService.Login(
                _username,
                "StrongPassword123!");

            Assert.That(
                _authorizationService.CanAccessAdminDashboard(user),
                Is.False);
        }

        [Test]
        public void Admin_CanAccessAdminDashboard()
        {
            string adminUsername = "admin_" + Guid.NewGuid().ToString("N");
            string adminEmail = adminUsername + "@example.com";

            try
            {
                using var connection =
                    new MySqlConnection(_connectionString);

                connection.Open();

                string passwordHash =
                    BCrypt.Net.BCrypt.HashPassword("AdminPassword123!");

                string sql = @"
                    INSERT INTO AuthUsers
                        (Username, Email, PasswordHash, Role)
                    VALUES
                        (@username, @email, @hash, @role);
                ";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@username", adminUsername);
                command.Parameters.AddWithValue("@email", adminEmail);
                command.Parameters.AddWithValue("@hash", passwordHash);
                command.Parameters.AddWithValue("@role", "admin");

                command.ExecuteNonQuery();

                AuthenticatedUser? admin = _authService.Login(
                    adminUsername,
                    "AdminPassword123!");

                Assert.That(admin, Is.Not.Null);
                Assert.That(
                    _authorizationService.CanAccessAdminDashboard(admin),
                    Is.True);
            }
            finally
            {
                using var connection =
                    new MySqlConnection(_connectionString);

                connection.Open();

                using var command = new MySqlCommand(
                    "DELETE FROM AuthUsers WHERE Username = @username",
                    connection);

                command.Parameters.AddWithValue("@username", adminUsername);
                command.ExecuteNonQuery();
            }
        }

        [Test]
        public void UnauthenticatedUser_CannotAccessAdminDashboard()
        {
            Assert.That(
                _authorizationService.CanAccessAdminDashboard(null),
                Is.False);
        }
    }
}
