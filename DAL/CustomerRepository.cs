using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.DAL
{
    public class CustomerRepository
    {
        public async Task<List<Customer>> GetAllAsync()
        {
            var list = new List<Customer>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id, Name, Phone, Email, Address, CreatedAt FROM Customers ORDER BY Name", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(MapCustomer(reader));
                }
            }
            return list;
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id, Name, Phone, Email, Address, CreatedAt FROM Customers WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return MapCustomer(reader);
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Customer cust)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"INSERT INTO Customers (Name,Phone,Email,Address) VALUES (@Name,@Phone,@Email,@Address);
                  SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Name", cust.Name);
                cmd.Parameters.AddWithValue("@Phone", (object)cust.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)cust.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)cust.Address ?? DBNull.Value);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Customer cust)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "UPDATE Customers SET Name=@Name,Phone=@Phone,Email=@Email,Address=@Address WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", cust.Id);
                cmd.Parameters.AddWithValue("@Name", cust.Name);
                cmd.Parameters.AddWithValue("@Phone", (object)cust.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)cust.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)cust.Address ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Customers WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Customer>> SearchAsync(string keyword)
        {
            var list = new List<Customer>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "SELECT Id,Name,Phone,Email,Address,CreatedAt FROM Customers WHERE Name LIKE @K OR Phone LIKE @K OR Email LIKE @K ORDER BY Name", conn))
            {
                cmd.Parameters.AddWithValue("@K", $"%{keyword}%");
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(MapCustomer(reader));
                }
            }
            return list;
        }

        private Customer MapCustomer(SqlDataReader r)
        {
            return new Customer
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                Phone = r.IsDBNull(2) ? "" : r.GetString(2),
                Email = r.IsDBNull(3) ? "" : r.GetString(3),
                Address = r.IsDBNull(4) ? "" : r.GetString(4),
                CreatedAt = r.GetDateTime(5)
            };
        }
    }
}
