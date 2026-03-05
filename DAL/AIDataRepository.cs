using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;

namespace BussinessErp.DAL
{
    /// <summary>
    /// Repository for AI analysis data queries.
    /// </summary>
    public class AIDataRepository
    {
        /// <summary>
        /// Monthly sales for forecasting (last N months).
        /// </summary>
        public async Task<List<(int Year, int Month, decimal Total)>> GetMonthlySalesHistoryAsync(int months = 18)
        {
            var list = new List<(int, int, decimal)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT YEAR(Date) AS Y, MONTH(Date) AS M, SUM(TotalAmount) AS Total
                  FROM Sales WHERE Date >= DATEADD(MONTH, -@N, GETDATE())
                  GROUP BY YEAR(Date), MONTH(Date)
                  ORDER BY Y, M", conn))
            {
                cmd.Parameters.AddWithValue("@N", months);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2)));
                }
            }
            return list;
        }

        /// <summary>
        /// Product sell-through rate (units sold per month).
        /// </summary>
        public async Task<List<(int ProductId, string Name, int CurrentStock, decimal AvgMonthlySales, int ReorderLevel)>> GetInventoryTurnoverAsync()
        {
            var list = new List<(int, string, int, decimal, int)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT
                    p.Id, p.Name, p.Quantity,
                    ISNULL(CAST(SUM(si.Quantity) AS DECIMAL(18,2)) /
                        NULLIF(DATEDIFF(MONTH, MIN(s.Date), GETDATE()), 0), 0) AS AvgMonthlySales,
                    p.ReorderLevel
                  FROM Products p
                  LEFT JOIN SaleItems si ON p.Id = si.ProductId
                  LEFT JOIN Sales s ON si.SaleId = s.Id
                  GROUP BY p.Id, p.Name, p.Quantity, p.ReorderLevel
                  ORDER BY AvgMonthlySales DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetDecimal(3), reader.GetInt32(4)));
            }
            return list;
        }

        /// <summary>
        /// Profit margins by product.
        /// </summary>
        public async Task<List<(string Name, string Category, decimal CostPrice, decimal SellPrice, decimal MarginPct, int UnitsSold)>> GetProfitMarginsAsync()
        {
            var list = new List<(string, string, decimal, decimal, decimal, int)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT
                    p.Name, ISNULL(p.Category,'N/A'), p.CostPrice, p.SellPrice,
                    CASE WHEN p.SellPrice > 0
                         THEN ((p.SellPrice - p.CostPrice) / p.SellPrice) * 100
                         ELSE 0 END AS MarginPct,
                    ISNULL(SUM(si.Quantity), 0) AS UnitsSold
                  FROM Products p
                  LEFT JOIN SaleItems si ON p.Id = si.ProductId
                  GROUP BY p.Name, p.Category, p.CostPrice, p.SellPrice
                  ORDER BY MarginPct DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    list.Add((reader.GetString(0), reader.GetString(1), reader.GetDecimal(2),
                              reader.GetDecimal(3), reader.GetDecimal(4), reader.GetInt32(5)));
            }
            return list;
        }

        /// <summary>
        /// Category performance summary.
        /// </summary>
        public async Task<List<(string Category, decimal TotalRevenue, decimal TotalProfit, int UnitsSold)>> GetCategoryAnalysisAsync()
        {
            var list = new List<(string, decimal, decimal, int)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT
                    ISNULL(p.Category,'Uncategorized'),
                    SUM(si.Quantity * si.SellPrice) AS Revenue,
                    SUM(si.Quantity * (si.SellPrice - p.CostPrice)) AS Profit,
                    SUM(si.Quantity) AS UnitsSold
                  FROM SaleItems si
                  INNER JOIN Products p ON si.ProductId = p.Id
                  GROUP BY p.Category
                  ORDER BY Revenue DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    list.Add((reader.GetString(0), reader.GetDecimal(1), reader.GetDecimal(2), reader.GetInt32(3)));
            }
            return list;
        }

        /// <summary>
        /// Today's total sales.
        /// </summary>
        public async Task<decimal> GetTodaysSalesAsync()
        {
            var result = await DatabaseHelper.ExecuteScalarAsync(
                "SELECT ISNULL(SUM(TotalAmount),0) FROM Sales WHERE CAST(Date AS DATE) = CAST(GETDATE() AS DATE)");
            return Convert.ToDecimal(result);
        }

        /// <summary>
        /// Best month by revenue.
        /// </summary>
        public async Task<(int Year, int Month, decimal Total)> GetBestMonthAsync()
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 YEAR(Date), MONTH(Date), SUM(TotalAmount) AS Total
                  FROM Sales GROUP BY YEAR(Date), MONTH(Date) ORDER BY Total DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                    return (reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2));
            }
            return (0, 0, 0);
        }

        /// <summary>
        /// Product with highest profit margin.
        /// </summary>
        public async Task<(string Name, decimal Margin)> GetHighestMarginProductAsync()
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 Name,
                    CASE WHEN SellPrice > 0 THEN ((SellPrice - CostPrice) / SellPrice) * 100 ELSE 0 END AS Margin
                  FROM Products ORDER BY Margin DESC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                    return (reader.GetString(0), reader.GetDecimal(1));
            }
            return ("N/A", 0);
        }

        /// <summary>
        /// Products that need restocking soon (will run out within weeks based on sales rate).
        /// </summary>
        public async Task<List<(string Name, int CurrentStock, decimal WeeksLeft)>> GetRestockPredictionsAsync()
        {
            var list = new List<(string, int, decimal)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT p.Name, p.Quantity,
                    CASE WHEN ISNULL(AVG(weekly.WeeklySales),0) > 0
                         THEN CAST(p.Quantity AS DECIMAL(18,2)) / AVG(weekly.WeeklySales)
                         ELSE 999 END AS WeeksLeft
                  FROM Products p
                  OUTER APPLY (
                      SELECT CAST(SUM(si.Quantity) AS DECIMAL(18,2)) /
                             NULLIF(DATEDIFF(WEEK, MIN(s.Date), GETDATE()),0) AS WeeklySales
                      FROM SaleItems si INNER JOIN Sales s ON si.SaleId = s.Id
                      WHERE si.ProductId = p.Id
                  ) weekly
                  GROUP BY p.Id, p.Name, p.Quantity
                  HAVING CASE WHEN ISNULL(AVG(weekly.WeeklySales),0) > 0
                              THEN CAST(p.Quantity AS DECIMAL(18,2)) / AVG(weekly.WeeklySales)
                              ELSE 999 END < 8
                  ORDER BY WeeksLeft ASC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    list.Add((reader.GetString(0), reader.GetInt32(1), reader.GetDecimal(2)));
            }
            return list;
        }

        /// <summary>
        /// Slow-moving products (low sales in last 3 months).
        /// </summary>
        public async Task<List<(string Name, string Category, int Stock, int UnitsSold3M)>> GetSlowMoversAsync()
        {
            var list = new List<(string, string, int, int)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT p.Name, ISNULL(p.Category,'N/A'), p.Quantity,
                    ISNULL((SELECT SUM(si.Quantity) FROM SaleItems si
                            INNER JOIN Sales s ON si.SaleId = s.Id
                            WHERE si.ProductId = p.Id AND s.Date >= DATEADD(MONTH,-3,GETDATE())), 0)
                  FROM Products p
                  WHERE p.Quantity > 0
                  ORDER BY 4 ASC", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    list.Add((reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3)));
            }
            return list;
        }

        /// <summary>
        /// Get top customers based on sales volume and invoice count within a period.
        /// </summary>
        public async Task<List<(string Name, decimal TotalRevenue, int InvoiceCount)>> GetTopCustomerDataAsync(int days = 90)
        {
            var list = new List<(string, decimal, int)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT c.Name, SUM(s.TotalAmount) AS TotalRevenue, COUNT(s.Id) AS InvoiceCount
                  FROM Customers c
                  INNER JOIN Sales s ON c.Id = s.CustomerId
                  WHERE s.Date >= DATEADD(DAY, -@Days, GETDATE())
                  GROUP BY c.Id, c.Name
                  ORDER BY TotalRevenue DESC", conn))
            {
                cmd.Parameters.AddWithValue("@Days", days);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add((reader.GetString(0), reader.GetDecimal(1), reader.GetInt32(2)));
                }
            }
            return list;
        }

        /// <summary>
        /// Get top suppliers based on purchase volume and order count within a period.
        /// </summary>
        public async Task<List<(string Name, decimal TotalSpend, int OrderCount)>> GetTopSupplierDataAsync(int days = 90)
        {
            var list = new List<(string, decimal, int)>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"SELECT sup.Name, SUM(p.TotalAmount) AS TotalSpend, COUNT(p.Id) AS OrderCount
                  FROM Suppliers sup
                  INNER JOIN Purchases p ON sup.Id = p.SupplierId
                  WHERE p.Date >= DATEADD(DAY, -@Days, GETDATE())
                  GROUP BY sup.Id, sup.Name
                  ORDER BY TotalSpend DESC", conn))
            {
                cmd.Parameters.AddWithValue("@Days", days);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add((reader.GetString(0), reader.GetDecimal(1), reader.GetInt32(2)));
                }
            }
            return list;
        }
    }
}
