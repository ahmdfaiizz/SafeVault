
using System;
using MySql.Data.MySqlClient;

namespace SafeVault
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            string password =
                Environment.GetEnvironmentVariable("SAFEVAULT_DB_PASSWORD")
                ?? throw new InvalidOperationException(
                    "SAFEVAULT_DB_PASSWORD environment variable is not set.");

            _connectionString =
                $"Server=localhost;Port=3306;Database=SafeVaultDB;User ID=root;Password={password};";
        }

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddUser(string username, string email)
        {
            using var connection =
                new MySqlConnection(_connectionString);

            connection.Open();

            string sql = @"
                INSERT INTO Users (Username, Email)
                VALUES (@username, @email);
            ";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@email", email);

            int rowsAffected = command.ExecuteNonQuery();

            return rowsAffected == 1;
        }
    }
}
