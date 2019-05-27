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
                            DateTime forcastDate = ((DateTime)InvoiceDate).AddDays(forwardTimeSpan.Days);
                            forecastPaymentPoint.ActualDate = forcastDate;
                            forecastPayments.Add(forecastPaymentPoint);
                        }
                        else
                        {
                            foreach(PO_CUSTOMDATE customPaymentDate in CustomPaymentDates)
                            {
                                ExoDataPoint forecastPaymentPoint = new ExoDataPoint();
                                forecastPaymentPoint.Costs = PO_RemainingPrice * customPaymentDate.PAYMENT_PERCENT;
                                forecastPaymentPoint.ActualDate = customPaymentDate.PAYMENT_DATE.Date;
                                forecastPayments.Add(forecastPaymentPoint);
                            }
                        }
                    }
                }

                return forecastPayments;
            }
        }

        public void SaveForecastPaymentDates(IBluePrintsEntitiesUnitOfWork unitOfWork)
        {
            if (ForecastConfig != null)
            {
                IQueryable<FORECAST_PO_RESULT> results = unitOfWork.FORECAST_PO_RESULTS.Where(x => x.GUID_FORECAST_PO == ForecastConfig.GUID);
                List<FORECAST_PO_RESULT> removeResults = new List<FORECAST_PO_RESULT>();
                foreach (FORECAST_PO_RESULT result in results)
                {
                    ExoDataPoint payment = ForecastPayments.FirstOrDefault(x => x.PONumber == PONO && x.ActualDate.Date == result.FORECAST_DATE.Date);
                    if (payment == null)
                        removeResults.Add(result);
                }

                foreach(FORECAST_PO_RESULT removeResult in removeResults)
                {
                    unitOfWork.FORECAST_PO_RESULTS.Remove(removeResult);
                }

                foreach(ExoDataPoint payment in ForecastPayments)
                {
                    FORECAST_PO_RESULT result = results.FirstOrDefault(x => x.PONO == PONO && x.FORECAST_DATE == payment.ActualDate);
                    if(result != null)
                    {
                        result.FORECAST_AMOUNT = payment.Costs;
                    }
                    else
                    {
                        FORECAST_PO_RESULT newResult = new FORECAST_PO_RESULT();
                        newResult.PONO = PONO;
                        newResult.FORECAST_AMOUNT = payment.Costs;
                        newResult.FORECAST_DATE = payment.ActualDate.Date;
                        newResult.GUID_FORECAST_PO = ForecastConfig.GUID;
                        unitOfWork.FORECAST_PO_RESULTS.Add(newResult);
                    }
                }

                unitOfWork.SaveChanges();
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
