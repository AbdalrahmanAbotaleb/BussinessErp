using System;

namespace BussinessErp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime CreatedAt { get; set; }

        public decimal ProfitMargin =>
            SellPrice > 0 ? ((SellPrice - CostPrice) / SellPrice) * 100 : 0;
    }
}
