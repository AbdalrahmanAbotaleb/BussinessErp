using System;
using System.Security.Cryptography;
using System.Text;

namespace BussinessErp.Helpers
{
    /// <summary>
    /// SHA256 password hashing utility.
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// Hashes a plain-text password using SHA256.
        /// </summary>
        public static string HashPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// Verifies a plain-text password against a stored hash.
        /// </summary>
        public static bool VerifyPassword(string plainText, string storedHash)
        {
            string computedHash = HashPassword(plainText);
            return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
