using System;
using System.Collections.Generic;

namespace BussinessErp.Models
{
    public class InvoiceViewModel
    {
        public string InvoiceTitle { get; set; } // e.g., "SALE INVOICE" or "PURCHASE INVOICE"
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string PartnerLabel { get; set; } // "Customer" or "Supplier"
        public string PartnerName { get; set; }
        public List<InvoiceItemViewModel> Items { get; set; } = new List<InvoiceItemViewModel>();
        public decimal TotalAmount { get; set; }
        public string CompanyName { get; set; } = "Bussiness ERP Ltd.";
    }

    public class InvoiceItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
