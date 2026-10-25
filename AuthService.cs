
using System;
using MySql.Data.MySqlClient;

namespace SafeVault
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService()
        {
            string password =
                Environment.GetEnvironmentVariable("SAFEVAULT_DB_PASSWORD")
                ?? throw new InvalidOperationException(
                    "SAFEVAULT_DB_PASSWORD environment variable is not set.");

            _connectionString =
                $"Server=localhost;Port=3306;Database=SafeVaultDB;User ID=root;Password={password};";
        }

        // Constructor for tests or other configured environments.
        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool Register(
            string username,
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrEmpty(password))
            {
                return false;
            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(password);

            using var connection =
                new MySqlConnection(_connectionString);

            connection.Open();

            string sql = @"
                INSERT INTO AuthUsers
                    (Username, Email, PasswordHash, Role)
                VALUES
                    (@username, @email, @passwordHash, @role);
            ";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@role", "user");

            try
            {
                return command.ExecuteNonQuery() == 1;
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return false;
            }
        }

        public AuthenticatedUser? Login(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password))
            {
                return null;
            }

            using var connection =
                new MySqlConnection(_connectionString);

            connection.Open();

            string sql = @"
                SELECT Id, Username, PasswordHash, Role
                FROM AuthUsers
                WHERE Username = @username;
            ";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            int id = reader.GetInt32("Id");
            string storedUsername = reader.GetString("Username");
            string passwordHash = reader.GetString("PasswordHash");
            string role = reader.GetString("Role");

            if (!BCrypt.Net.BCrypt.Verify(password, passwordHash))
            {
                return null;
            }

            return new AuthenticatedUser(id, storedUsername, role);
        }
    }

    public class AuthenticatedUser
    {
        public int Id { get; }
        public string Username { get; }
        public string Role { get; }

        public AuthenticatedUser(int id, string username, string role)
        {
            Id = id;
            Username = username;
            Role = role;
        }
    }
}
