using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _repo = new EmployeeRepository();

        public Task<List<Employee>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Employee> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<List<Employee>> SearchAsync(string keyword) => _repo.SearchAsync(keyword);

        public async Task<(bool Success, string Error)> AddAsync(Employee emp)
        {
            RoleGuard.RequiresAdmin("Add Employee");
            string error;
            if (!ValidationHelper.IsRequired(emp.Name, "Name", out error)) return (false, error);
            if (!ValidationHelper.IsNonNegativeNumber(emp.Salary, "Salary", out error)) return (false, error);

            await _repo.AddAsync(emp);
            return (true, null);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(Employee emp)
        {
            RoleGuard.RequiresAdmin("Update Employee");
            string error;
            if (!ValidationHelper.IsRequired(emp.Name, "Name", out error)) return (false, error);
            if (!ValidationHelper.IsNonNegativeNumber(emp.Salary, "Salary", out error)) return (false, error);

            await _repo.UpdateAsync(emp);
            return (true, null);
        }

        public Task DeleteAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete Employee");
            return _repo.DeleteAsync(id);
        }
    }
}
