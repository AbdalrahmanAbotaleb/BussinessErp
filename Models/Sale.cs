using System;
using System.Collections.Generic;

namespace BussinessErp.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public List<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
