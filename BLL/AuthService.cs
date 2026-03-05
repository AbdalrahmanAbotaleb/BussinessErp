using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    /// <summary>
    /// Authentication and user management service.
    /// </summary>
    public class AuthService
    {
        private readonly UserRepository _repo = new UserRepository();
        private static User _currentUser;

        public static User CurrentUser => _currentUser;
        public static string CurrentRole => _currentUser?.Role ?? "Employee";
        public static bool IsAdmin => CurrentRole == "Admin";
        public static bool IsManager => CurrentRole == "Manager" || IsAdmin;

        public async Task<User> LoginAsync(string username, string password)
        {
            if (ValidationHelper.IsNullOrEmpty(username) || ValidationHelper.IsNullOrEmpty(password))
                return null;

            var user = await _repo.GetByUsernameAsync(username);
            if (user == null) return null;

            if (!SecurityHelper.VerifyPassword(password, user.PasswordHash))
                return null;

            _currentUser = user;
            AppLogger.Info($"User '{username}' logged in with role '{user.Role}'.");
            return user;
        }

        public static void Logout()
        {
            AppLogger.Info($"User '{_currentUser?.Username}' logged out.");
            _currentUser = null;
        }

        public async Task<bool> CreateUserAsync(string username, string password, string role)
        {
            RoleGuard.RequiresAdmin("Create User");
            string error;
            if (!ValidationHelper.IsRequired(username, "Username", out error)) return false;
            if (!ValidationHelper.IsMinLength(password, 6, "Password", out error)) return false;

            var existing = await _repo.GetByUsernameAsync(username);
            if (existing != null) return false;

            var user = new User
            {
                Username = username,
                PasswordHash = SecurityHelper.HashPassword(password),
                Role = role
            };
            await _repo.AddAsync(user);
            AppLogger.Info($"User '{username}' created with role '{role}'.");
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            string error;
            if (!ValidationHelper.IsMinLength(newPassword, 6, "Password", out error)) return false;

            var users = await _repo.GetAllAsync();
            var user = users.Find(u => u.Id == userId);
            if (user == null) return false;

            user.PasswordHash = SecurityHelper.HashPassword(newPassword);
            await _repo.UpdateAsync(user);
            return true;
        }

        public async Task<bool> EnsureUserAsync(string username, string password, string role)
        {
            var user = await _repo.GetByUsernameAsync(username);
            if (user == null)
            {
                return await CreateUserAsync(username, password, role);
            }
            
            if (!SecurityHelper.VerifyPassword(password, user.PasswordHash))
            {
                user.PasswordHash = SecurityHelper.HashPassword(password);
                await _repo.UpdateAsync(user);
                AppLogger.Info($"User '{username}' password sync'd to default.");
            }
            return true;
        }

        public Task<System.Collections.Generic.List<User>> GetAllUsersAsync() => _repo.GetAllAsync();
        public Task DeleteUserAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete User");
            return _repo.DeleteAsync(id);
        }
        public Task UpdateUserAsync(User user)
        {
            RoleGuard.RequiresAdmin("Update User");
            return _repo.UpdateAsync(user);
        }
    }
}
