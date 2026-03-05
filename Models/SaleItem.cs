namespace BussinessErp.Models
{
    public class SaleItem
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal SellPrice { get; set; }
        public decimal LineTotal => Quantity * SellPrice;
    }
}
