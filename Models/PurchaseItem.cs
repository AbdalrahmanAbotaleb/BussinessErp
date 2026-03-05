namespace BussinessErp.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal LineTotal => Quantity * CostPrice;
    }
}
