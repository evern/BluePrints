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
        public FORECAST_PO ForecastConfig { get; private set; }
        public List<ExoDataPoint> ExoPOs { get; set; }
        public List<ExoDataPoint> ExoActuals { get; set; }
        public List<PO_CUSTOMDATE> CustomPaymentDates { get; set; }
        public POPaymentTerms PaymentTerms { get; set; }

        public void SetForecastConfig(FORECAST_PO forecast)
        {
            ForecastConfig = forecast;
            PaymentTerms = forecast.MODE;
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

        List<ExoDataPoint> forecastPayments { get; set; }
        public IEnumerable<ExoDataPoint> ForecastPayments
        {
            get
            {
                if(forecastPayments == null)
                {
                    forecastPayments = new List<ExoDataPoint>();
                    if (InvoiceDate != null)
                    {
                        int? daysForward = null;
                        if (PaymentTerms == POPaymentTerms.Thirty_Days)
                            daysForward = 30;
                        else if (PaymentTerms == POPaymentTerms.Sixty_Days)
                            daysForward = 60;
                        else if (PaymentTerms == POPaymentTerms.Ninety_Days)
                            daysForward = 90;
                        else if (PaymentTerms == POPaymentTerms.None)
                            daysForward = 1;

                        if(daysForward != null)
                        {
                            ExoDataPoint forecastPaymentPoint = new ExoDataPoint();
                            forecastPaymentPoint.Costs = PO_RemainingPrice;
                            TimeSpan forwardTimeSpan = new TimeSpan((int)daysForward, 0, 0, 0);
                            forecastPaymentPoint.ActualDate = ChronologicalHelpers.ForecastDataDate((DateTime)InvoiceDate, DateTime.Now, forwardTimeSpan);
                            forecastPayments.Add(forecastPaymentPoint);
                        }
                        else
                        {
                            foreach(PO_CUSTOMDATE customPaymentDate in CustomPaymentDates)
                            {
                                ExoDataPoint forecastPaymentPoint = new ExoDataPoint();
                                forecastPaymentPoint.Costs = PO_RemainingPrice * customPaymentDate.PAYMENT_PERCENT;
                                forecastPaymentPoint.ActualDate = customPaymentDate.PAYMENT_DATE;
                                forecastPayments.Add(forecastPaymentPoint);
                            }
                        }
                    }
                }

                return forecastPayments;
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
    }
}
