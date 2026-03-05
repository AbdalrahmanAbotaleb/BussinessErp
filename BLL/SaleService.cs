using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class SaleService
    {
        private readonly SaleRepository _repo = new SaleRepository();

        public Task<List<Sale>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Sale> GetByIdWithItemsAsync(int id) => _repo.GetByIdWithItemsAsync(id);
        public Task<List<Sale>> GetByDateRangeAsync(DateTime from, DateTime to) => _repo.GetByDateRangeAsync(from, to);

        /// <summary>
        /// Creates a sale via stored procedure — stock is updated automatically.
        /// </summary>
        public async Task<(bool Success, int SaleId, string Error)> CreateSaleAsync(int? customerId, List<SaleItem> items)
        {
            if (items == null || items.Count == 0)
                return (false, 0, "No items in sale.");

            foreach (var item in items)
            {
                if (item.ProductId <= 0) return (false, 0, "Invalid product selected.");
                if (item.Quantity <= 0) return (false, 0, "Quantity must be greater than zero.");
                if (item.SellPrice <= 0) return (false, 0, "Price must be greater than zero.");
            }

            int saleId = await _repo.AddSaleAsync(customerId, items);
            return (true, saleId, null);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(Sale sale)
        {
            if (!RoleGuard.CanEditSale(sale)) 
                return (false, "Unauthorized: Finalized sales can only be edited by Managers.");
            
            await _repo.UpdateAsync(sale);
            return (true, null);
        }

        public Task DeleteAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete Sale");
            return _repo.DeleteAsync(id);
        }
    }
}
