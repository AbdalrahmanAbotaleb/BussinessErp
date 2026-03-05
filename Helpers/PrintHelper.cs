using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BussinessErp.Models;

namespace BussinessErp.Helpers
{
    public static class PrintHelper
    {
        public static void PrintInvoice(InvoiceViewModel invoice)
        {
            if (invoice == null) return;

            using (PrintDocument pd = new PrintDocument())
            {
                pd.PrintPage += (s, e) => DrawInvoice(e, invoice);
                
                using (PrintPreviewDialog ppd = new PrintPreviewDialog())
                {
                    ppd.Document = pd;
                    ppd.WindowState = FormWindowState.Maximized;
                    ppd.ShowDialog();
                }
            }
        }

        private static void DrawInvoice(PrintPageEventArgs e, InvoiceViewModel invoice)
        {
            Graphics g = e.Graphics;
            Font fontTitle = new Font("Arial", 18, FontStyle.Bold);
            Font fontHeader = new Font("Arial", 12, FontStyle.Bold);
            Font fontBody = new Font("Arial", 10);
            Font fontBodyBold = new Font("Arial", 10, FontStyle.Bold);

            float x = 50;
            float y = 50;
            float width = e.PageBounds.Width - 100;

            // Header
            g.DrawString(invoice.CompanyName, fontTitle, Brushes.Teal, x, y);
            y += 35;
            g.DrawString(invoice.InvoiceTitle, fontHeader, Brushes.Black, x, y);
            y += 30;

            // Info
            g.DrawString($"ID: {invoice.TransactionId}", fontBody, Brushes.Black, x, y);
            g.DrawString($"Date: {invoice.Date:yyyy-MM-dd HH:mm}", fontBody, Brushes.Black, x + 250, y);
            y += 20;
            g.DrawString($"{invoice.PartnerLabel}: {invoice.PartnerName}", fontBody, Brushes.Black, x, y);
            y += 30;

            // Table Header
            g.DrawLine(Pens.Black, x, y, x + width, y);
            y += 5;
            g.DrawString("Product", fontBodyBold, Brushes.Black, x, y);
            g.DrawString("Qty", fontBodyBold, Brushes.Black, x + 300, y);
            g.DrawString("Price", fontBodyBold, Brushes.Black, x + 400, y);
            g.DrawString("Total", fontBodyBold, Brushes.Black, x + 500, y);
            y += 20;
            g.DrawLine(Pens.Black, x, y, x + width, y);
            y += 10;

            // Items
            foreach (var item in invoice.Items)
            {
                g.DrawString(item.ProductName, fontBody, Brushes.Black, x, y);
                g.DrawString(item.Quantity.ToString(), fontBody, Brushes.Black, x + 300, y);
                g.DrawString(item.UnitPrice.ToString("C"), fontBody, Brushes.Black, x + 400, y);
                g.DrawString(item.Subtotal.ToString("C"), fontBody, Brushes.Black, x + 500, y);
                y += 20;
            }

            // Footer
            y += 10;
            g.DrawLine(Pens.Black, x, y, x + width, y);
            y += 10;
            g.DrawString("GRAND TOTAL:", fontHeader, Brushes.Black, x + 350, y);
            g.DrawString(invoice.TotalAmount.ToString("C"), fontHeader, Brushes.Teal, x + 500, y);
            
            y += 50;
            g.DrawString("Thank you for your business!", fontBody, Brushes.Gray, x, y);
        }
    }
}
