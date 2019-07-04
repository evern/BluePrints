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
        public DateTime ActualCutOffDate { get; set; }
        public List<FORECAST_PO> FORECAST_POs { get; set; }
        public decimal TotalForecast => FORECAST_POs.Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
        public decimal Unforecasted => (PO_RemainingPrice - TotalForecast);
        public bool IsPOError => Math.Round(Unforecasted) != 0;

        public decimal ErrorImageWidth => IsPOError ? 15 : 0;

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

        public void UpdateForecastPayments(IEnumerable<FORECAST_PO> allFORECAST_POs, DateTime actualCutOffDate)
        {
            ActualCutOffDate = actualCutOffDate;
            FORECAST_POs.Clear();
            ResetPaymentDates();
            IEnumerable<FORECAST_PO> currentPOForecasts = allFORECAST_POs.Where(x => x.PONO == this.PONO);
            foreach(FORECAST_PO currentPOForecast in currentPOForecasts)
            {
                FORECAST_POs.Add(currentPOForecast);
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
                    if (InvoiceDate != null)
                    {
                        foreach (FORECAST_PO FORECAST_PO in FORECAST_POs.OrderBy(x => x.FORECAST_DATE))
                        {
                            if (FORECAST_PO.FORECAST_DATE.Date <= ActualCutOffDate.Date || FORECAST_PO.FORECAST_VALUE == null)
                                continue;

                            ExoDataPoint forecastPaymentPoint = new ExoDataPoint();

                            forecastPaymentPoint.Costs = (decimal)FORECAST_PO.FORECAST_VALUE;
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
