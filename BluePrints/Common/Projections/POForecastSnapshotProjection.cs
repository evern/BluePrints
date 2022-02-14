using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class POForecastSnapshotProjection : EntityBase
    {
        public string PONO { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string VariationCode { get; set; }
        public string Comments { get; set; }
        public DateTime ActualCutOffDate { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> CurrentPOSnapshots { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> CurrentActualSnapshots { get; set; }
        public List<FORECAST_PO> FORECAST_POs { get; set; }
        public decimal TotalForecast => FORECAST_POs.Where(x => x.FORECAST_DATE > ActualCutOffDate).Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
        public decimal Unforecasted => PO_RemainingPrice - TotalForecast;
        public bool IsPOError => Math.Round(Unforecasted) != 0;
        public decimal ErrorImageWidth => IsPOError ? 15 : 0;

        public POForecastSnapshotProjection()
        {
            FORECAST_POs = new List<FORECAST_PO>();
        }

        public DateTime? FirstActualDate { get; set; }

        public void UpdateForecastPayments(IEnumerable<FORECAST_PO> allFORECAST_POs, IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> allActuals, DateTime actualCutOffDate)
        {
            ActualCutOffDate = actualCutOffDate;
            CurrentActualSnapshots?.Clear();
            FORECAST_POs.Clear();
            ResetPaymentDates();
            IEnumerable<FORECAST_PO> currentPOForecasts = getPOForecasts(allFORECAST_POs);
            CurrentActualSnapshots = getCurrentActuals(allActuals);
            foreach(FORECAST_PO currentPOForecast in currentPOForecasts)
            {
                //because forecast can sometimes store outdated job code, cost group and cost type, validation is required before adding, else forecast PO can show that it's forecasted but forecast module will pick it up on the wrong code
                if(CurrentPOSnapshots.Any(x => x.SUBJOB_CODE == currentPOForecast.JOB_CODE && x.DISCIPLINE_CODE == currentPOForecast.DISCIPLINE_CODE && x.COMMODITY_CODE == currentPOForecast.COMMODITY_CODE && x.STOCK_CODE == currentPOForecast.STOCK_CODE && x.VARIATION_CODE == currentPOForecast.VARIATION_CODE))
                    FORECAST_POs.Add(currentPOForecast);
            }

            this.Update();
        }

        protected virtual List<FORECAST_PO> getPOForecasts(IEnumerable<FORECAST_PO> allFORECAST_POs)
        {
            return allFORECAST_POs.Where(x => x.PONO == this.PONO && x.VARIATION_CODE == this.VariationCode).ToList();
        }

        protected virtual List<FORECAST_JOB_HOUR_SNAPSHOT> getCurrentActuals(IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> allActuals)
        {
            return allActuals.Where(x => x.PO_NUMBER == this.PONO && x.VARIATION_CODE == this.VariationCode).ToList();
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
                    if (FORECAST_POs != null)
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

        List<ExoDataPoint> forecastActuals { get; set; }
        public IEnumerable<ExoDataPoint> ForecastActuals
        {
            get
            {
                if (forecastActuals == null)
                {
                    forecastActuals = new List<ExoDataPoint>();
                    if (CurrentActualSnapshots != null)
                    {
                        var groupByDateFORECASTS = CurrentActualSnapshots.Where(x => x.FORECAST_DATE != null).GroupBy(x => x.FORECAST_DATE).Select(g => new { ForecastDate = (DateTime)g.Key, ForecastCost = g.Where(x => x.FORECAST_DATE != null).Sum(x => (decimal)x.FORECAST_COST) }).OrderBy(x => x.ForecastDate);
                        foreach (var groupByDateFORECAST in groupByDateFORECASTS)
                        {
                            ExoDataPoint forecastPaymentPoint = new ExoDataPoint();

                            forecastPaymentPoint.Costs = groupByDateFORECAST.ForecastCost;
                            forecastPaymentPoint.ActualDate = groupByDateFORECAST.ForecastDate;

                            forecastActuals.Add(forecastPaymentPoint);
                        }
                    }
                }

                return forecastActuals;
            }
        }

        public void ResetPaymentDates()
        {
            forecastPayments = null;
        }

        public decimal PO_SuppliedQty
        {
            get
            {
                if (CurrentActualSnapshots == null)
                    return 0;

                return CurrentActualSnapshots.Sum(x => x.FORECAST_QTY);
            }
        }

        public decimal PO_OrderQuantity
        {
            get
            {
                if (CurrentPOSnapshots == null)
                    return 0;

                return CurrentPOSnapshots.Sum(x => x.FORECAST_QTY);
            }
        }

        public decimal PO_Quantity
        {
            get => PO_OrderQuantity - PO_SuppliedQty;
        }

        public decimal PO_RemainingPrice
        {
            get
            {
                if (CurrentActualSnapshots == null)
                    return 0;

                return CurrentPOSnapshots.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal PO_Invoiced
        {
            get
            {
                if (CurrentActualSnapshots == null)
                    return 0;

                return CurrentActualSnapshots.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal PO_TotalPrice
        {
            get
            {
                if (CurrentActualSnapshots == null)
                    return 0;

                IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> POSnapshotsWithTotal = CurrentPOSnapshots.Where(x => x.PO_TOTAL != null);
                if (POSnapshotsWithTotal.Count() > 0)
                    return (decimal)POSnapshotsWithTotal.Sum(x => x.PO_TOTAL);
                else
                    return 0;
            }
        }
    }
}
