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
                        if(ForecastConfig == null)
                        {
                            ExoDataPoint forecastPaymentPoint = new ExoDataPoint();
                            forecastPaymentPoint.Costs = PO_RemainingPrice;
                            forecastPaymentPoint.ActualDate = DateTime.Now.Date.AddMonths(1);
                            forecastPayments.Add(forecastPaymentPoint);
                        }
                        else if(ForecastConfig.MODE != POPaymentTerms.Custom)
                        {
                            decimal remainingPeriod = RemainingPeriod;

                            decimal costPerPeriod = PO_RemainingPrice / remainingPeriod;
                            DateTime forecastDate = LastActionDate;
                            forecastDate = forecastDate.AddMonths(monthsForward);

                            do
                            {
                                if (forecastDate.Month < DateTime.Now.Month)
                                {
                                    forecastDate = forecastDate.AddMonths(monthsForward);
                                    continue;
                                }

                                ExoDataPoint forecastPaymentPoint = new ExoDataPoint();
                                forecastPaymentPoint.Costs = costPerPeriod;
                                forecastPaymentPoint.ActualDate = forecastDate;
                                forecastPayments.Add(forecastPaymentPoint);

                                forecastDate = forecastDate.AddMonths(monthsForward);
                                remainingPeriod -= 1;
                            } while (remainingPeriod > 0);
                        }
                        else
                        {
                            foreach(PO_CUSTOMDATE customPaymentDate in CustomPaymentDates)
                            {
                                if (customPaymentDate.PAYMENT_DATE.Month < DateTime.Now.Month)
                                    continue;

                                ExoDataPoint forecastPaymentPoint = new ExoDataPoint();
                                forecastPaymentPoint.Costs = PO_RemainingPrice * customPaymentDate.PAYMENT_PERCENT;
                                forecastPaymentPoint.ActualDate = customPaymentDate.PAYMENT_DATE.Date;
                                forecastPayments.Add(forecastPaymentPoint);
                            }
                        }
                    }

                    if(forecastPayments.Count == 0)
                    {
                        //fallback when there are nothing added
                        ExoDataPoint fallbackPaymentPoint = new ExoDataPoint();
                        fallbackPaymentPoint.Costs = PO_RemainingPrice;
                        fallbackPaymentPoint.ActualDate = DateTime.Now.Date.AddMonths(1);
                        forecastPayments.Add(fallbackPaymentPoint);
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

        private int monthsForward
        {
            get
            {
                if (PaymentTerms == POPaymentTerms.Thirty_Days)
                    return 1;
                else if (PaymentTerms == POPaymentTerms.Sixty_Days)
                    return 2;
                else if (PaymentTerms == POPaymentTerms.Ninety_Days)
                    return 3;
                else
                    return 1;
            }
        }

        public bool IsCustom
        {
            get
            {
                return ForecastConfig != null && ForecastConfig.MODE == POPaymentTerms.Custom;
            }
        }

        public decimal RemainingPeriodEdit
        {
            get
            {
                return RemainingPeriod;
            }
            set
            {
                if(ForecastConfig != null)
                    ForecastConfig.REMAINING_PERIOD = value;
            }
        }

        public decimal RemainingPeriod
        {
            get
            {
                if (ForecastConfig == null)
                    return 1;

                decimal remainingPeriod = PaymentTerms == POPaymentTerms.Custom ? CustomPaymentDates.Count : ForecastConfig.REMAINING_PERIOD;
                decimal elapsedPeriodSinceRecordCreated = 0;

                DateTime loopDate = DateTime.Now;
                loopDate = loopDate.Date.AddMonths(-1 * (monthsForward));

                while (loopDate.Date > LastActionDate.Date)
                {
                    elapsedPeriodSinceRecordCreated += 1;
                    loopDate = loopDate.Date.AddMonths(-1 * (monthsForward));
                };

                remainingPeriod -= elapsedPeriodSinceRecordCreated;
                if (remainingPeriod <= 0)
                    remainingPeriod = 1;

                return remainingPeriod;
            }
        }

        public DateTime LastActionDate
        {
            get
            {
                if (ForecastConfig == null)
                    return DateTime.Now.Date;

                return ForecastConfig.UPDATED == null ? ForecastConfig.CREATED : (DateTime)ForecastConfig.UPDATED;
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
