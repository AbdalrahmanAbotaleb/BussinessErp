using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.DAL
{
    public class SupplierRepository
    {
        public async Task<List<Supplier>> GetAllAsync()
        {
            var list = new List<Supplier>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id,Name,Phone,Email,Address FROM Suppliers ORDER BY Name", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) list.Add(Map(reader));
            }
            return list;
        }

        public async Task<Supplier> GetByIdAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id,Name,Phone,Email,Address FROM Suppliers WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return Map(reader);
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Supplier s)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "INSERT INTO Suppliers (Name,Phone,Email,Address) VALUES (@Name,@Phone,@Email,@Address); SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Name", s.Name);
                cmd.Parameters.AddWithValue("@Phone", (object)s.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)s.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)s.Address ?? DBNull.Value);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Supplier s)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("UPDATE Suppliers SET Name=@Name,Phone=@Phone,Email=@Email,Address=@Address WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", s.Id);
                cmd.Parameters.AddWithValue("@Name", s.Name);
                cmd.Parameters.AddWithValue("@Phone", (object)s.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)s.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)s.Address ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Suppliers WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Supplier>> SearchAsync(string keyword)
        {
            var list = new List<Supplier>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id,Name,Phone,Email,Address FROM Suppliers WHERE Name LIKE @K OR Phone LIKE @K ORDER BY Name", conn))
            {
                cmd.Parameters.AddWithValue("@K", $"%{keyword}%");
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(Map(reader));
                }
            }
            return list;
        }

        private Supplier Map(SqlDataReader r)
        {
            return new Supplier
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                Phone = r.IsDBNull(2) ? "" : r.GetString(2),
                Email = r.IsDBNull(3) ? "" : r.GetString(3),
                Address = r.IsDBNull(4) ? "" : r.GetString(4)
            };
        }
    }
}
