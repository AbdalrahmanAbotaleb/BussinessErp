using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class CustomerService
    {
        private readonly CustomerRepository _repo = new CustomerRepository();

        public Task<List<Customer>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Customer> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<List<Customer>> SearchAsync(string keyword) => _repo.SearchAsync(keyword);

        public async Task<(bool Success, string Error)> AddAsync(Customer cust)
        {
            string error;
            if (!ValidationHelper.IsRequired(cust.Name, "Name", out error)) return (false, error);
            if (!ValidationHelper.IsValidEmail(cust.Email, out error)) return (false, error);
            if (!ValidationHelper.IsValidPhone(cust.Phone, out error)) return (false, error);

            await _repo.AddAsync(cust);
            return (true, null);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(Customer cust)
        {
            string error;
            if (!ValidationHelper.IsRequired(cust.Name, "Name", out error)) return (false, error);
            if (!ValidationHelper.IsValidEmail(cust.Email, out error)) return (false, error);
            if (!ValidationHelper.IsValidPhone(cust.Phone, out error)) return (false, error);

            await _repo.UpdateAsync(cust);
            return (true, null);
        }

        public Task DeleteAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete Customer");
            return _repo.DeleteAsync(id);
        }
    }
}
