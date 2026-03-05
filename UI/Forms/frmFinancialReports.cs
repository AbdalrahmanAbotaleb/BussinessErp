using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using BussinessErp.BLL;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace BussinessErp.UI.Forms
{
    public class frmFinancialReports : Form
    {
        private DateTimePicker dtFrom, dtTo;
        private Button btnRefresh;
        private FlowLayoutPanel panelSummary;
        private Panel panelExpenses;
        private DataGridView dgvExpenses;
        private Chart chartCustomer, chartProduct;
        private Button btnPrint;
        private PLReportData _lastData;
        private readonly FinancialService _service = new FinancialService();

        public frmFinancialReports()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UIHelper.StyleForm(this, LanguageManager.Get("nav_financials"));
            this.Padding = new Padding(20);
            this.AutoScroll = true; // Enable form-level scrolling

            LanguageManager.ApplyRTL(this);

            // Title
            var lblTitle = new Label
            {
                Text = LanguageManager.Get("financial_title"),
                Font = UIHelper.FontTitle,
                ForeColor = UIHelper.TextDark,
                Dock = DockStyle.Top,
                Height = 60
            };

            // Filter Bar
            var panelFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10)
            };

            var lblFrom = new Label { Text = LanguageManager.Get("from"), AutoSize = true, Location = new Point(0, 15) };
            dtFrom = new DateTimePicker { Location = new Point(50, 12), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-1) };
            
            var lblTo = new Label { Text = LanguageManager.Get("to"), AutoSize = true, Location = new Point(220, 15) };
            dtTo = new DateTimePicker { Location = new Point(250, 12), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            btnRefresh = new Button { Text = LanguageManager.Get("generate_report"), Location = new Point(420, 8), Width = 180 };
            UIHelper.StyleButton(btnRefresh);
            btnRefresh.Click += async (s, e) => await LoadReportAsync();

            btnPrint = new Button { Text = LanguageManager.Get("print_report"), Location = new Point(610, 8), Width = 150, BackColor = Color.FromArgb(60, 60, 60) };
            UIHelper.StyleButton(btnPrint, Color.FromArgb(60, 60, 60));
            btnPrint.Enabled = false;
            btnPrint.Click += (s, e) => PrintReport();

            panelFilters.Controls.AddRange(new Control[] { lblFrom, dtFrom, lblTo, dtTo, btnRefresh, btnPrint });

            // Summary Row
            panelSummary = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 130,
                WrapContents = false,
                AutoScroll = true
            };

            // Charts Panel
            var panelCharts = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 450, 
                ColumnCount = 1, 
                Padding = new Padding(0, 10, 0, 10),
                AutoScroll = false // Disable panel-level scrolling, use form's
            };
            panelCharts.RowStyles.Add(new RowStyle(SizeType.Absolute, 400f));
            panelCharts.RowStyles.Add(new RowStyle(SizeType.Absolute, 400f));
            panelCharts.Height = 820; // Total height for two large charts

            chartCustomer = CreateChart("Profit by Customer (Top 10)");
            chartProduct = CreateChart("Profit by Product (Top 10)");

            panelCharts.Controls.Add(chartCustomer, 0, 0);
            panelCharts.Controls.Add(chartProduct, 0, 1);

            // Expenses Detail Area
            panelExpenses = new Panel
            {
                Dock = DockStyle.Top, // Stack it
                Height = 400, // Fixed height to allow scrolling
                Padding = new Padding(0, 10, 0, 0)
            };

            var lblExpTitle = new Label
            {
                Text = LanguageManager.Get("expense_breakdown"),
                Font = UIHelper.FontSubtitle,
                ForeColor = UIHelper.TextDark,
                Dock = DockStyle.Top,
                Height = 40
            };

            dgvExpenses = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White
            };
            UIHelper.StyleDataGridView(dgvExpenses);

            panelExpenses.Controls.Add(dgvExpenses);
            panelExpenses.Controls.Add(lblExpTitle);

            this.Controls.Add(panelExpenses);
            this.Controls.Add(panelCharts);
            this.Controls.Add(panelSummary);
            this.Controls.Add(panelFilters);
            this.Controls.Add(lblTitle);

            this.Load += async (s, e) => await LoadReportAsync();
        }

        private async Task LoadReportAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                btnRefresh.Text = LanguageManager.Get("processing");

                var data = await _service.GetPLStatementAsync(dtFrom.Value.Date, dtTo.Value.Date.AddDays(1).AddSeconds(-1));
                _lastData = data;

                if (this.IsDisposed) return;

                // Update Summary Cards
                panelSummary.Controls.Clear();
                panelSummary.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("total_revenue"), data.TotalRevenue.ToString("C"), UIHelper.AccentBlue, 280));
                panelSummary.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("gross_profit"), data.GrossProfit.ToString("C"), UIHelper.PrimaryTeal, 280));
                panelSummary.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("total_expenses"), data.TotalExpenses.ToString("C"), UIHelper.AccentOrange, 280));
                panelSummary.Controls.Add(UIHelper.CreateKPICard(LanguageManager.Get("net_profit"), data.NetProfit.ToString("C"), data.NetProfit >= 0 ? UIHelper.AccentGreen : UIHelper.AccentRed, 280));

                // Update Grid
                dgvExpenses.DataSource = data.ExpensesByCategory.Select(x => new { 
                    Category = x.Key, 
                    Amount = x.Value.ToString("C"), 
                    Percentage = (data.TotalExpenses > 0 ? (x.Value / data.TotalExpenses * 100).ToString("F1") + "%" : "0%") 
                }).ToList();

                // Update Charts
                UpdateChart(chartCustomer, data.ProfitByCustomer, LanguageManager.Get("profit_by_customer"), Color.Teal);
                UpdateChart(chartProduct, data.ProfitByProduct, LanguageManager.Get("profit_by_product"), Color.Orange);

                btnPrint.Enabled = true;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Failed to load report: " + ex.Message);
            }
            finally
            {
                btnRefresh.Enabled = true;
                btnRefresh.Text = LanguageManager.Get("generate_report");
            }
        }

        private Chart CreateChart(string title)
        {
            var chart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            var area = new ChartArea("Main") 
            { 
                BackColor = Color.White,
                AxisX = { 
                    Title = "Name", 
                    Interval = 1, 
                    LabelStyle = { Angle = -45, Font = new Font("Segoe UI", 10, FontStyle.Bold) }, // Increased to 10 Bold
                    TitleFont = new Font("Segoe UI", 11, FontStyle.Bold)
                },
                AxisY = { 
                    Title = "Profit", 
                    LabelStyle = { Format = "C0", Font = new Font("Segoe UI", 10) }, // Increased to 10
                    TitleFont = new Font("Segoe UI", 11, FontStyle.Bold)
                }
            };
            chart.ChartAreas.Add(area);
            chart.Titles.Add(new Title(title) { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = UIHelper.PrimaryTeal }); // Increased to 14
            return chart;
        }

        private void UpdateChart(Chart chart, Dictionary<string, decimal> data, string seriesName, Color color)
        {
            chart.Series.Clear();
            var series = new Series(seriesName) 
            { 
                ChartType = SeriesChartType.Column, 
                IsValueShownAsLabel = true,
                LabelFormat = "C0",
                Font = new Font("Segoe UI", 10, FontStyle.Bold), // Added Font to series labels
                Palette = ChartColorPalette.SeaGreen
            };
            chart.Series.Add(series);
            
            foreach (var item in data.Take(10))
            {
                var pIdx = series.Points.AddXY(item.Key, (double)item.Value);
                series.Points[pIdx].ToolTip = $"{item.Key}: {item.Value:C}";
            }
        }

        private void PrintReport()
        {
            if (_lastData == null) return;

            using (var ms1 = new MemoryStream())
            using (var ms2 = new MemoryStream())
            {
                chartCustomer.SaveImage(ms1, ChartImageFormat.Bmp);
                chartProduct.SaveImage(ms2, ChartImageFormat.Bmp);
                
                var bmp1 = new Bitmap(ms1);
                var bmp2 = new Bitmap(ms2);

                FinancialPrintHelper.PrintPLReport(_lastData, dtFrom.Value, dtTo.Value, bmp1, bmp2);
            }
        }
    }
}
