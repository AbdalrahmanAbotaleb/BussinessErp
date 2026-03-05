using System;
using System.Collections.Generic;

namespace BussinessErp.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}
