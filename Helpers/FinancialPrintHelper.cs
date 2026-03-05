using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BussinessErp.DAL;
using System.Collections.Generic;
using System.Linq;

namespace BussinessErp.Helpers
{
    public static class FinancialPrintHelper
    {
        public static void PrintPLReport(PLReportData data, DateTime startDate, DateTime endDate, Bitmap chartCustomer = null, Bitmap chartProduct = null)
        {
            if (data == null) return;

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
                pd.PrintPage += (s, e) => DrawReport(e, data, startDate, endDate, chartCustomer, chartProduct);
                
                using (PrintPreviewDialog ppd = new PrintPreviewDialog())
                {
                    ppd.Document = pd;
                    ppd.WindowState = FormWindowState.Maximized;
                    ppd.ShowDialog();
                }
            }
        }

        private static void DrawReport(PrintPageEventArgs e, PLReportData data, DateTime start, DateTime end, Bitmap chartCustomer, Bitmap chartProduct)
        {
            Graphics g = e.Graphics;
            Font fontTitle = new Font("Arial", 20, FontStyle.Bold); // Increased to 20
            Font fontHeader = new Font("Arial", 14, FontStyle.Bold); // Increased to 14
            Font fontBody = new Font("Arial", 11); // Increased to 11
            Font fontBodyBold = new Font("Arial", 11, FontStyle.Bold);

            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float width = e.MarginBounds.Width;

            // Header
            g.DrawString("AI Business Brain — Profit & Loss Statement", fontTitle, Brushes.Teal, x, y);
            y += 40;
            g.DrawString($"Period: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}", fontBody, Brushes.Black, x, y);
            y += 30;

            // Summary Table
            g.DrawString("Summary", fontHeader, Brushes.Black, x, y);
            y += 25;
            DrawRow(g, "Total Revenue", data.TotalRevenue.ToString("C"), fontBody, x, y, width); y += 20;
            DrawRow(g, "Cost of Goods Sold (COGS)", $"-{data.COGS:C}", fontBody, x, y, width); y += 20;
            g.DrawLine(Pens.LightGray, x, y, x + width, y); y += 5;
            DrawRow(g, "Gross Profit", data.GrossProfit.ToString("C"), fontBodyBold, x, y, width); y += 25;
            DrawRow(g, "Total Operating Expenses", $"-{data.TotalExpenses:C}", fontBody, x, y, width); y += 20;
            g.DrawLine(Pens.Black, x, y, x + width, y); y += 5;
            DrawRow(g, "NET PROFIT", data.NetProfit.ToString("C"), fontHeader, x, y, width, Brushes.DarkGreen); y += 40;

            // Charts Section
            if (chartCustomer != null)
            {
                g.DrawString("Profit by Top Customers", fontHeader, Brushes.Black, x, y);
                y += 25;
                float chartHeight = 250;
                g.DrawImage(chartCustomer, x, y, width, chartHeight);
                y += chartHeight + 20;
                
                if (chartProduct != null)
                {
                    g.DrawString("Profit by Top Products", fontHeader, Brushes.Black, x, y);
                    y += 25;
                    g.DrawImage(chartProduct, x, y, width, chartHeight);
                    y += chartHeight + 20;
                }
            }

            // Detailed Profit Breakdown (Top 5 Customers)
            g.DrawString("Top Profitable Customers", fontHeader, Brushes.Black, x, y);
            y += 25;
            g.FillRectangle(Brushes.GhostWhite, x, y, width, 25);
            g.DrawString("Customer Name", fontBodyBold, Brushes.Black, x + 5, y + 5);
            g.DrawString("Profit Contribution", fontBodyBold, Brushes.Black, x + width - 150, y + 5);
            y += 30;
            
            foreach(var item in data.ProfitByCustomer.Take(5))
            {
                g.DrawString(item.Key, fontBody, Brushes.Black, x + 5, y);
                g.DrawString(item.Value.ToString("C"), fontBody, Brushes.Black, x + width - 150, y);
                y += 20;
            }
        }

        private static void DrawRow(Graphics g, string label, string val, Font font, float x, float y, float width, Brush brush = null)
        {
            brush = brush ?? Brushes.Black;
            g.DrawString(label, font, Brushes.Black, x, y);
            g.DrawString(val, font, brush, x + width - 150, y);
        }
    }
}
