using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;

namespace BussinessErp.DAL
{
    public class FinancialRepository
    {
        public async Task<PLReportData> GetPLStatementAsync(DateTime startDate, DateTime endDate)
        {
            var data = new PLReportData();
            
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            {
                // 1. Total Revenue (Sales)
                string salesSql = "SELECT SUM(TotalAmount) FROM Sales WHERE Date BETWEEN @Start AND @End";
                using (var cmd = new SqlCommand(salesSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Start", startDate);
                    cmd.Parameters.AddWithValue("@End", endDate);
                    var val = await cmd.ExecuteScalarAsync();
                    data.TotalRevenue = val != DBNull.Value ? Convert.ToDecimal(val) : 0;
                }

                // 2. Cost of Goods Sold (COGS)
                string cogsSql = @"SELECT SUM(si.Quantity * p.CostPrice) 
                                 FROM SaleItems si 
                                 INNER JOIN Sales s ON si.SaleId = s.Id 
                                 INNER JOIN Products p ON si.ProductId = p.Id 
                                 WHERE s.Date BETWEEN @Start AND @End";
                using (var cmd = new SqlCommand(cogsSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Start", startDate);
                    cmd.Parameters.AddWithValue("@End", endDate);
                    var val = await cmd.ExecuteScalarAsync();
                    data.COGS = val != DBNull.Value ? Convert.ToDecimal(val) : 0;
                }

                // 3. Expenses by Category
                string expensesSql = @"SELECT Category, SUM(Amount) 
                                     FROM Expenses 
                                     WHERE Date BETWEEN @Start AND @End 
                                     GROUP BY Category";
                using (var cmd = new SqlCommand(expensesSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Start", startDate);
                    cmd.Parameters.AddWithValue("@End", endDate);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string cat = reader.IsDBNull(0) ? "General" : reader.GetString(0);
                            decimal amt = reader.GetDecimal(1);
                            data.ExpensesByCategory.Add(cat, amt);
                            data.TotalExpenses += amt;
                        }
                    }
                }

                // 4. Profit by Customer
                string customerProfitSql = @"
                    SELECT c.Name, SUM(si.Quantity * (si.SellPrice - p.CostPrice)) as Profit
                    FROM SaleItems si
                    INNER JOIN Sales s ON si.SaleId = s.Id
                    INNER JOIN Customers c ON s.CustomerId = c.Id
                    INNER JOIN Products p ON si.ProductId = p.Id
                    WHERE s.Date BETWEEN @Start AND @End
                    GROUP BY c.Name
                    ORDER BY Profit DESC";
                using (var cmd = new SqlCommand(customerProfitSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Start", startDate);
                    cmd.Parameters.AddWithValue("@End", endDate);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.ProfitByCustomer.Add(reader.GetString(0), reader.GetDecimal(1));
                        }
                    }
                }

                // 5. Profit by Product
                string productProfitSql = @"
                    SELECT p.Name, SUM(si.Quantity * (si.SellPrice - p.CostPrice)) as Profit
                    FROM SaleItems si
                    INNER JOIN Sales s ON si.SaleId = s.Id
                    INNER JOIN Products p ON si.ProductId = p.Id
                    WHERE s.Date BETWEEN @Start AND @End
                    GROUP BY p.Name
                    ORDER BY Profit DESC";
                using (var cmd = new SqlCommand(productProfitSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Start", startDate);
                    cmd.Parameters.AddWithValue("@End", endDate);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.ProfitByProduct.Add(reader.GetString(0), reader.GetDecimal(1));
                        }
                    }
                }
            }
            
            return data;
        }
    }

    public class PLReportData
    {
        public decimal TotalRevenue { get; set; }
        public decimal COGS { get; set; }
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> ProfitByCustomer { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> ProfitByProduct { get; set; } = new Dictionary<string, decimal>();
        public decimal TotalExpenses { get; set; }
        public decimal GrossProfit => TotalRevenue - COGS;
        public decimal NetProfit => GrossProfit - TotalExpenses;
    }
}
