using System;

namespace SafeVault
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to SafeVault!");

            Console.Write("Enter your username: ");
            string username = Console.ReadLine() ?? "";

            if (!InputValidator.IsValidUsername(username))
            {
                Console.WriteLine("Invalid username.");
                return;
            }

            Console.Write("Enter your email: ");
            string email = Console.ReadLine() ?? "";

            if (!InputValidator.IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return;
            }

            try
            {
                UserRepository repository = new UserRepository();

                bool success = repository.AddUser(username, email);

                if (success)
                {
                    Console.WriteLine("User saved successfully!");
                }
                else
                {
                    Console.WriteLine("The user could not be saved.");
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}