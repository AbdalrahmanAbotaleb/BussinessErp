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
    public class PurchaseRepository
    {
        public async Task<List<Purchase>> GetAllAsync()
        {
            var list = new List<Purchase>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT p.Id, p.SupplierId, ISNULL(s.Name,'Unknown'), p.Date, p.TotalAmount
                  FROM Purchases p LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                  ORDER BY p.Date DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new Purchase
                    {
                        Id = reader.GetInt32(0),
                        SupplierId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                        SupplierName = reader.GetString(2),
                        Date = reader.GetDateTime(3),
                        TotalAmount = reader.GetDecimal(4)
                    });
                }
            }
            return list;
        }

        public async Task<Purchase> GetByIdWithItemsAsync(int id)
        {
            Purchase purchase = null;
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            {
                using (var cmd = new SqlCommand(
                    @"SELECT p.Id, p.SupplierId, ISNULL(s.Name,'Unknown'), p.Date, p.TotalAmount
                      FROM Purchases p LEFT JOIN Suppliers s ON p.SupplierId = s.Id WHERE p.Id=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            purchase = new Purchase
                            {
                                Id = reader.GetInt32(0),
                                SupplierId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                SupplierName = reader.GetString(2),
                                Date = reader.GetDateTime(3),
                                TotalAmount = reader.GetDecimal(4)
                            };
                        }
                    }
                }

                if (purchase != null)
                {
                    using (var cmd2 = new SqlCommand(
                        @"SELECT pi.Id, pi.PurchaseId, pi.ProductId, pr.Name, pi.Quantity, pi.CostPrice
                          FROM PurchaseItems pi INNER JOIN Products pr ON pi.ProductId = pr.Id
                          WHERE pi.PurchaseId = @PurchaseId", conn))
                    {
                        cmd2.Parameters.AddWithValue("@PurchaseId", id);
                        using (var reader2 = await cmd2.ExecuteReaderAsync())
                        {
                            while (await reader2.ReadAsync())
                            {
                                purchase.Items.Add(new PurchaseItem
                                {
                                    Id = reader2.GetInt32(0),
                                    PurchaseId = reader2.GetInt32(1),
                                    ProductId = reader2.GetInt32(2),
                                    ProductName = reader2.GetString(3),
                                    Quantity = reader2.GetInt32(4),
                                    CostPrice = reader2.GetDecimal(5)
                                });
                            }
                        }
                    }
                }
            }
            return purchase;
        }

        /// <summary>
        /// Creates a purchase using the sp_AddPurchase stored procedure.
        /// </summary>
        public async Task<int> AddPurchaseAsync(int? supplierId, List<PurchaseItem> items)
        {
            var serializer = new JavaScriptSerializer();
            var itemsJson = serializer.Serialize(items.ConvertAll(i => new
            {
                i.ProductId,
                i.Quantity,
                i.CostPrice
            }));

            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("sp_AddPurchase", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SupplierId", (object)supplierId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Items", itemsJson);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task UpdateAsync(Purchase purchase)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("UPDATE Purchases SET SupplierId=@SupplierId, Date=@Date, TotalAmount=@TotalAmount WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", purchase.Id);
                cmd.Parameters.AddWithValue("@SupplierId", (object)purchase.SupplierId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", purchase.Date);
                cmd.Parameters.AddWithValue("@TotalAmount", purchase.TotalAmount);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Purchases WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
