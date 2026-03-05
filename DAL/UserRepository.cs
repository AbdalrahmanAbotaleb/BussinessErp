using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.DAL
{
    public class UserRepository
    {
        public async Task<User> GetByUsernameAsync(string username)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id, Username, PasswordHash, Role, CreatedAt FROM Users WHERE Username = @Username", conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Role = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<List<User>> GetAllAsync()
        {
            var list = new List<User>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id, Username, PasswordHash, Role, CreatedAt FROM Users ORDER BY Username", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        PasswordHash = reader.GetString(2),
                        Role = reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4)
                    });
                }
            }
            return list;
        }

        public async Task<int> AddAsync(User user)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@Username, @PasswordHash, @Role); SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@Role", user.Role);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task UpdateAsync(User user)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "UPDATE Users SET Username = @Username, PasswordHash = @PasswordHash, Role = @Role WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", user.Id);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@Role", user.Role);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Users WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
