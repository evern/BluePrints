using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class POForecastProjection
    {
        public string PONO { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string VariationCode { get; set; }
        public string Comments { get; set; }
        public List<ExoDataPoint> ExoPOs { get; set; }
        public List<ExoDataPoint> ExoActuals { get; set; }
        public DateTime ActualCutOffDate { get; set; }
        public List<FORECAST_PO> FORECAST_POs { get; set; }
        public decimal TotalForecast => FORECAST_POs.Where(x => x.FORECAST_DATE > ActualCutOffDate).Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
        public decimal Unforecasted => (PO_RemainingPrice - TotalForecast);
        public bool IsPOError => Math.Round(Unforecasted) != 0;
        public decimal ErrorImageWidth => IsPOError ? 15 : 0;

        public POForecastProjection()
        {
            FORECAST_POs = new List<FORECAST_PO>();
            ExoActuals = new List<ExoDataPoint>();
        }

        public DateTime? FirstActualDate
        {
            get
            {
                if (ExoPOs == null)
                    return null;

                return ExoPOs.Min(x => x.ActualDate);
            }
        }

        public DateTime? FirstInvoiceDate
        {
            get
            {
                if (ExoActuals == null || ExoActuals.Count == 0)
                    return null;

                return ExoActuals.Min(x => x.InvoiceDate);
            }
        }

        public void UpdateForecastPayments(IEnumerable<FORECAST_PO> allFORECAST_POs, IEnumerable<ExoDataPoint> allActuals, DateTime actualCutOffDate)
        {
            ActualCutOffDate = actualCutOffDate;
            ExoActuals.Clear();
            FORECAST_POs.Clear();
            ResetPaymentDates();
            IEnumerable<FORECAST_PO> currentPOForecasts = allFORECAST_POs.Where(x => x.PONO == this.PONO && x.VARIATION_CODE == this.VariationCode);
            IEnumerable<ExoDataPoint> currentActuals = allActuals.Where(x => x.PONumber == this.PONO && x.Variation_Code == this.VariationCode);
            foreach(FORECAST_PO currentPOForecast in currentPOForecasts)
            {
                //because forecast can sometimes store outdated job code, cost group and cost type, validation is required before adding, else forecast PO can show that it's forecasted but forecast module will pick it up on the wrong code
                if(ExoPOs.Any(x => x.Subjob_Name == currentPOForecast.JOB_CODE && x.Discipline_Code == currentPOForecast.DISCIPLINE_CODE && x.Commodity_Code == currentPOForecast.COMMODITY_CODE && x.StockCode == currentPOForecast.STOCK_CODE && x.Variation_Code == currentPOForecast.VARIATION_CODE))
                    FORECAST_POs.Add(currentPOForecast);
            }

            foreach(ExoDataPoint currentActual in currentActuals)
            {
                ExoActuals.Add(currentActual);
            }

            this.RaisePropertiesChanged();
        }

        List<ExoDataPoint> forecastPayments { get; set; }
        public IEnumerable<ExoDataPoint> ForecastPayments
        {
            get
            {
                if(forecastPayments == null)
                {
                    if (ActualCutOffDate == null)
                        return null;

                    forecastPayments = new List<ExoDataPoint>();
                    if (ExoPOs != null)
                    {
                        var groupByDateFORECASTS = FORECAST_POs.GroupBy(x => x.FORECAST_DATE).Select(g => new { ForecastDate = g.Key, ForecastCost = g.Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE) }).OrderBy(x => x.ForecastDate);
                        foreach (var groupByDateFORECAST in groupByDateFORECASTS)
                        {
                            if (groupByDateFORECAST.ForecastDate <= ActualCutOffDate.Date || groupByDateFORECAST.ForecastCost == 0)
                                continue;

                            ExoDataPoint forecastPaymentPoint = new ExoDataPoint();

                            forecastPaymentPoint.Costs = groupByDateFORECAST.ForecastCost;
                            forecastPaymentPoint.ActualDate = groupByDateFORECAST.ForecastDate;

                            forecastPayments.Add(forecastPaymentPoint);
                        }
                    }
                }

                return forecastPayments;
            }
        }

        public DateTime FirstForecastDate
        {
            get
            {
                if (FORECAST_POs.Count == 0)
                    return DateTime.Now.Date;

                return FORECAST_POs.Min(x => x.FORECAST_DATE);
            }
        }

        public DateTime LastForecastDate
        {
            get
            {
                if (FORECAST_POs.Count == 0)
                    return DateTime.Now.Date;

                return FORECAST_POs.Max(x => x.FORECAST_DATE);
            }
        }

        public void ResetPaymentDates()
        {
            forecastPayments = null;
        }

        public decimal PO_RemainingPrice
        {
            get => ExoPOs.Sum(x => x.Costs);
        }

        public decimal PO_Invoiced
        {
            get => ExoActuals.Sum(x => x.Costs);
        }

        public decimal PO_TotalPrice
        {
            get => ExoPOs.Sum(x => x.TotalCosts);
        }
    }
}
