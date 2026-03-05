using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.DAL
{
    public class ExpenseRepository
    {
        public async Task<List<Expense>> GetAllAsync()
        {
            var list = new List<Expense>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id,Title,Amount,Category,Description,Date FROM Expenses ORDER BY Date DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) list.Add(Map(reader));
            }
            return list;
        }

        public async Task<int> AddAsync(Expense e)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "INSERT INTO Expenses (Title,Amount,Category,Description,Date) VALUES (@Title,@Amount,@Category,@Description,@Date); SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Title", e.Title);
                cmd.Parameters.AddWithValue("@Amount", e.Amount);
                cmd.Parameters.AddWithValue("@Category", (object)e.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)e.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", e.Date);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Expense e)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("UPDATE Expenses SET Title=@Title,Amount=@Amount,Category=@Category,Description=@Description,Date=@Date WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", e.Id);
                cmd.Parameters.AddWithValue("@Title", e.Title);
                cmd.Parameters.AddWithValue("@Amount", e.Amount);
                cmd.Parameters.AddWithValue("@Category", (object)e.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)e.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", e.Date);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Expenses WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Expense>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            var list = new List<Expense>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "SELECT Id,Title,Amount,Category,Description,Date FROM Expenses WHERE CAST(Date AS DATE) BETWEEN @From AND @To ORDER BY Date DESC", conn))
            {
                cmd.Parameters.AddWithValue("@From", from.Date);
                cmd.Parameters.AddWithValue("@To", to.Date);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(Map(reader));
                }
            }
            return list;
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            var list = new List<string>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT DISTINCT Category FROM Expenses WHERE Category IS NOT NULL ORDER BY Category", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) list.Add(reader.GetString(0));
            }
            return list;
        }

        private Expense Map(SqlDataReader r)
        {
            return new Expense
            {
                Id = r.GetInt32(0),
                Title = r.GetString(1),
                Amount = r.GetDecimal(2),
                Category = r.IsDBNull(3) ? "" : r.GetString(3),
                Description = r.IsDBNull(4) ? "" : r.GetString(4),
                Date = r.GetDateTime(5)
            };
        }
    }
}
