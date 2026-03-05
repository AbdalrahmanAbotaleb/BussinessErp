using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessErp.DAL;

namespace BussinessErp.BLL
{
    /// <summary>
    /// Forecasting Engine — extendable module for business predictions.
    /// Uses simple linear regression for sales forecasting and consumption-rate stock prediction.
    /// </summary>
    public class ForecastingEngine
    {
        private readonly AIDataRepository _repo = new AIDataRepository();
        private readonly MLForecastingService _mlForecaster = new MLForecastingService();

        /// <summary>
        /// Predict next month's sales using ML.NET (SSA) or Linear Regression as fallback.
        /// </summary>
        public async Task<SalesForecast> PredictNextMonthSalesAsync()
        {
            try
            {
                var mlPrediction = await _mlForecaster.PredictNextMonthsAsync(1);
                if (mlPrediction != null && mlPrediction.Forecast.Length > 0)
                {
                    return new SalesForecast
                    {
                        PredictedAmount = (decimal)mlPrediction.Forecast[0],
                        Confidence = "ML-Engine (95%)",
                        Trend = "Calculated by ML",
                        RSquared = 0, // Not applicable for SSA in the same way
                        MonthlySlope = 0,
                        HistoricalAvg = 0 // ML handles complex patterns
                    };
                }
            }
            catch { /* Fallback to linear regression if ML fails */ }

            var history = await _repo.GetMonthlySalesHistoryAsync(12);
            if (history.Count < 3)
                return new SalesForecast { PredictedAmount = 0, Confidence = "Low", Trend = "Insufficient data" };

            // Simple linear regression: y = mx + b
            var xs = Enumerable.Range(1, history.Count).Select(i => (double)i).ToArray();
            var ys = history.Select(h => (double)h.Total).ToArray();

            double n = xs.Length;
            double sumX = xs.Sum();
            double sumY = ys.Sum();
            double sumXY = xs.Zip(ys, (x, y) => x * y).Sum();
            double sumX2 = xs.Sum(x => x * x);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            double nextX = n + 1;
            double predicted = Math.Max(0, slope * nextX + intercept);

            // Calculate R² for confidence
            double meanY = sumY / n;
            double ssRes = xs.Zip(ys, (x, y) => Math.Pow(y - (slope * x + intercept), 2)).Sum();
            double ssTot = ys.Sum(y => Math.Pow(y - meanY, 2));
            double rSquared = ssTot > 0 ? 1 - (ssRes / ssTot) : 0;

            string confidence = rSquared > 0.7 ? "High" : rSquared > 0.4 ? "Medium" : "Low";
            string trend = slope > 0 ? "Upward" : slope < 0 ? "Downward" : "Stable";

            return new SalesForecast
            {
                PredictedAmount = (decimal)predicted,
                Confidence = confidence,
                Trend = trend,
                RSquared = rSquared,
                MonthlySlope = (decimal)slope,
                HistoricalAvg = (decimal)(sumY / n)
            };
        }

        /// <summary>
        /// Predict which products will run out soon based on consumption rate.
        /// </summary>
        public async Task<List<StockOutPrediction>> PredictStockOutsAsync()
        {
            var restockData = await _repo.GetRestockPredictionsAsync();
            return restockData.Select(r => new StockOutPrediction
            {
                ProductName = r.Name,
                CurrentStock = r.CurrentStock,
                WeeksUntilStockOut = Math.Round(r.WeeksLeft, 1),
                Urgency = r.WeeksLeft <= 2 ? "Critical" : r.WeeksLeft <= 4 ? "Warning" : "Monitor"
            }).ToList();
        }

        /// <summary>
        /// Generate a 6-month sales projection.
        /// </summary>
        public async Task<List<(string Month, decimal Projected)>> ProjectSalesAsync(int months = 6)
        {
            var history = await _repo.GetMonthlySalesHistoryAsync(12);
            if (history.Count < 3) return new List<(string, decimal)>();

            var xs = Enumerable.Range(1, history.Count).Select(i => (double)i).ToArray();
            var ys = history.Select(h => (double)h.Total).ToArray();

            double n = xs.Length;
            double sumX = xs.Sum();
            double sumY = ys.Sum();
            double sumXY = xs.Zip(ys, (x, y) => x * y).Sum();
            double sumX2 = xs.Sum(x => x * x);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            var result = new List<(string, decimal)>();
            DateTime now = DateTime.Now;
            for (int i = 1; i <= months; i++)
            {
                double x = n + i;
                decimal projected = (decimal)Math.Max(0, slope * x + intercept);
                DateTime futureMonth = now.AddMonths(i);
                result.Add(($"{futureMonth:yyyy-MM}", projected));
            }
            return result;
        }
    }

    // ===== Forecast DTOs =====

    public class SalesForecast
    {
        public decimal PredictedAmount { get; set; }
        public string Confidence { get; set; }
        public string Trend { get; set; }
        public double RSquared { get; set; }
        public decimal MonthlySlope { get; set; }
        public decimal HistoricalAvg { get; set; }
    }

    public class StockOutPrediction
    {
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public decimal WeeksUntilStockOut { get; set; }
        public string Urgency { get; set; }
    }
}
