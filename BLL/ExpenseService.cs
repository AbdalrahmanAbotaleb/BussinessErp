using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessErp.DAL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.BLL
{
    public class ExpenseService
    {
        private readonly ExpenseRepository _repo = new ExpenseRepository();

        public Task<List<Expense>> GetAllAsync() => _repo.GetAllAsync();
        public Task<List<Expense>> GetByDateRangeAsync(DateTime from, DateTime to) => _repo.GetByDateRangeAsync(from, to);
        public Task<List<string>> GetCategoriesAsync() => _repo.GetCategoriesAsync();

        public async Task<(bool Success, string Error)> AddAsync(Expense e)
        {
            RoleGuard.RequiresManager("Add Expense");
            string error;
            if (!ValidationHelper.IsRequired(e.Title, "Title", out error)) return (false, error);
            if (!ValidationHelper.IsPositiveNumber(e.Amount, "Amount", out error)) return (false, error);

            await _repo.AddAsync(e);
            return (true, null);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(Expense e)
        {
            RoleGuard.RequiresManager("Update Expense");
            string error;
            if (!ValidationHelper.IsRequired(e.Title, "Title", out error)) return (false, error);
            if (!ValidationHelper.IsPositiveNumber(e.Amount, "Amount", out error)) return (false, error);

            await _repo.UpdateAsync(e);
            return (true, null);
        }

        public Task DeleteAsync(int id)
        {
            RoleGuard.RequiresAdmin("Delete Expense");
            return _repo.DeleteAsync(id);
        }
    }
}
