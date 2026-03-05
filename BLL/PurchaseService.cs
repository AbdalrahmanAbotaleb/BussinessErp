using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class PurchaseService
    {
        private readonly PurchaseRepository _repo = new PurchaseRepository();

        public Task<List<Purchase>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Purchase> GetByIdWithItemsAsync(int id) => _repo.GetByIdWithItemsAsync(id);

        /// <summary>
        /// Creates a purchase via stored procedure — stock is updated automatically.
        /// </summary>
        public async Task<(bool Success, int PurchaseId, string Error)> CreatePurchaseAsync(int? supplierId, List<PurchaseItem> items)
        {
            RoleGuard.RequiresManager("Create Purchase");
            if (items == null || items.Count == 0)
                return (false, 0, "No items in purchase.");

            foreach (var item in items)
            {
                if (item.ProductId <= 0) return (false, 0, "Invalid product selected.");
                if (item.Quantity <= 0) return (false, 0, "Quantity must be greater than zero.");
                if (item.CostPrice < 0) return (false, 0, "Cost price cannot be negative.");
            }

            int purchaseId = await _repo.AddPurchaseAsync(supplierId, items);
            return (true, purchaseId, null);
        }

        public Task DeleteAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete Purchase");
            return _repo.DeleteAsync(id);
        }
    }
}
