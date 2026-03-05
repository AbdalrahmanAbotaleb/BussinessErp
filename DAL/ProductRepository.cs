using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.DAL
{
    public class ProductRepository
    {
        public async Task<List<Product>> GetAllAsync()
        {
            var list = new List<Product>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id,Name,Category,CostPrice,SellPrice,Quantity,ReorderLevel,CreatedAt FROM Products ORDER BY Name", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) list.Add(Map(reader));
            }
            return list;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id,Name,Category,CostPrice,SellPrice,Quantity,ReorderLevel,CreatedAt FROM Products WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return Map(reader);
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Product p)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"INSERT INTO Products (Name,Category,CostPrice,SellPrice,Quantity,ReorderLevel)
                  VALUES (@Name,@Category,@CostPrice,@SellPrice,@Quantity,@ReorderLevel);
                  SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Name", p.Name);
                cmd.Parameters.AddWithValue("@Category", (object)p.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CostPrice", p.CostPrice);
                cmd.Parameters.AddWithValue("@SellPrice", p.SellPrice);
                cmd.Parameters.AddWithValue("@Quantity", p.Quantity);
                cmd.Parameters.AddWithValue("@ReorderLevel", p.ReorderLevel);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Product p)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"UPDATE Products SET Name=@Name,Category=@Category,CostPrice=@CostPrice,
                  SellPrice=@SellPrice,Quantity=@Quantity,ReorderLevel=@ReorderLevel WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name);
                cmd.Parameters.AddWithValue("@Category", (object)p.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CostPrice", p.CostPrice);
                cmd.Parameters.AddWithValue("@SellPrice", p.SellPrice);
                cmd.Parameters.AddWithValue("@Quantity", p.Quantity);
                cmd.Parameters.AddWithValue("@ReorderLevel", p.ReorderLevel);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Products WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Product>> SearchAsync(string keyword)
        {
            var list = new List<Product>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "SELECT Id,Name,Category,CostPrice,SellPrice,Quantity,ReorderLevel,CreatedAt FROM Products WHERE Name LIKE @K OR Category LIKE @K ORDER BY Name", conn))
            {
                cmd.Parameters.AddWithValue("@K", $"%{keyword}%");
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(Map(reader));
                }
            }
            return list;
        }

        public async Task<List<Product>> GetLowStockAsync()
        {
            var list = new List<Product>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("EXEC sp_GetLowStockProducts", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new Product
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Category = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Quantity = reader.GetInt32(3),
                        ReorderLevel = reader.GetInt32(4)
                    });
                }
            }
            return list;
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            var list = new List<string>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT DISTINCT Category FROM Products WHERE Category IS NOT NULL ORDER BY Category", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    list.Add(reader.GetString(0));
            }
            return list;
        }

        public async Task<List<Product>> GetByCategoryAsync(string category)
        {
            var list = new List<Product>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "SELECT Id,Name,Category,CostPrice,SellPrice,Quantity,ReorderLevel,CreatedAt FROM Products WHERE Category=@Cat ORDER BY Name", conn))
            {
                cmd.Parameters.AddWithValue("@Cat", category);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(Map(reader));
                }
            }
            return list;
        }

        /// <summary>
        /// Update stock via stored procedure only.
        /// </summary>
        public async Task UpdateStockAsync(int productId, int quantityChange, bool isDecrease)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("sp_UpdateProductStock", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductId", productId);
                cmd.Parameters.AddWithValue("@QuantityChange", quantityChange);
                cmd.Parameters.AddWithValue("@IsDecrease", isDecrease);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private Product Map(SqlDataReader r)
        {
            return new Product
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                Category = r.IsDBNull(2) ? "" : r.GetString(2),
                CostPrice = r.GetDecimal(3),
                SellPrice = r.GetDecimal(4),
                Quantity = r.GetInt32(5),
                ReorderLevel = r.GetInt32(6),
                CreatedAt = r.GetDateTime(7)
            };
        }
    }
}
