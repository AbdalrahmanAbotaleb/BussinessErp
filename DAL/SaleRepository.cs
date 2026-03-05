using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;
using BussinessErp.Models;
using System.Web.Script.Serialization;

namespace BussinessErp.DAL
{
    public class SaleRepository
    {
        public async Task<List<Sale>> GetAllAsync()
        {
            var list = new List<Sale>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT s.Id, s.CustomerId, ISNULL(c.Name,'Walk-in') AS CustomerName, s.Date, s.TotalAmount
                  FROM Sales s LEFT JOIN Customers c ON s.CustomerId = c.Id
                  ORDER BY s.Date DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new Sale
                    {
                        Id = reader.GetInt32(0),
                        CustomerId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                        CustomerName = reader.GetString(2),
                        Date = reader.GetDateTime(3),
                        TotalAmount = reader.GetDecimal(4)
                    });
                }
            }
            return list;
        }

        public async Task<Sale> GetByIdWithItemsAsync(int id)
        {
            Sale sale = null;
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            {
                using (var cmd = new SqlCommand(
                    @"SELECT s.Id, s.CustomerId, ISNULL(c.Name,'Walk-in'), s.Date, s.TotalAmount
                      FROM Sales s LEFT JOIN Customers c ON s.CustomerId = c.Id WHERE s.Id=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            sale = new Sale
                            {
                                Id = reader.GetInt32(0),
                                CustomerId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                CustomerName = reader.GetString(2),
                                Date = reader.GetDateTime(3),
                                TotalAmount = reader.GetDecimal(4)
                            };
                        }
                    }
                }

                if (sale != null)
                {
                    using (var cmd2 = new SqlCommand(
                        @"SELECT si.Id, si.SaleId, si.ProductId, p.Name, si.Quantity, si.SellPrice
                          FROM SaleItems si INNER JOIN Products p ON si.ProductId = p.Id
                          WHERE si.SaleId = @SaleId", conn))
                    {
                        cmd2.Parameters.AddWithValue("@SaleId", id);
                        using (var reader2 = await cmd2.ExecuteReaderAsync())
                        {
                            while (await reader2.ReadAsync())
                            {
                                sale.Items.Add(new SaleItem
                                {
                                    Id = reader2.GetInt32(0),
                                    SaleId = reader2.GetInt32(1),
                                    ProductId = reader2.GetInt32(2),
                                    ProductName = reader2.GetString(3),
                                    Quantity = reader2.GetInt32(4),
                                    SellPrice = reader2.GetDecimal(5)
                                });
                            }
                        }
                    }
                }
            }
            return sale;
        }

        /// <summary>
        /// Creates a sale using the sp_AddSale stored procedure (handles stock updates).
        /// </summary>
        public async Task<int> AddSaleAsync(int? customerId, List<SaleItem> items)
        {
            // Serialize items to JSON for the SP
            var serializer = new JavaScriptSerializer();
            var itemsJson = serializer.Serialize(items.ConvertAll(i => new
            {
                i.ProductId,
                i.Quantity,
                i.SellPrice
            }));

            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("sp_AddSale", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerId", (object)customerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Items", itemsJson);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task UpdateAsync(Sale sale)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("UPDATE Sales SET CustomerId=@CustomerId, Date=@Date, TotalAmount=@TotalAmount WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", sale.Id);
                cmd.Parameters.AddWithValue("@CustomerId", (object)sale.CustomerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", sale.Date);
                cmd.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Sales WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Sale>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            var list = new List<Sale>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT s.Id, s.CustomerId, ISNULL(c.Name,'Walk-in'), s.Date, s.TotalAmount
                  FROM Sales s LEFT JOIN Customers c ON s.CustomerId = c.Id
                  WHERE CAST(s.Date AS DATE) BETWEEN @From AND @To
                  ORDER BY s.Date DESC", conn))
            {
                cmd.Parameters.AddWithValue("@From", from.Date);
                cmd.Parameters.AddWithValue("@To", to.Date);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Sale
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                            CustomerName = reader.GetString(2),
                            Date = reader.GetDateTime(3),
                            TotalAmount = reader.GetDecimal(4)
                        });
                    }
                }
            }
            return list;
        }
    }
}
