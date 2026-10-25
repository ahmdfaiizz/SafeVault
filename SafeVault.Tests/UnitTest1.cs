
using System;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using SafeVault;

namespace SafeVault.Tests
{
    public class UserRepositoryTests
    {
        private readonly string _connectionString =
            CreateTestConnectionString();

        private static string CreateTestConnectionString()
        {
            string password =
                Environment.GetEnvironmentVariable("SAFEVAULT_DB_PASSWORD")
                ?? throw new InvalidOperationException(
                    "SAFEVAULT_DB_PASSWORD environment variable is not set.");

            return
                $"Server=localhost;Port=3306;Database=SafeVaultTestDB;User ID=root;Password={password};";
        }

        [Test]
        public void AddUser_StoresSqlLookingUsernameAsData()
        {
            string username = "' OR 1=1 --";
            string email = "test@example.com";

            var repository = new UserRepository(_connectionString);

            bool success = repository.AddUser(username, email);

            Assert.That(success, Is.True);

            try
            {
                using var connection =
                    new MySqlConnection(_connectionString);

                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Username = @username
                    AND Email = @email;
                ";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@email", email);

                int count = Convert.ToInt32(command.ExecuteScalar());

                Assert.That(count, Is.EqualTo(1));
            }
            finally
            {
                using var connection =
                    new MySqlConnection(_connectionString);

                connection.Open();

                string deleteSql = @"
                    DELETE FROM Users
                    WHERE Username = @username
                    AND Email = @email;
                ";

                using var command =
                    new MySqlCommand(deleteSql, connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@email", email);

                command.ExecuteNonQuery();
            }
        }
    }
}
