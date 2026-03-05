using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;

namespace BussinessErp.BLL
{
    public class DashboardService
    {
        private readonly DashboardRepository _repo = new DashboardRepository();

        public Task<(int TransactionCount, decimal TotalSales)> GetDailySalesAsync(DateTime? date = null)
        {
            RoleGuard.RequiresManager("View Daily Sales");
            return _repo.GetDailySalesAsync(date);
        }

        public Task<(decimal Revenue, decimal COGS, decimal Expenses, decimal NetProfit)> GetMonthlyProfitAsync(int? year = null, int? month = null)
        {
            RoleGuard.RequiresManager("View Monthly Profit");
            return _repo.GetMonthlyProfitAsync(year, month);
        }

        public Task<int> GetLowStockCountAsync() => _repo.GetLowStockCountAsync();

        public Task<List<(string Month, decimal Total)>> GetMonthlySalesTrendAsync(int months = 12)
        {
            RoleGuard.RequiresManager("View Sales Trend");
            return _repo.GetMonthlySalesTrendAsync(months);
        }

        public Task<List<(string Month, decimal Revenue, decimal COGS, decimal Profit)>> GetMonthlyProfitTrendAsync(int months = 12)
        {
            RoleGuard.RequiresManager("View Profit Trend");
            return _repo.GetMonthlyProfitTrendAsync(months);
        }

        public Task<List<(string Category, decimal Total)>> GetCategoryPerformanceAsync()
            => _repo.GetCategoryPerformanceAsync();

        public Task<List<(string Name, int UnitsSold, decimal Revenue)>> GetTopProductsAsync(int topN = 5)
            => _repo.GetTopProductsAsync(topN);
    }
}
