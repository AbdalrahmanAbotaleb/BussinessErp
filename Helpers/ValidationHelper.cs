using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BussinessErp.Helpers
{
    /// <summary>
    /// Input validation helpers for the BLL/UI layers.
    /// </summary>
    public static class ValidationHelper
    {
        public static bool IsNullOrEmpty(string value) => string.IsNullOrWhiteSpace(value);

        public static bool IsRequired(string value, string fieldName, out string error)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                error = $"{fieldName} is required.";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsPositiveNumber(decimal value, string fieldName, out string error)
        {
            if (value <= 0)
            {
                error = $"{fieldName} must be greater than zero.";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsNonNegativeNumber(decimal value, string fieldName, out string error)
        {
            if (value < 0)
            {
                error = $"{fieldName} cannot be negative.";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsPositiveInt(int value, string fieldName, out string error)
        {
            if (value <= 0)
            {
                error = $"{fieldName} must be greater than zero.";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsValidEmail(string email, out string error)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                error = null; // Email is optional
                return true;
            }
            try
            {
                var addr = new MailAddress(email);
                error = null;
                return addr.Address == email;
            }
            catch
            {
                error = "Invalid email format.";
                return false;
            }
        }

        public static bool IsValidPhone(string phone, out string error)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                error = null; // Phone is optional
                return true;
            }
            if (!Regex.IsMatch(phone, @"^[\d\+\-\(\)\s]{7,20}$"))
            {
                error = "Invalid phone number format.";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsMinLength(string value, int minLength, string fieldName, out string error)
        {
            if (value != null && value.Length < minLength)
            {
                error = $"{fieldName} must be at least {minLength} characters.";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsInRange(decimal value, decimal min, decimal max, string fieldName, out string error)
        {
            if (value < min || value > max)
            {
                error = $"{fieldName} must be between {min:N2} and {max:N2}.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
