using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using BussinessErp.DAL;

namespace BussinessErp.BLL
{
    public class SalesData
    {
        public float Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class SalesPrediction
    {
        public float[] Forecast { get; set; }
        public float[] ConfidenceLowerBound { get; set; }
        public float[] ConfidenceUpperBound { get; set; }
    }

    public class MLForecastingService
    {
        private readonly MLContext _mlContext;
        private readonly AIDataRepository _repo = new AIDataRepository();

        public MLForecastingService()
        {
            _mlContext = new MLContext();
        }

        public async Task<SalesPrediction> PredictNextMonthsAsync(int horizon = 3)
        {
            var history = await _repo.GetMonthlySalesHistoryAsync(24); // Support up to 2 years
            if (history.Count < 6)
                return null; // Not enough data for SSA

            var data = history.Select(h => new SalesData 
            { 
                Amount = (float)h.Total,
                Date = new DateTime(h.Year, h.Month, 1)
            }).ToList();

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(SalesPrediction.Forecast),
                inputColumnName: nameof(SalesData.Amount),
                windowSize: 4, // Quarterly patterns
                seriesLength: data.Count,
                trainSize: data.Count,
                horizon: horizon,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: nameof(SalesPrediction.ConfidenceLowerBound),
                confidenceUpperBoundColumn: nameof(SalesPrediction.ConfidenceUpperBound));

            var model = pipeline.Fit(dataView);
            var forecastingEngine = model.CreateTimeSeriesEngine<SalesData, SalesPrediction>(_mlContext);
            
            return forecastingEngine.Predict();
        }
    }
}
