using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class ProductService
    {
        private readonly ProductRepository _repo = new ProductRepository();

        public Task<List<Product>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Product> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<List<Product>> SearchAsync(string keyword) => _repo.SearchAsync(keyword);
        public Task<List<Product>> GetLowStockAsync() => _repo.GetLowStockAsync();
        private static List<string> _categoryCache;
        public async Task<List<string>> GetCategoriesAsync()
        {
            if (_categoryCache == null)
            {
                _categoryCache = await _repo.GetCategoriesAsync();
            }
            return _categoryCache;
        }

        public void ClearCategoryCache() => _categoryCache = null;

        public Task<List<Product>> GetByCategoryAsync(string cat) => _repo.GetByCategoryAsync(cat);

        public async Task<(bool Success, string Error)> AddAsync(Product p)
        {
            RoleGuard.RequiresManager("Add Product");
            string error;
            if (!ValidationHelper.IsRequired(p.Name, "Product Name", out error)) return (false, error);
            if (!ValidationHelper.IsPositiveNumber(p.SellPrice, "Sell Price", out error)) return (false, error);
            if (!ValidationHelper.IsNonNegativeNumber(p.CostPrice, "Cost Price", out error)) return (false, error);
            if (p.CostPrice > p.SellPrice) return (false, "Cost Price cannot exceed Sell Price.");

            await _repo.AddAsync(p);
            return (true, null);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(Product p)
        {
            RoleGuard.RequiresManager("Update Product");
            string error;
            if (!ValidationHelper.IsRequired(p.Name, "Product Name", out error)) return (false, error);
            if (!ValidationHelper.IsPositiveNumber(p.SellPrice, "Sell Price", out error)) return (false, error);
            if (!ValidationHelper.IsNonNegativeNumber(p.CostPrice, "Cost Price", out error)) return (false, error);

            await _repo.UpdateAsync(p);
            return (true, null);
        }

        public Task DeleteAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete Product");
            return _repo.DeleteAsync(id);
        }

        /// <summary>
        /// Stock updates exclusively through stored procedure.
        /// </summary>
        public Task UpdateStockAsync(int productId, int qty, bool isDecrease) =>
            _repo.UpdateStockAsync(productId, qty, isDecrease);
    }
}
