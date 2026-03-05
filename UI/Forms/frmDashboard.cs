using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BussinessErp.BLL;
using BussinessErp.Helpers;

namespace BussinessErp.UI.Forms
{
    public class frmDashboard : Form
    {
        private FlowLayoutPanel panelKPIs;
        private Chart chartSalesTrend, chartProfit, chartCategory;
        private DataGridView dgvLowStock, dgvTopProducts;
        private Panel panelBottom;
        private TableLayoutPanel tableCharts;

        public frmDashboard()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_dashboard");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.AutoScroll = true;
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            // Title
            var lblTitle = new Label
            {
                Text = LanguageManager.Get("nav_dashboard"),
                Font = UIHelper.FontTitle,
                ForeColor = UIHelper.TextDark,
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 5, 0, 5)
            };

            // KPI Cards row
            panelKPIs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 130,
                AutoScroll = false,
                WrapContents = false,
                Padding = new Padding(0)
            };

            // Charts panel
            tableCharts = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 350,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 10)
            };
            tableCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tableCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tableCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            chartSalesTrend = CreateChart(LanguageManager.Get("dash_sales_trend"));
            chartSalesTrend.Dock = DockStyle.Fill;

            chartProfit = CreateChart(LanguageManager.Get("dash_profit_analysis"));
            chartProfit.Dock = DockStyle.Fill;

            chartCategory = CreateChart(LanguageManager.Get("dash_cat_perf"));
            chartCategory.Dock = DockStyle.Fill;

            tableCharts.Controls.Add(chartSalesTrend, 0, 0);
            tableCharts.Controls.Add(chartProfit, 1, 0);
            tableCharts.Controls.Add(chartCategory, 2, 0);

            // Bottom panel — Low Stock + Top Products
            panelBottom = new Panel
            {
                Dock = DockStyle.Top,
                Height = 280,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Low Stock Panel
            var panelLowStock = new Panel
            {
                Dock = DockStyle.Left,
                Width = 500,
                Padding = new Padding(0, 0, 10, 0)
            };
            var lblLow = new Label { Text = LanguageManager.Get("dash_low_stock_alert"), Font = UIHelper.FontSubtitle, ForeColor = UIHelper.AccentRed, Dock = DockStyle.Top, Height = 30 };
            dgvLowStock = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvLowStock);
            panelLowStock.Controls.Add(dgvLowStock);
            panelLowStock.Controls.Add(lblLow);

            // Top Products Panel
            var panelTop = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 0, 0)
            };
            var lblTop = new Label { Text = LanguageManager.Get("dash_top_products"), Font = UIHelper.FontSubtitle, ForeColor = UIHelper.PrimaryTeal, Dock = DockStyle.Top, Height = 30 };
            dgvTopProducts = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvTopProducts);
            panelTop.Controls.Add(dgvTopProducts);
            panelTop.Controls.Add(lblTop);

            panelBottom.Controls.Add(panelTop);
            panelBottom.Controls.Add(panelLowStock);

            this.Controls.Add(panelBottom);
            this.Controls.Add(tableCharts);
            this.Controls.Add(panelKPIs);
            this.Controls.Add(lblTitle);

            this.Load += async (s, e) => await LoadDashboardDataAsync();
        }

        private Chart CreateChart(string title)
        {
            var chart = new Chart
            {
                BackColor = UIHelper.CardBg,
                Padding = new Padding(8)
            };
            var area = new ChartArea("Main")
            {
                BackColor = UIHelper.CardBg,
                AxisX = { LabelStyle = { Font = UIHelper.FontSmall, ForeColor = UIHelper.TextMuted }, LineColor = UIHelper.CardBorder, MajorGrid = { LineColor = Color.FromArgb(240, 240, 240) } },
                AxisY = { LabelStyle = { Font = UIHelper.FontSmall, ForeColor = UIHelper.TextMuted }, LineColor = UIHelper.CardBorder, MajorGrid = { LineColor = Color.FromArgb(240, 240, 240) } }
            };
            chart.ChartAreas.Add(area);
            chart.Titles.Add(new Title(title) { Font = UIHelper.FontBodyBold, ForeColor = UIHelper.TextDark });
            return chart;
        }

        private async System.Threading.Tasks.Task LoadDashboardDataAsync()
        {
            try
            {
                var svc = new DashboardService();
                var productSvc = new ProductService();

                // Launch separate tasks for each dashboard section for incremental updates
                var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>
                {
                    LoadKPIsAsync(svc),
                    LoadSalesTrendAsync(svc),
                    LoadProfitTrendAsync(svc),
                    LoadCategoryPerformanceAsync(svc),
                    LoadLowStockGridAsync(productSvc),
                    LoadTopProductsGridAsync(svc)
                };

                await System.Threading.Tasks.Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Dashboard load error", ex);
            }
        }

        private async System.Threading.Tasks.Task LoadKPIsAsync(DashboardService svc)
        {
            try
            {
                var dailyTask = svc.GetDailySalesAsync();
                var monthlyTask = svc.GetMonthlyProfitAsync();
                var lowStockCountTask = svc.GetLowStockCountAsync();

                await System.Threading.Tasks.Task.WhenAll(dailyTask, monthlyTask, lowStockCountTask);
                if (this.IsDisposed) return;

                var daily = await dailyTask;
                var monthly = await monthlyTask;
                var lowStockCount = await lowStockCountTask;

                panelKPIs.Invoke((Action)(() => {
                    panelKPIs.Controls.Clear();
                    panelKPIs.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("dash_daily_sales"), daily.TotalSales.ToString("C0"), UIHelper.AccentBlue, 250, 110));
                    
                    if (AuthService.IsManager)
                    {
                        panelKPIs.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("dash_monthly_revenue"), monthly.Revenue.ToString("C0"), UIHelper.PrimaryTeal, 250, 110));
                        panelKPIs.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("dash_net_profit"), monthly.NetProfit.ToString("C0"), monthly.NetProfit >= 0 ? UIHelper.AccentGreen : UIHelper.AccentRed, 250, 110));
                    }
                    panelKPIs.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("dash_low_stock_items"), lowStockCount.ToString(), UIHelper.AccentOrange, 250, 110));
                }));
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadSalesTrendAsync(DashboardService svc)
        {
            try
            {
                var trend = await svc.GetMonthlySalesTrendAsync(12);
                if (this.IsDisposed) return;

                chartSalesTrend.Invoke((Action)(() => {
                    if (chartSalesTrend.Series.IndexOf("Sales") == -1)
                    {
                        var series = new Series("Sales") { ChartType = SeriesChartType.SplineArea, Color = Color.FromArgb(120, UIHelper.PrimaryTeal), BorderColor = UIHelper.PrimaryTeal, BorderWidth = 2 };
                        chartSalesTrend.Series.Add(series);
                    }
                    chartSalesTrend.Series["Sales"].Points.Clear();
                    foreach (var t in trend)
                        chartSalesTrend.Series["Sales"].Points.AddXY(t.Month.Substring(5), (double)t.Total);
                }));
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadProfitTrendAsync(DashboardService svc)
        {
            try
            {
                var profitTrend = await svc.GetMonthlyProfitTrendAsync(12);
                if (this.IsDisposed) return;

                chartProfit.Invoke((Action)(() => {
                    if (chartProfit.Series.IndexOf("Revenue") == -1)
                    {
                        chartProfit.Series.Add(new Series("Revenue") { ChartType = SeriesChartType.Column, Color = UIHelper.PrimaryTeal });
                        chartProfit.Series.Add(new Series("Profit") { ChartType = SeriesChartType.Column, Color = UIHelper.AccentGreen });
                    }
                    chartProfit.Series["Revenue"].Points.Clear();
                    chartProfit.Series["Profit"].Points.Clear();
                    
                    if (AuthService.IsManager)
                    {
                        foreach (var p in profitTrend)
                        {
                            chartProfit.Series["Revenue"].Points.AddXY(p.Month.Substring(5), (double)p.Revenue);
                            chartProfit.Series["Profit"].Points.AddXY(p.Month.Substring(5), (double)p.Profit);
                        }
                    }
                    else
                    {
                        if (chartProfit.Titles.Count > 0)
                            chartProfit.Titles[0].Text = LanguageManager.Get("dash_profit_restricted");
                    }
                }));
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadCategoryPerformanceAsync(DashboardService svc)
        {
            try
            {
                var categories = await svc.GetCategoryPerformanceAsync();
                if (this.IsDisposed) return;

                chartCategory.Invoke((Action)(() => {
                    if (chartCategory.Series.IndexOf("Categories") == -1)
                        chartCategory.Series.Add(new Series("Categories") { ChartType = SeriesChartType.Pie });

                    var catSeries = chartCategory.Series["Categories"];
                    catSeries.Points.Clear();
                    Color[] colors = { UIHelper.PrimaryTeal, UIHelper.AccentOrange, UIHelper.AccentBlue, UIHelper.AccentGreen, UIHelper.AccentRed, UIHelper.PrimaryTealLight, Color.FromArgb(156, 39, 176) };
                    int ci = 0;
                    foreach (var c in categories)
                    {
                        var ptIdx = catSeries.Points.AddXY(c.Category, (double)c.Total);
                        catSeries.Points[ptIdx].Color = colors[ci % colors.Length];
                        catSeries.Points[ptIdx].Label = c.Category;
                        ci++;
                    }
                }));
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadLowStockGridAsync(ProductService productSvc)
        {
            try
            {
                var lowStock = await productSvc.GetLowStockAsync();
                if (this.IsDisposed) return;

                dgvLowStock.Invoke((Action)(() => {
                    dgvLowStock.DataSource = lowStock;
                }));
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadTopProductsGridAsync(DashboardService svc)
        {
            try
            {
                var topProducts = await svc.GetTopProductsAsync(5);
                if (this.IsDisposed) return;

                dgvTopProducts.Invoke((Action)(() => {
                    dgvTopProducts.DataSource = topProducts.ConvertAll(t => new { t.Name, t.UnitsSold, Revenue = t.Revenue.ToString("C") });
                }));
            }
            catch { }
        }
    }
}
