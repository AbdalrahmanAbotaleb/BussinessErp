using System;
using System.Threading.Tasks;
using BussinessErp.DAL;

namespace BussinessErp.BLL
{
    public class FinancialService
    {
        private readonly FinancialRepository _repo = new FinancialRepository();

        public async Task<PLReportData> GetPLStatementAsync(DateTime startDate, DateTime endDate)
        {
            // Business logic could include tax calculations, currency conversion, etc.
            // For now, we fetch the aggregated data from the repository.
            return await _repo.GetPLStatementAsync(startDate, endDate);
        }
    }
}
