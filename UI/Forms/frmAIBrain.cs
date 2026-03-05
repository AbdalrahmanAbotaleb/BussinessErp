using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BussinessErp.BLL;
using BussinessErp.Helpers;

namespace BussinessErp.UI.Forms
{
    public class frmAIBrain : Form
    {
        private RichTextBox rtbOutput;
        private Button btnForecast, btnSuggestions, btnAnalyze;
        private ComboBox cmbPeriod;
        private FlowLayoutPanel panelQuickActions;
        private Panel panelChat;
        private Chart chartForecast;

        private readonly BusinessAdvisor _advisor = new BusinessAdvisor();
        private readonly ForecastingEngine _forecasting = new ForecastingEngine();
        private readonly AIAnalysisEngine _analysis = new AIAnalysisEngine();

        public frmAIBrain() 
        { 
            InitializeComponent(); 
            LanguageManager.ApplyRTL(this);
        }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_ai_brain");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            // Title
            var lblTitle = new Label { Text = LanguageManager.Get("lbl_aibrain_title"), Font = UIHelper.FontTitle, ForeColor = UIHelper.PrimaryTeal, Dock = DockStyle.Top, Height = 45 };


            // Quick action buttons
            panelQuickActions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(0, 8, 0, 8), WrapContents = false, AutoScroll = true };

            btnForecast = CreateQuickButton(LanguageManager.Get("btn_forecast"), UIHelper.PrimaryTeal);
            btnSuggestions = CreateQuickButton(LanguageManager.Get("btn_suggestions"), UIHelper.AccentOrange);
            btnAnalyze = CreateQuickButton(LanguageManager.Get("btn_category_analysis"), UIHelper.AccentBlue);
            var btnDeepReasoning = CreateQuickButton(LanguageManager.Get("btn_deep_reasoning"), Color.FromArgb(0, 150, 136));
            var btnStockOut = CreateQuickButton(LanguageManager.Get("btn_restock_alert"), UIHelper.AccentRed);
            var btnSlowMovers = CreateQuickButton(LanguageManager.Get("btn_slow_movers"), Color.FromArgb(156, 39, 176));
            var btnTopPartners = CreateQuickButton(LanguageManager.Get("btn_top_partners"), Color.FromArgb(63, 81, 181));
            
            cmbPeriod = new ComboBox 
            { 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Width = 120,
                Margin = new Padding(10, 5, 0, 0)
            };
            cmbPeriod.Items.AddRange(new object[] { 
                LanguageManager.Get("period_30"), 
                LanguageManager.Get("period_90"), 
                LanguageManager.Get("period_365") 
            });
            cmbPeriod.SelectedIndex = 1; // 3 months default

            panelQuickActions.Controls.AddRange(new Control[] { btnForecast, btnSuggestions, btnAnalyze, btnDeepReasoning, btnStockOut, btnSlowMovers, btnTopPartners });
            panelQuickActions.Controls.Add(new Label { Text = LanguageManager.Get("lbl_period"), Margin = new Padding(20, 10, 0, 0), Font = UIHelper.FontBody });
            panelQuickActions.Controls.Add(cmbPeriod);

            // Main split: chat output + forecast chart
            var splitMain = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 600 };



            // Chat panel
            panelChat = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.CardBg, Padding = new Padding(10) };

            rtbOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 25, 30),
                ForeColor = Color.FromArgb(220, 225, 230),
                Font = new Font("Cascadia Code", 10.5F, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            panelChat.Controls.Add(rtbOutput);


            // Forecast chart panel
            var panelRight = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.CardBg, Padding = new Padding(10) };
            var lblChart = new Label { Text = LanguageManager.Get("lbl_sales_projection"), Font = UIHelper.FontSubtitle, ForeColor = UIHelper.PrimaryTeal, Dock = DockStyle.Top, Height = 30 };
            chartForecast = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.CardBg,
                MinimumSize = new Size(100, 100)
            };

            var area = new ChartArea("Forecast")
            {
                BackColor = UIHelper.CardBg,
                AxisX = { LabelStyle = { Font = UIHelper.FontSmall }, MajorGrid = { LineColor = Color.FromArgb(230, 230, 230) } },
                AxisY = { LabelStyle = { Font = UIHelper.FontSmall, Format = "C0" }, MajorGrid = { LineColor = Color.FromArgb(230, 230, 230) } }
            };
            chartForecast.ChartAreas.Add(area);
            panelRight.Controls.Add(chartForecast);
            panelRight.Controls.Add(lblChart);

            splitMain.Panel1.Controls.Add(panelChat);
            splitMain.Panel2.Controls.Add(panelRight);

            // Container for scrolling
            var pnlContainer = new Panel { Location = new Point(0,0), Name = "pnlContainer" };
            // Adding in specific order: Top-most (visually) last in collection for Fill to work correctly
            pnlContainer.Controls.Add(splitMain);
            pnlContainer.Controls.Add(panelQuickActions);
            pnlContainer.Controls.Add(lblTitle);

            this.Controls.Add(pnlContainer);
            this.AutoScroll = true;
            this.AutoScrollMinSize = new Size(1200, 300); // Back up enforcement


            this.Resize += (s, e) => {
                if (this.WindowState == FormWindowState.Minimized) return;
                pnlContainer.Width = Math.Max(1200, this.ClientSize.Width);
                pnlContainer.Height = Math.Max(300, this.ClientSize.Height);
            };

            // Initial sizing
            pnlContainer.Width = Math.Max(1200, this.ClientSize.Width);
            pnlContainer.Height = Math.Max(300, this.ClientSize.Height);




            // ===== EVENT WIRING =====


            btnForecast.Click += async (s, e) => {
                AppendOutput(LanguageManager.Get("msg_generating_forecast"), UIHelper.PrimaryTealLight);
                var forecast = await _forecasting.PredictNextMonthSalesAsync();
                string result = LanguageManager.Get("lbl_sales_forecast_title") +
                                string.Format(LanguageManager.Get("lbl_predicted_next_month"), forecast.PredictedAmount.ToString("C")) +
                                string.Format(LanguageManager.Get("lbl_trend"), forecast.Trend, forecast.Confidence) +
                                $"  R²: {forecast.RSquared:F3} | Monthly Slope: {forecast.MonthlySlope:C}\n" +
                                string.Format(LanguageManager.Get("lbl_historical_avg"), forecast.HistoricalAvg.ToString("C"));
                AppendOutput(result, Color.FromArgb(100, 220, 180));

                // Update chart
                var projections = await _forecasting.ProjectSalesAsync(6);
                if (this.IsDisposed) return;

                if (chartForecast.Series.IndexOf("Projected Sales") == -1)
                {
                    chartForecast.Series.Add(new Series("Projected Sales") 
                    { 
                        ChartType = SeriesChartType.Column, 
                        Color = UIHelper.PrimaryTeal,
                        ChartArea = "Forecast"
                    });
                }

                var series = chartForecast.Series["Projected Sales"];
                series.Points.Clear();
                foreach (var p in projections)
                    series.Points.AddXY(p.Month, (double)p.Projected);
            };

            btnSuggestions.Click += async (s, e) => {
                AppendOutput(LanguageManager.Get("msg_generating_suggestions"), UIHelper.AccentOrange);
                var suggestions = await _advisor.GenerateSuggestionsAsync();
                if (suggestions.Count == 0) { AppendOutput(LanguageManager.Get("msg_no_suggestions"), UIHelper.AccentGreen); return; }
                foreach (var sg in suggestions)
                {
                    AppendOutput($"\n{sg.Icon} [{sg.Category}] {sg.Title}\n", Color.White);
                    AppendOutput($"   {sg.Description}\n", Color.FromArgb(180, 190, 200));
                    AppendOutput($"   → {sg.Action}\n", UIHelper.AccentGreen);
                }
            };

            btnAnalyze.Click += async (s, e) => {
                AppendOutput(LanguageManager.Get("msg_analyzing_categories"), UIHelper.AccentBlue);
                var cats = await _analysis.AnalyzeCategoryPerformanceAsync();
                foreach (var c in cats)
                    AppendOutput($"  {c.Category}: {c.Revenue:C} revenue, {c.Profit:C} profit, {c.UnitsSold:N0} units ({c.SharePct:F1}%)\n", Color.FromArgb(180, 200, 220));
            };

            btnDeepReasoning.Click += async (s, e) => {
                AppendOutput(LanguageManager.Get("msg_initiating_deep_reasoning"), Color.FromArgb(0, 200, 180));
                AppendOutput(LanguageManager.Get("msg_analyzing_market"), Color.FromArgb(150, 160, 170));
                
                string question = "Perform a complete audit of my business state and suggest the top 3 strategic moves I should make this month to maximize profit and efficiency.";
                string answer = await _advisor.AnswerQuestionAsync(question);
                
                AppendOutput($"\n{answer}\n\n", Color.FromArgb(220, 225, 230));
                AppendOutput("─────────────────────────────────────────────────────\n\n", Color.FromArgb(60, 70, 80));
            };

            btnStockOut.Click += async (s, e) => {
                AppendOutput(LanguageManager.Get("msg_checking_restock"), UIHelper.AccentRed);
                var answer = await _advisor.AnswerQuestionAsync("What should we restock?");
                AppendOutput(answer + "\n", Color.FromArgb(255, 180, 100));
            };

            btnSlowMovers.Click += async (s, e) => {
                AppendOutput(LanguageManager.Get("msg_finding_slow_movers"), Color.FromArgb(156, 39, 176));
                var answer = await _advisor.AnswerQuestionAsync("Which products are not selling?");
                AppendOutput(answer + "\n", Color.FromArgb(200, 160, 220));
            };

            btnTopPartners.Click += async (s, e) => {
                AppendOutput(LanguageManager.Get("msg_analyzing_partners"), Color.FromArgb(63, 81, 181));
                
                int days = cmbPeriod.SelectedIndex == 0 ? 30 : cmbPeriod.SelectedIndex == 1 ? 90 : 365; 
                
                var customers = await _analysis.AnalyzeTopCustomersAsync(days);
                var suppliers = await _analysis.AnalyzeTopSuppliersAsync(days);

                AppendOutput(string.Format(LanguageManager.Get("lbl_top_customers"), days), Color.White);
                foreach (var c in customers.Take(5))
                {
                    AppendOutput($"   • {c.Name}: {c.TotalRevenue:C} ({c.ContributionPct:F1}% share)\n", Color.FromArgb(100, 220, 255));
                }

                AppendOutput(string.Format(LanguageManager.Get("lbl_top_suppliers"), days), Color.White);
                foreach (var sup in suppliers.Take(5))
                {
                    AppendOutput($"   • {sup.Name}: {sup.TotalSpend:C} ({sup.ContributionPct:F1}% share)\n", UIHelper.AccentOrange);
                }

                // Insights
                var insights = await _advisor.GeneratePartnerInsightsAsync(days);
                if (insights.Any())
                {
                    AppendOutput(LanguageManager.Get("lbl_partner_insights"), UIHelper.AccentGreen);
                    foreach (var insight in insights)
                    {
                        AppendOutput($"   {insight.Icon} {insight.Description}\n   → {insight.Action}\n", Color.FromArgb(180, 190, 200));
                    }
                }

                // Update Chart with Top Customers revenue share
                if (this.IsDisposed) return;
                
                chartForecast.Series.Clear();
                var series = new Series("Revenue Share") 
                { 
                    ChartType = SeriesChartType.Pie,
                    ChartArea = "Forecast"
                };
                chartForecast.Series.Add(series);
                
                foreach(var c in customers.Take(5))
                {
                    var point = series.Points.Add((double)c.TotalRevenue);
                    point.Label = $"{c.Name} ({c.ContributionPct:F0}%)";
                    point.LegendText = c.Name;
                }
                
                AppendOutput("\n─────────────────────────────────────────────────────\n\n", Color.FromArgb(60, 70, 80));
            };

            // Welcome message
            this.Load += (s, e) => {
                AppendOutput(LanguageManager.Get("msg_welcome_aibrain"), UIHelper.PrimaryTealLight);
                AppendOutput(LanguageManager.Get("msg_aibrain_desc"), Color.FromArgb(180, 190, 200));
                AppendOutput(LanguageManager.Get("msg_quick_actions"), Color.FromArgb(140, 150, 160));

                AppendOutput("─────────────────────────────────────────────────────\n\n", Color.FromArgb(60, 70, 80));
            };
        }

        private Button CreateQuickButton(string text, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(180, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void AppendOutput(string text, Color color)

        {
            rtbOutput.SelectionStart = rtbOutput.TextLength;
            rtbOutput.SelectionLength = 0;
            rtbOutput.SelectionColor = color;
            rtbOutput.AppendText(text);
            rtbOutput.ScrollToCaret();
        }
    }
}
