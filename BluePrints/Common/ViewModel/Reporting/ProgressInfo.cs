using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProgressInfo
    {
        IEnumerable<DataPoint> rawDataPoints;
        readonly DateTime reportingDataDate;
        public readonly decimal BudgetedUnits;
        public readonly decimal TotalUnits;
        public readonly decimal BudgetedCosts;
        public readonly decimal TotalCosts;
        readonly IEnumerable<VariationAdjustment> rawVariationAdjustments;
        readonly bool hideDataPointsBeforeDataDate;
        readonly DateTime firstAlignedDataDate;
        readonly TimeSpan reportInterval;
        public bool FromP6 { get; private set; }

        /// <summary>
        /// Used For Workpack Summary Stats
        /// </summary>
        /// <param name="summaryStats">Project Summary Stats</param>
        public ProgressInfo(SummaryStats summaryStats)
        {
            this.reportingDataDate = summaryStats.ReportingDataDate;
            this.BudgetedUnits = summaryStats.BudgetedUnits;
            this.BudgetedCosts = summaryStats.BudgetedCosts;
            this.TotalUnits = summaryStats.TotalUnits;
            this.TotalCosts = summaryStats.TotalCosts;
            this.firstAlignedDataDate = summaryStats.FirstAlignedDataDate;
            this.reportInterval = summaryStats.ReportingInterval;
            this.rawVariationAdjustments = summaryStats.VariationAdjustments;
        }

        public ProgressInfo(DateTime reportingDataDate, decimal budgetedUnits, decimal totalUnits, decimal budgetedCosts, decimal totalCosts, DateTime firstAlignedDataDate, TimeSpan reportInterval, IEnumerable<VariationAdjustment> rawVariationAdjustments = null, bool hideDataPointsBeforeDataDate = false)
        {
            this.reportingDataDate = reportingDataDate;
            this.BudgetedUnits = budgetedUnits;
            this.TotalUnits = totalUnits;
            this.BudgetedCosts = budgetedCosts;
            this.TotalCosts = totalCosts;
            this.firstAlignedDataDate = firstAlignedDataDate;
            this.reportInterval = reportInterval;
            this.rawVariationAdjustments = rawVariationAdjustments;
            this.hideDataPointsBeforeDataDate = hideDataPointsBeforeDataDate;
        }

        public void SetData(IEnumerable<DataPoint> rawDataPoints)
        {
            this.rawDataPoints = rawDataPoints;
        }

        public IEnumerable<ExoDataPoint> ExoDataPoints
        {
            get
            {
                if (this.rawDataPoints == null)
                    return new List<ExoDataPoint>();

                IEnumerable<ExoDataPoint> exoDataPoints = this.rawDataPoints as IEnumerable<ExoDataPoint>;
                if (exoDataPoints != null)
                {
                    exoDataPoints = exoDataPoints.OrderBy(x => x.ProgressDate);
                    return exoDataPoints;
                }

                return new List<ExoDataPoint>();
            }
        }

        public List<DataPoint> GetData()
        {
            if (this.rawDataPoints == null)
                return new List<DataPoint>();

            return this.rawDataPoints.ToList();
        }

        public void SetFromP6()
        {
            this.FromP6 = true;
        }

        private ObservableCollection<DataPoint> cumulativeDataPoints { get; set; }
        public ObservableCollection<DataPoint> CumulativeDataPoints
        {
            get
            {
                if (cumulativeDataPoints == null && rawDataPoints != null && rawDataPoints.Count() > 0 && firstAlignedDataDate != null)
                {
                    cumulativeDataPoints = ISupportProgressReportingExtensions.GroupDataPointsByPeriod(rawDataPoints, BudgetedUnits, BudgetedCosts, firstAlignedDataDate, reportInterval, Guid.Empty, rawVariationAdjustments);
                }

                return cumulativeDataPoints;
            }
        }

        private ObservableCollection<DataPoint> dataPoints { get; set; }
        public ObservableCollection<DataPoint> DataPoints
        {
            get
            {
                if (dataPoints == null && CumulativeDataPoints != null && CumulativeDataPoints.Count() > 0 && reportingDataDate != null)
                {
                    DateTime? plotStartdate = hideDataPointsBeforeDataDate ? reportingDataDate : (DateTime ?)null;
                    dataPoints = ISupportProgressReportingExtensions.ConvertCumulativeToPeriodDataPoint(CumulativeDataPoints, plotStartdate);
                }

                return dataPoints;
            }
        }

        private DataPoint currentPeriodDataPoint { get; set; }
        public DataPoint CurrentPeriodDataPoint
        {
            get
            {
                if (currentPeriodDataPoint == null && reportingDataDate != null && CumulativeDataPoints != null && CumulativeDataPoints.Count() > 0 && reportingDataDate != null)
                    currentPeriodDataPoint = ISupportProgressReportingExtensions.GeneratePeriodDataPointFromCumulative(
                            CumulativeDataPoints, reportingDataDate.Date);

                return currentPeriodDataPoint;
            }
        }

        private DataPoint currentPeriodCumulativeDataPoint { get; set; }
        public DataPoint CurrentPeriodCumulativeDataPoint
        {
            get
            {
                if (currentPeriodCumulativeDataPoint == null && CumulativeDataPoints != null && CumulativeDataPoints.Count > 0 && reportingDataDate != null)
                    currentPeriodCumulativeDataPoint = ISupportProgressReportingExtensions.FindDataPointByDate(CumulativeDataPoints,
                            reportingDataDate.Date);
                return currentPeriodCumulativeDataPoint;
            }
        }

        public DateTime StartDate
        {
            get
            {
                if (DataPoints == null)
                    return new DateTime();

                return DataPoints.Min(x => x.ProgressDate);
            }
        }

        public DateTime EndDate
        {
            get
            {
                if (DataPoints == null)
                    return new DateTime();

                return DataPoints.Max(x => x.ProgressDate);
            }
        }
    }

    public class ExoDataPoint : DataPoint
    {
        public string WorkpackName { get; set; }
        public string ResourceName { get; set; }
    }

    public class DataPoint
    {
        public DateTime ProgressDate { get; set; }
        public decimal Units { get; set; }
        public decimal Costs { get; set; }
        //Used to store actuals while storing burn
        public decimal Actuals { get; set; }
        public decimal BudgetedUnits { get; set; }
        public decimal BudgetedCosts { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitsPercentage
        {
            get
            {
                if (BudgetedUnits == 0 || Units == 0)
                    return 0;
                else
                    return Units / BudgetedUnits;
            }
        }

        public decimal CostsPercentage
        {
            get
            {
                if (BudgetedCosts == 0 || Costs == 0)
                    return 0;
                else
                    return Costs / BudgetedCosts;
            }
        }
    }
}