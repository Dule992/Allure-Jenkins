using System;
using System.Linq;
using System.Text.RegularExpressions;
using API_Automation.Models.Request;
using Bogus;

namespace API_Automation.Helpers
{
    public static class FakeUserFactory
    {
        private static readonly Random _random = new();

        // Generates a strong and valid password
        public static string GenerateValidPassword()
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            string passwordString;

            do
            {
                char GetRandomChar(string chars) => chars[_random.Next(chars.Length)];

                var passwordChars = new char[10]; // slightly longer password for better security

                // Mandatory character types
                passwordChars[0] = GetRandomChar(lower);
                passwordChars[1] = GetRandomChar(upper);
                passwordChars[2] = GetRandomChar(digits);
                passwordChars[3] = GetRandomChar(special);

                // Fill the rest with random characters from all sets
                string allChars = lower + upper + digits + special;
                for (int i = 4; i < passwordChars.Length; i++)
                    passwordChars[i] = GetRandomChar(allChars);

                // Shuffle the array to make the character order unpredictable
                passwordString = new string(passwordChars.OrderBy(_ => _random.Next()).ToArray());

            } while (!IsValidPassword(passwordString)); // repeat until password passes regex validation

            // Optional logging (if Logger class exists)
            try
            {
                Logger.Log($"Generated valid password: {passwordString}");
            }
            catch
            {
                // Ignore if Logger is not implemented
            }

            return passwordString;
        }

        // Regex validation – ensures password complexity
        public static bool IsValidPassword(string password)
        {
            // Must contain at least one uppercase letter, one lowercase letter,
            // one number, one special character, and be at least 8 characters long
            const string pattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\w\d\s]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}
