using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.BLL;
using BussinessErp.Helpers;

namespace BussinessErp.DAL
{
    /// <summary>
    /// Repository for dashboard KPIs and chart data.
    /// </summary>
    public class DashboardRepository
    {
        public async Task<(int TransactionCount, decimal TotalSales)> GetDailySalesAsync(DateTime? date = null)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("sp_GetDailySales", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Date", (object)date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserRole", AuthService.CurrentRole);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return (reader.GetInt32(0), reader.GetDecimal(1));
                }
            }
            return (0, 0);
        }

        public async Task<(decimal Revenue, decimal COGS, decimal Expenses, decimal NetProfit)> GetMonthlyProfitAsync(int? year = null, int? month = null)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("sp_GetMonthlyProfit", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Year", (object)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object)month ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserRole", AuthService.CurrentRole);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return (reader.GetDecimal(0), reader.GetDecimal(1), reader.GetDecimal(2), reader.GetDecimal(3));
                }
            }
            return (0, 0, 0, 0);
        }

        public async Task<int> GetLowStockCountAsync()
        {
            var result = await DatabaseHelper.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM Products WHERE Quantity <= ReorderLevel");
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Returns monthly sales totals for the last N months (for trend chart).
        /// </summary>
        public async Task<List<(string Month, decimal Total)>> GetMonthlySalesTrendAsync(int months = 12)
        {
            var list = new List<(string, decimal)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT TOP(@N)
                    CAST(YEAR(Date) AS VARCHAR) + '-' + RIGHT('0' + CAST(MONTH(Date) AS VARCHAR), 2) AS Month,
                    SUM(TotalAmount) AS Total
                  FROM Sales
                  WHERE Date >= DATEADD(MONTH, -@N, GETDATE())
                  GROUP BY YEAR(Date), MONTH(Date)
                  ORDER BY Month", conn))
            {
                cmd.Parameters.AddWithValue("@N", months);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add((reader.GetString(0), reader.GetDecimal(1)));
                }
            }
            return list;
        }

        /// <summary>
        /// Monthly profit data for chart (Revenue, COGS, Profit).
        /// </summary>
        public async Task<List<(string Month, decimal Revenue, decimal COGS, decimal Profit)>> GetMonthlyProfitTrendAsync(int months = 12)
        {
            var list = new List<(string, decimal, decimal, decimal)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT
                    CAST(YEAR(s.Date) AS VARCHAR) + '-' + RIGHT('0' + CAST(MONTH(s.Date) AS VARCHAR), 2) AS Month,
                    SUM(si.Quantity * si.SellPrice) AS Revenue,
                    SUM(si.Quantity * p.CostPrice) AS COGS,
                    SUM(si.Quantity * si.SellPrice) - SUM(si.Quantity * p.CostPrice) AS Profit
                  FROM Sales s
                  INNER JOIN SaleItems si ON s.Id = si.SaleId
                  INNER JOIN Products p ON si.ProductId = p.Id
                  WHERE s.Date >= DATEADD(MONTH, -@N, GETDATE())
                  GROUP BY YEAR(s.Date), MONTH(s.Date)
                  ORDER BY Month", conn))
            {
                cmd.Parameters.AddWithValue("@N", months);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add((reader.GetString(0), reader.GetDecimal(1), reader.GetDecimal(2), reader.GetDecimal(3)));
                }
            }
            return list;
        }

        /// <summary>
        /// Sales by category for pie chart.
        /// </summary>
        public async Task<List<(string Category, decimal Total)>> GetCategoryPerformanceAsync()
        {
            var list = new List<(string, decimal)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT ISNULL(p.Category,'Uncategorized'), SUM(si.Quantity * si.SellPrice) AS Total
                  FROM SaleItems si
                  INNER JOIN Products p ON si.ProductId = p.Id
                  GROUP BY p.Category
                  ORDER BY Total DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    list.Add((reader.GetString(0), reader.GetDecimal(1)));
            }
            return list;
        }

        /// <summary>
        /// Top N selling products.
        /// </summary>
        public async Task<List<(string Name, int UnitsSold, decimal Revenue)>> GetTopProductsAsync(int topN = 5)
        {
            var list = new List<(string, int, decimal)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("sp_GetTopSellingProducts", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TopN", topN);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add((reader.GetString(1), reader.GetInt32(3), reader.GetDecimal(4)));
                }
            }
            return list;
        }
    }
}
