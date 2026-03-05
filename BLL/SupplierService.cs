using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class SupplierService
    {
        private readonly SupplierRepository _repo = new SupplierRepository();

        public Task<List<Supplier>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Supplier> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<List<Supplier>> SearchAsync(string keyword) => _repo.SearchAsync(keyword);

        public async Task<(bool Success, string Error)> AddAsync(Supplier s)
        {
            RoleGuard.RequiresManager("Add Supplier");
            string error;
            if (!ValidationHelper.IsRequired(s.Name, "Name", out error)) return (false, error);
            if (!ValidationHelper.IsValidEmail(s.Email, out error)) return (false, error);

            await _repo.AddAsync(s);
            return (true, null);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(Supplier s)
        {
            RoleGuard.RequiresManager("Update Supplier");
            string error;
            if (!ValidationHelper.IsRequired(s.Name, "Name", out error)) return (false, error);
            if (!ValidationHelper.IsValidEmail(s.Email, out error)) return (false, error);

            await _repo.UpdateAsync(s);
            return (true, null);
        }

        public Task DeleteAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete Supplier");
            return _repo.DeleteAsync(id);
        }
    }
}
