using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BussinessErp.DAL;

namespace BussinessErp.BLL
{
    /// <summary>
    /// Business Advisor — rule-based suggestion engine.
    /// Modular: each suggestion type is a separate method that can be extended.
    /// </summary>
    public class BusinessAdvisor
    {
        private readonly AIDataRepository _repo = new AIDataRepository();
        private readonly AIAnalysisEngine _analysis = new AIAnalysisEngine();
        private readonly ForecastingEngine _forecasting = new ForecastingEngine();

        /// <summary>
        /// Generate all business suggestions.
        /// </summary>
        public async Task<List<BusinessSuggestion>> GenerateSuggestionsAsync()
        {
            var suggestions = new List<BusinessSuggestion>();

            suggestions.AddRange(await GenerateRestockSuggestionsAsync());
            suggestions.AddRange(await GenerateSlowMoverSuggestionsAsync());
            suggestions.AddRange(await GeneratePricingSuggestionsAsync());
            suggestions.AddRange(await GenerateTrendSuggestionsAsync());

            return suggestions.OrderByDescending(s => s.Priority).ToList();
        }

        /// <summary>
        /// Restock alerts for high-demand products running low.
        /// </summary>
        public async Task<List<BusinessSuggestion>> GenerateRestockSuggestionsAsync()
        {
            var stockOuts = await _forecasting.PredictStockOutsAsync();
            return stockOuts.Where(s => s.Urgency != "Monitor").Select(s => new BusinessSuggestion
            {
                Category = "Restock",
                Priority = s.Urgency == "Critical" ? 10 : 7,
                Icon = "⚠️",
                Title = $"Restock: {s.ProductName}",
                Description = $"Only {s.CurrentStock} units left. Estimated {s.WeeksUntilStockOut:F0} weeks until stock-out.",
                Action = $"Order more {s.ProductName} immediately."
            }).ToList();
        }

        /// <summary>
        /// Identify slow-moving items and suggest actions.
        /// </summary>
        public async Task<List<BusinessSuggestion>> GenerateSlowMoverSuggestionsAsync()
        {
            var slowMovers = await _repo.GetSlowMoversAsync();
            return slowMovers.Where(s => s.UnitsSold3M < 5 && s.Stock > 20).Take(5).Select(s => new BusinessSuggestion
            {
                Category = "Slow Movers",
                Priority = 5,
                Icon = "📉",
                Title = $"Slow Mover: {s.Name}",
                Description = $"{s.Stock} units in stock but only {s.UnitsSold3M} sold in 3 months ({s.Category}).",
                Action = "Consider discounting, bundling, or reducing reorder level."
            }).ToList();
        }

        /// <summary>
        /// Pricing optimization suggestions.
        /// </summary>
        public async Task<List<BusinessSuggestion>> GeneratePricingSuggestionsAsync()
        {
            var margins = await _analysis.AnalyzeProfitMarginsAsync();
            var suggestions = new List<BusinessSuggestion>();

            // High-volume, low-margin products
            var lowmarginHighVol = margins.Where(m => m.MarginPct < 20 && m.UnitsSold > 50).Take(3);
            foreach (var item in lowmarginHighVol)
            {
                suggestions.Add(new BusinessSuggestion
                {
                    Category = "Pricing",
                    Priority = 6,
                    Icon = "💰",
                    Title = $"Price Review: {item.Name}",
                    Description = $"Margin is only {item.MarginPct:F1}% but {item.UnitsSold} units sold. Consider a small price increase.",
                    Action = $"Current: {item.SellPrice:C} → Suggested: {item.SellPrice * 1.05m:C} (+5%)"
                });
            }

            // Very high-margin, low-volume products
            var highMarginLowVol = margins.Where(m => m.MarginPct > 60 && m.UnitsSold < 10).Take(3);
            foreach (var item in highMarginLowVol)
            {
                suggestions.Add(new BusinessSuggestion
                {
                    Category = "Pricing",
                    Priority = 4,
                    Icon = "📊",
                    Title = $"Volume Opportunity: {item.Name}",
                    Description = $"High margin ({item.MarginPct:F1}%) but only {item.UnitsSold} units sold.",
                    Action = "Consider a promotional price to drive volume."
                });
            }

            return suggestions;
        }

        /// <summary>
        /// Trend-based suggestions.
        /// </summary>
        public async Task<List<BusinessSuggestion>> GenerateTrendSuggestionsAsync()
        {
            var suggestions = new List<BusinessSuggestion>();
            var forecast = await _forecasting.PredictNextMonthSalesAsync();

            if (forecast.Trend == "Downward")
            {
                suggestions.Add(new BusinessSuggestion
                {
                    Category = "Trend",
                    Priority = 8,
                    Icon = "📉",
                    Title = "Declining Sales Trend",
                    Description = $"Sales are trending downward. Next month forecast: {forecast.PredictedAmount:N0} ({forecast.Confidence} confidence).",
                    Action = "Review marketing strategy, consider promotions or seasonal campaigns."
                });
            }
            else if (forecast.Trend == "Upward")
            {
                suggestions.Add(new BusinessSuggestion
                {
                    Category = "Trend",
                    Priority = 3,
                    Icon = "📈",
                    Title = "Growing Sales Trend",
                    Description = $"Sales are trending upward! Next month forecast: {forecast.PredictedAmount:N0} ({forecast.Confidence} confidence).",
                    Action = "Ensure adequate stock levels to meet growing demand."
                });
            }

            return suggestions;
        }

        /// <summary>
        /// Generate insights about top business partners.
        /// </summary>
        public async Task<List<BusinessSuggestion>> GeneratePartnerInsightsAsync(int days = 90)
        {
            var insights = new List<BusinessSuggestion>();
            
            var topCustomers = await _analysis.AnalyzeTopCustomersAsync(days);
            if (topCustomers.Any())
            {
                var top = topCustomers.First();
                if (top.ContributionPct > 20)
                {
                    insights.Add(new BusinessSuggestion
                    {
                        Category = "Partner",
                        Priority = 9,
                        Icon = "⭐",
                        Title = "Key Client Concentration",
                        Description = $"{top.Name} contributes {top.ContributionPct:F1}% of total revenue in the last {days} days.",
                        Action = "Review relationship health and ensure retention strategy."
                    });
                }
            }

            var topSuppliers = await _analysis.AnalyzeTopSuppliersAsync(days);
            if (topSuppliers.Any())
            {
                var top = topSuppliers.First();
                if (top.ContributionPct > 30)
                {
                    insights.Add(new BusinessSuggestion
                    {
                        Category = "Partner",
                        Priority = 8,
                        Icon = "🚚",
                        Title = "Supplier Dependency",
                        Description = $"{top.Name} accounts for {top.ContributionPct:F1}% of total purchasing volume.",
                        Action = "Consider diversifying suppliers to mitigate supply chain risk."
                    });
                }
            }

            return insights;
        }

        /// <summary>
        /// Answer dynamic business questions using Gemini LLM reasoning.
        /// </summary>
        public async Task<string> AnswerQuestionAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "Please ask a business question.";

            string q = question.ToLower().Trim();

            // Check if it's a simple pattern-matched question first for speed
            string simpleAnswer = await Task.Run(() => GetSimpleAnswer(q));
            if (simpleAnswer != null) return simpleAnswer;

            // Deep Reasoning via Gemini
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["GeminiApiKey"];
            var gemini = new GeminiService(apiKey);
            var context = new BusinessContext
            {
                TodaySales = await _repo.GetTodaysSalesAsync(),
                TopCustomers = (await _analysis.AnalyzeTopCustomersAsync(90)).Cast<dynamic>().ToList(),
                TopSuppliers = (await _analysis.AnalyzeTopSuppliersAsync(90)).Cast<dynamic>().ToList(),
                SlowMovers = (await _repo.GetSlowMoversAsync()).Cast<dynamic>().ToList(),
                SalesForecast = (await _forecasting.PredictNextMonthSalesAsync()).PredictedAmount.ToString("C")
            };

            return await gemini.GetReasoningAsync(question, context);
        }

        private async Task<string> GetSimpleAnswer(string q)
        {
            // Top Customers
            if (q.Contains("top") && q.Contains("customer"))
            {
                var top = await _analysis.AnalyzeTopCustomersAsync(90);
                if (!top.Any()) return "No customer data available for analysis.";

                string result = "🤝 Top Customers (Last 90 Days):\n\n";
                foreach (var c in top.Take(5))
                {
                    result += $"• {c.Name}: {c.TotalRevenue:C} ({c.InvoiceCount} invoices, Avg: {c.AverageInvoiceValue:C})\n" +
                             $"  Contribution: {c.ContributionPct:F1}%\n";
                }
                return result;
            }

            // Top Suppliers
            if (q.Contains("top") && (q.Contains("supplier") || q.Contains("vendor")))
            {
                var top = await _analysis.AnalyzeTopSuppliersAsync(90);
                if (!top.Any()) return "No supplier data available for analysis.";

                string result = "🏢 Top Suppliers (Last 90 Days):\n\n";
                foreach (var s in top.Take(5))
                {
                    result += $"• {s.Name}: {s.TotalSpend:C} ({s.OrderCount} orders, Avg: {s.AverageOrderValue:C})\n" +
                             $"  Volume Share: {s.ContributionPct:F1}%\n";
                }
                return result;
            }

            // Today's sales
            if (q.Contains("today") && q.Contains("sales"))
            {
                decimal today = await _repo.GetTodaysSalesAsync();
                return $"📊 Today's total sales: {today:C}\n\nThis includes all transactions recorded for {DateTime.Today:MMMM dd, yyyy}.";
            }

            // Highest revenue month
            if ((q.Contains("highest") || q.Contains("best") || q.Contains("top")) && (q.Contains("month") || q.Contains("revenue")))
            {
                var best = await _repo.GetBestMonthAsync();
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(best.Month);
                return $"🏆 Best month: {monthName} {best.Year}\nTotal Revenue: {best.Total:C}\n\nThis was the highest-grossing month in the recorded history.";
            }

            // Highest profit margin product
            if (q.Contains("profit") && q.Contains("margin") && q.Contains("product"))
            {
                var top = await _repo.GetHighestMarginProductAsync();
                return $"💰 Highest profit margin product: {top.Name}\nMargin: {top.Margin:F1}%\n\nThis product generates the highest percentage profit per unit sold.";
            }

            // Restock / what to order
            if (q.Contains("restock") || q.Contains("order") || q.Contains("stock") || q.Contains("run out"))
            {
                var predictions = await _forecasting.PredictStockOutsAsync();
                if (predictions.Count == 0)
                    return "✅ All products have adequate stock levels for the next 8 weeks.";

                string result = "📦 Products needing restock:\n\n";
                foreach (var p in predictions.Take(10))
                    result += $"• {p.ProductName} — {p.CurrentStock} units left, ~{p.WeeksUntilStockOut:F0} weeks [{p.Urgency}]\n";
                return result;
            }

            // Forecast / prediction
            if (q.Contains("forecast") || q.Contains("predict") || q.Contains("next month"))
            {
                var forecast = await _forecasting.PredictNextMonthSalesAsync();
                return $"🔮 Sales Forecast:\n\nPredicted next month: {forecast.PredictedAmount:C}\nTrend: {forecast.Trend}\nConfidence: {forecast.Confidence}\nR²: {forecast.RSquared:F3}\nHistorical Avg: {forecast.HistoricalAvg:C}";
            }

            // Slow movers
            if (q.Contains("slow") || q.Contains("not selling") || q.Contains("dead stock"))
            {
                var slowMovers = await _repo.GetSlowMoversAsync();
                if (slowMovers.Count == 0)
                    return "All products have reasonable sales velocity.";

                string result = "🐌 Slow-moving products (last 3 months):\n\n";
                foreach (var s in slowMovers.Take(10))
                    result += $"• {s.Name} ({s.Category}) — {s.Stock} in stock, {s.UnitsSold3M} sold\n";
                return result;
            }

            // Category performance
            if (q.Contains("category") || q.Contains("categories"))
            {
                var categories = await _analysis.AnalyzeCategoryPerformanceAsync();
                string result = "📊 Category Performance:\n\n";
                foreach (var c in categories)
                    result += $"• {c.Category}: {c.Revenue:C} revenue, {c.Profit:C} profit, {c.UnitsSold} units ({c.SharePct:F1}% share)\n";
                return result;
            }

            // Suggestions
            if (q.Contains("suggest") || q.Contains("advice") || q.Contains("recommend"))
            {
                var suggestions = await GenerateSuggestionsAsync();
                suggestions.AddRange(await GeneratePartnerInsightsAsync());
                
                if (suggestions.Count == 0)
                    return "No immediate suggestions at this time. Business is operating within normal parameters.";

                string result = "💡 Business Suggestions:\n\n";
                foreach (var s in suggestions.OrderByDescending(x => x.Priority).Take(8))
                    result += $"{s.Icon} [{s.Category}] {s.Title}\n  {s.Description}\n  → {s.Action}\n\n";
                return result;
            }

            return "🤖 I can answer questions about:\n" +
                   "• \"Who are our top customers?\"\n" +
                   "• \"Who are our best suppliers?\"\n" +
                   "• \"What are today's total sales?\"\n" +
                   "• \"Which month had highest revenue?\"\n" +
                   "• \"Which product has highest profit margin?\"\n" +
                   "• \"What should we restock next week?\"\n" +
                   "• \"What's the sales forecast for next month?\"\n" +
                   "• \"Which products are not selling?\"\n" +
                   "• \"How are categories performing?\"\n" +
                   "• \"Give me business suggestions\"\n\n" +
                   "Try asking one of these questions!";
        }
    }

    public class BusinessSuggestion
    {
        public string Category { get; set; }
        public int Priority { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
    }
}
