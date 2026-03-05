using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class InvoiceService
    {
        private readonly SaleService _saleSvc = new SaleService();
        private readonly PurchaseService _purchSvc = new PurchaseService();

        public async Task<InvoiceViewModel> GetSaleInvoiceAsync(int saleId)
        {
            var sale = await _saleSvc.GetByIdWithItemsAsync(saleId);
            if (sale == null) return null;

            return new InvoiceViewModel
            {
                InvoiceTitle = "SALE INVOICE",
                TransactionId = sale.Id,
                Date = sale.Date,
                PartnerLabel = "Customer",
                PartnerName = sale.CustomerName ?? "Cash Customer",
                TotalAmount = sale.TotalAmount,
                Items = sale.Items.Select(i => new InvoiceItemViewModel
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.SellPrice
                }).ToList()
            };
        }

        public async Task<InvoiceViewModel> GetPurchaseInvoiceAsync(int purchaseId)
        {
            var purchase = await _purchSvc.GetByIdWithItemsAsync(purchaseId);
            if (purchase == null) return null;

            return new InvoiceViewModel
            {
                InvoiceTitle = "PURCHASE INVOICE",
                TransactionId = purchase.Id,
                Date = purchase.Date,
                PartnerLabel = "Supplier",
                PartnerName = purchase.SupplierName ?? "Unknown Supplier",
                TotalAmount = purchase.TotalAmount,
                Items = purchase.Items.Select(i => new InvoiceItemViewModel
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.CostPrice
                }).ToList()
            };
        }
    }
}
