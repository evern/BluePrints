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
        public List<ExoDataPoint> ExoPOs { get; set; }
        public List<ExoDataPoint> ExoActuals { get; set; }
        public List<FORECAST_PO> FORECAST_POs { get; set; }

        public POForecastProjection()
        {
            FORECAST_POs = new List<FORECAST_PO>();
        }

        public DateTime? InvoiceDate
        {
            get
            {
                if (PO_RemainingPrice == 0)
                    return null;

                if (ExoPOs == null)
                    return null;

                return ExoPOs.Min(x => x.ActualDate);
            }
        }

        public void UpdateForecastPayments(IEnumerable<FORECAST_PO> allFORECAST_POs)
        {
            FORECAST_POs.Clear();
            ResetPaymentDates();
            IEnumerable<FORECAST_PO> currentPOs = allFORECAST_POs.Where(x => x.PONO == this.PONO);
            foreach(FORECAST_PO currentPO in currentPOs)
            {
                FORECAST_POs.Add(currentPO);
            }
        }

        List<ExoDataPoint> forecastPayments { get; set; }
        public IEnumerable<ExoDataPoint> ForecastPayments
        {
            get
            {
                if(forecastPayments == null)
                {
                    DateTime lastDayOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1);
                    forecastPayments = new List<ExoDataPoint>();

                    if (InvoiceDate != null)
                    {
                        foreach(FORECAST_PO FORECAST_PO in FORECAST_POs)
                        {
                            if (FORECAST_PO.FORECAST_DATE.Month < DateTime.Now.Month || FORECAST_PO.FORECAST_PERCENT == null)
                                continue;

                            ExoDataPoint forecastPaymentPoint = new ExoDataPoint();
                            forecastPaymentPoint.Costs = Math.Round(PO_TotalPrice) * (decimal)FORECAST_PO.FORECAST_PERCENT;
                            forecastPaymentPoint.ActualDate = FORECAST_PO.FORECAST_DATE.Date;
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

        public decimal PO_TotalPrice
        {
            get => ExoPOs.Sum(x => x.TotalCosts);
        }
    }
}
