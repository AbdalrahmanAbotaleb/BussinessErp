using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessErp.DAL;

namespace BussinessErp.BLL
{
    /// <summary>
    /// AI Analysis Engine — modular and extendable.
    /// Analyzes sales trends, inventory turnover, profit margins, category performance.
    /// </summary>
    public class AIAnalysisEngine
    {
        private readonly AIDataRepository _repo = new AIDataRepository();

        /// <summary>
        /// Analyze sales trends — returns monthly trend with growth rates.
        /// </summary>
        public async Task<List<SalesTrendPoint>> AnalyzeSalesTrendsAsync()
        {
            var monthly = await _repo.GetMonthlySalesHistoryAsync(18);
            var result = new List<SalesTrendPoint>();

            for (int i = 0; i < monthly.Count; i++)
            {
                var point = new SalesTrendPoint
                {
                    Year = monthly[i].Year,
                    Month = monthly[i].Month,
                    Total = monthly[i].Total,
                    GrowthRate = i > 0 && monthly[i - 1].Total > 0
                        ? ((monthly[i].Total - monthly[i - 1].Total) / monthly[i - 1].Total) * 100
                        : 0
                };
                result.Add(point);
            }
            return result;
        }

        /// <summary>
        /// Analyze inventory turnover — identifies fast and slow-moving products.
        /// </summary>
        public async Task<InventoryAnalysis> AnalyzeInventoryTurnoverAsync()
        {
            var data = await _repo.GetInventoryTurnoverAsync();
            var analysis = new InventoryAnalysis
            {
                FastMovers = data.Where(d => d.AvgMonthlySales > 10).Select(d => new ProductTurnover
                {
                    Name = d.Name,
                    CurrentStock = d.CurrentStock,
                    AvgMonthlySales = d.AvgMonthlySales,
                    MonthsOfStock = d.AvgMonthlySales > 0 ? (decimal)d.CurrentStock / d.AvgMonthlySales : 999
                }).ToList(),
                SlowMovers = data.Where(d => d.AvgMonthlySales <= 2 && d.CurrentStock > 0).Select(d => new ProductTurnover
                {
                    Name = d.Name,
                    CurrentStock = d.CurrentStock,
                    AvgMonthlySales = d.AvgMonthlySales,
                    MonthsOfStock = d.AvgMonthlySales > 0 ? (decimal)d.CurrentStock / d.AvgMonthlySales : 999
                }).ToList()
            };
            return analysis;
        }

        /// <summary>
        /// Analyze profit margins by product.
        /// </summary>
        public async Task<List<ProfitMarginInfo>> AnalyzeProfitMarginsAsync()
        {
            var data = await _repo.GetProfitMarginsAsync();
            return data.Select(d => new ProfitMarginInfo
            {
                Name = d.Name,
                Category = d.Category,
                CostPrice = d.CostPrice,
                SellPrice = d.SellPrice,
                MarginPct = d.MarginPct,
                UnitsSold = d.UnitsSold
            }).ToList();
        }

        /// <summary>
        /// Analyze category performance.
        /// </summary>
        public async Task<List<CategoryPerformance>> AnalyzeCategoryPerformanceAsync()
        {
            var data = await _repo.GetCategoryAnalysisAsync();
            decimal totalRevenue = data.Sum(d => d.TotalRevenue);

            return data.Select(d => new CategoryPerformance
            {
                Category = d.Category,
                Revenue = d.TotalRevenue,
                Profit = d.TotalProfit,
                UnitsSold = d.UnitsSold,
                SharePct = totalRevenue > 0 ? (d.TotalRevenue / totalRevenue) * 100 : 0
            }).ToList();
        }

        /// <summary>
        /// Analyze top customers within a given period.
        /// </summary>
        public async Task<List<TopCustomerInfo>> AnalyzeTopCustomersAsync(int days = 90, bool sortByRevenue = true)
        {
            var data = await _repo.GetTopCustomerDataAsync(days);
            decimal totalRevenue = data.Sum(d => d.TotalRevenue);

            var list = data.Select(d => new TopCustomerInfo
            {
                Name = d.Name,
                TotalRevenue = d.TotalRevenue,
                InvoiceCount = d.InvoiceCount,
                ContributionPct = totalRevenue > 0 ? (d.TotalRevenue / totalRevenue) * 100 : 0
            }).ToList();

            return sortByRevenue 
                ? list.OrderByDescending(l => l.TotalRevenue).ToList() 
                : list.OrderByDescending(l => l.InvoiceCount).ToList();
        }

        /// <summary>
        /// Analyze top suppliers within a given period.
        /// </summary>
        public async Task<List<TopSupplierInfo>> AnalyzeTopSuppliersAsync(int days = 90)
        {
            var data = await _repo.GetTopSupplierDataAsync(days);
            decimal totalSpend = data.Sum(d => d.TotalSpend);

            return data.Select(d => new TopSupplierInfo
            {
                Name = d.Name,
                TotalSpend = d.TotalSpend,
                OrderCount = d.OrderCount,
                ContributionPct = totalSpend > 0 ? (d.TotalSpend / totalSpend) * 100 : 0
            })
            .OrderByDescending(l => l.TotalSpend)
            .ToList();
        }
    }

    // ===== Analysis DTOs =====

    public class SalesTrendPoint
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Total { get; set; }
        public decimal GrowthRate { get; set; }
        public string Label => $"{Year}-{Month:D2}";
    }

    public class InventoryAnalysis
    {
        public List<ProductTurnover> FastMovers { get; set; } = new List<ProductTurnover>();
        public List<ProductTurnover> SlowMovers { get; set; } = new List<ProductTurnover>();
    }

    public class ProductTurnover
    {
        public string Name { get; set; }
        public int CurrentStock { get; set; }
        public decimal AvgMonthlySales { get; set; }
        public decimal MonthsOfStock { get; set; }
    }

    public class ProfitMarginInfo
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal MarginPct { get; set; }
        public int UnitsSold { get; set; }
    }

    public class CategoryPerformance
    {
        public string Category { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public int UnitsSold { get; set; }
        public decimal SharePct { get; set; }
    }

    public class TopCustomerInfo
    {
        public string Name { get; set; }
        public decimal TotalRevenue { get; set; }
        public int InvoiceCount { get; set; }
        public decimal AverageInvoiceValue => InvoiceCount > 0 ? TotalRevenue / InvoiceCount : 0;
        public decimal ContributionPct { get; set; }
    }

    public class TopSupplierInfo
    {
        public string Name { get; set; }
        public decimal TotalSpend { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue => OrderCount > 0 ? TotalSpend / OrderCount : 0;
        public decimal ContributionPct { get; set; }
    }
}
