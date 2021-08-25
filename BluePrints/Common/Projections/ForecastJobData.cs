using BaseModel.DataModel;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.ViewModels;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class ForecastJobData : EntityBase, IHaveDisciplineDesc, IForecastViewModel
    {
        public ForecastJobData()
        {
            DateCosts = new List<ForecastDateCost>();
        }

        public override string ToString()
        {
            if (Projection == null)
                return string.Empty;

            if (Projection.SubJobCode != null && Projection.DisciplineCode != null && Projection.CommodityCode != null)
                return Projection.SubJobCode + "-" + Projection.DisciplineCode + "-" + Projection.CommodityCode;
            else if (Projection.SubJobCode != null && Projection.DisciplineCode != null)
                return Projection.SubJobCode + "-" + Projection.DisciplineCode;
            else if (Projection.SubJobCode != null)
                return Projection.SubJobCode;
            else
                return string.Empty;
        }

        //used for compare data table to show type of cost that adds up to total forecast cost
        public string DropDownPhase { get; set; }       
        
        //used for show budget for indirects
        public decimal DropDownIndirectBudget { get; set; }

        public string CompareMask { get; set; }

        public ExoSubJobProjection Projection { get; set; }

        public List<ForecastDateCost> DateCosts { get; set; }

        public IEnumerable<IForecastDateCostViewModel> ForecastDateCosts => DateCosts;

        public List<ForecastJobData> CommodityJobs { get; set; }

        public IEnumerable<ExoTimeAuthorisation> RelevantJobLines { get; set; }

        public void SetRelevantJobLines(IEnumerable<ExoTimeAuthorisation> relevantJobLines)
        {
            if (Projection == null)
                throw new NotImplementedException();

            RelevantJobLines = relevantJobLines;
            Budget = relevantJobLines.Sum(x => x.BudgetCosts);
            Projection.ExoBudgetCosts = Budget;
            Rate = relevantJobLines.Sum(x => x.ForecastRate);
            Projection.ExoForecastRate = Rate;
        }

        //used for updating through view because relevant job lines hasn't been updated yet
        public void UpdateBudgetCost(decimal budgetCosts)
        {
            Budget = budgetCosts;
            Projection.ExoBudgetCosts = budgetCosts;
        }

        //used for updating through view because relevant job lines hasn't been updated yet
        public void UpdateBudgetRate(decimal budgetRate)
        {
            Rate = budgetRate;
            Projection.ExoForecastRate = budgetRate;
        }

        //construction job doesn't have deliverable total units, fall back to database stat total units
        public decimal DeliverableUnits { get; set; }

        public decimal Budget { get; set; }
        public decimal Rate { get; set; }
        public decimal Revenue { get; set; }
        public decimal CurrentBudget => Budget + Variation;
        public decimal Variation { get; set; }
        public decimal ActualCosts { get; set; }
        public decimal ActualUnits { get; set; }
        public decimal ActualUnitsPostDataDate { get; set; }
        public decimal ActualCostsPostDataDate { get; set; }
        public decimal ActualUnitsPreviousDataDate { get; set; }
        public decimal ActualCostsPreviousDataDate { get; set; }
        public decimal Invoiced { get; set; }
        public decimal Outstanding { get; set; }
        public decimal PreviousOutstanding { get; set; }
        public decimal P6BudgetedUnits { get; set; }
        public decimal P6RemainingUnits { get; set; }
        public decimal? EarnedUnits { get; set; }
        public decimal ProgressETC { get; set; }
        public decimal? P6RemainingUnitsOverride { get; set; }
        public decimal TotalCommitment => ActualCosts + Outstanding;
        public decimal? TotalCommitmentPreviousSaved { get; set; }
        public decimal TotalCommitmentPrevious => TotalCommitmentPreviousSaved != null ? (decimal)TotalCommitmentPreviousSaved : ActualCostsPreviousDataDate + PreviousOutstanding;
        public decimal TotalCommitmentDifference => TotalCommitment - TotalCommitmentPrevious;
        public bool IsProductivityFloating { get; set; }

        public decimal Productivity { get; set; }

        public decimal CurrentProductivity { get; set; }

        public decimal P6RemainingCosts { get; set; }
        public decimal PORemainingCosts { get; set; }
        public decimal Commitments => PORemainingCosts + P6RemainingCosts;
        public decimal OriginalUncommitted { get; set; }
        public decimal Uncommitted { get; set; }
        public decimal CurrentUncommitted { get; set; }
        public decimal TenderBudget { get; set; }
        public decimal PreviousEAC { get; set; }
        public decimal BudgetVariance => TenderBudget - Budget;
        public decimal EstimateToComplete => Outstanding + Uncommitted;
        public decimal OriginalEstimateAtCompletion => ActualCosts + Outstanding + OriginalUncommitted;
        public decimal EstimateAtCompletion => ActualCosts + Outstanding + Uncommitted;
        public decimal CurrentEstimateAtCompletion => ActualCosts + Outstanding + CurrentUncommitted;
        public decimal PeriodMovement => PreviousEAC - EstimateAtCompletion;
        public decimal PercentagePeriodMovement => PreviousEAC == 0 ? 0 : PeriodMovement / PreviousEAC;
        public bool IsContingency
        {
            get
            {
                if (Projection == null || Projection.SubJobCode == null || Projection.SubJobCode == string.Empty)
                    return false;

                return (Projection.CommodityCode == BluePrintsResources.ContingencyCostType);
            }
        }

        public bool IsProcurement
        {
            get
            {
                if (Projection == null || Projection.SubJobCode == null || Projection.SubJobCode == string.Empty)
                    return false;

                return Projection.SubJobCode.ToUpper().Contains("P");
            }
        }

        public bool IsConstruction
        {
            get
            {
                if (Projection == null || Projection.SubJobCode == null || Projection.SubJobCode == string.Empty)
                    return false;

                return Projection.SubJobCode.ToUpper().Contains("C");
            }
        }

        public bool IsDesign
        {
            get
            {
                if (Projection == null || Projection.SubJobCode == null || Projection.SubJobCode == string.Empty)
                    return false;

                return Projection.SubJobCode.ToUpper().Contains("D");
            }
        }

        public bool IsIndirect
        {
            get
            {
                if (Projection == null || Projection.SubJobCode == null || Projection.SubJobCode == string.Empty)
                    return false;

                return Projection.SubJobCode.ToUpper().Contains("I");
            }
        }

        public decimal PctCompleteCosts => EstimateAtCompletion == 0 ? 1 : ActualCosts / EstimateAtCompletion;
        public decimal PctCompleteUnits => P6BudgetedUnits == 0 ? 1 : (P6BudgetedUnits - P6RemainingUnits) / P6BudgetedUnits;
        public decimal PctComplete => IsProcurement ? PctCompleteCosts : PctCompleteUnits;
        public decimal Variance => Budget - EstimateAtCompletion;
        public bool IsBudgetReadOnly { get; set; }
        public bool IsPOError { get; set; }
        public decimal IsPOErrorImageWidth => IsPOError ? 15 : 0;
        public decimal IsCommodityCodeErrorImageWidth => IsCommodityCodeError ? 15 : 0;
        public decimal IsErrorMessageImageWidth => JobErrorMessage == string.Empty ? 0 : 15;
        public string JobErrorMessage { get; set; }
        public RATE FallBackRate { get; set; }

        public decimal P6NominalRate => P6RemainingUnits == 0 ? FallBackRate == null ? 0 : FallBackRate.RATE1 == null ? 0 : (decimal)FallBackRate.RATE1 : P6RemainingCosts / P6RemainingUnits;

        //used by detailed rows so that only P6 hour row can be edited
        public bool IsP6HoursRow { get; set; }

        #region Indirect details
        public string Description { get; set; }
        public string Source { get; set; }
        public string Reference { get; set; }
        public string Note { get; set; }
        public string UOM { get; set; }

        //used in indirect job forecast
        public decimal? JobRate { get; set; }
        #endregion

        #region Commodity Codes
        private IEnumerable<COMMODITY_CODE> COMMODITY_CODES { get; set; }
        public void PopulateCommodityCodes(IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection)
        {
            validCommodityCodes = null;
            COMMODITY_CODES = COMMODITY_CODECollection;
        }

        public List<COMMODITY_CODE> validCommodityCodes = null;
        public IEnumerable<COMMODITY_CODE> ValidCommodityCodes
        {
            get
            {
                if (COMMODITY_CODES == null || Projection == null || Projection.PhaseType == null || Projection.DisciplineCode == null || Projection.DisciplineCode.Length < 2)
                    return new List<COMMODITY_CODE>();

                if (validCommodityCodes == null)
                {
                    validCommodityCodes = BluePrintsDataUtils.FilterForValidCommodityCodes(COMMODITY_CODES, Projection.DisciplineCode, Projection.PhaseType).ToList();
                }

                return validCommodityCodes;
            }
        }

        public bool IsCommodityCodeError
        {
            get
            {
                if (Projection == null)
                    return true;

                if (Projection.DisciplineCode == null || Projection.DisciplineCode == string.Empty)
                    return false;

                if (Projection.CommodityCode.Length < 2 || Projection.CommodityCode == null || ValidCommodityCodes.Count() == 0)
                    return true;

                if (Projection.CommodityCode == BluePrintsResources.ContingencyCostType)
                    return false;

                if (Projection.PhaseType == Common.PhaseType.Tender)
                {
                    if (Projection.CommodityCode.Substring(0, 2) == BluePrintsResources.Default_TenderCommodityCode)
                        return false;
                    else
                        return true;
                }

                bool isCommodityCodeError = !ValidCommodityCodes.Any(x => x.CODE == Projection.CommodityCode);
                return isCommodityCodeError;
            }
        }

        public string DisciplineCode
        {
            get
            {
                if (Projection == null)
                    return string.Empty;

                return Projection.DisciplineCode;
            }
        }

        public string DisciplineDesc { get; set; }
        #endregion
    }

    public class ForecastDateCost : IForecastDateCostViewModel
    {
        public readonly DateTime FloorDate;
        public readonly DateTime CeilingDate;
        private readonly DateTime firstViewDate;
        private readonly DateTime firstForecastDate;
        public ForecastDateCost(DateTime date, DateTime firstViewDate, DateTime dataDate, bool isWeeks)
        {
            Date = date;
            this.firstViewDate = firstViewDate;
            this.firstForecastDate = dataDate;

            if (isWeeks)
            {
                firstForecastDate = firstForecastDate.Date.AddDays(7);
                FloorDate = date.Date.AddDays(-6);
                CeilingDate = date.Date;
            }
            else
            {
                firstForecastDate = new DateTime(dataDate.Date.Year, dataDate.Date.Month, 1).AddMonths(2).AddDays(-1);
                FloorDate = new DateTime(date.Date.Year, date.Date.Month, 1);
                CeilingDate = FloorDate.AddMonths(1).AddDays(-1);
            }
        }

        public IEnumerable<ExoDataPoint> MaterialDataPoints { get; set; }
        public IEnumerable<ExoDataPoint> ActualDataPoints { get; set; }
        public IEnumerable<FORECAST_PO> FORECAST_POS { get; set; }
        public IEnumerable<Common.ViewModel.Reporting.DataPoint> P6RemainingDataPoints { get; set; }
        public IEnumerable<RemainingCost> IndirectRemainingCosts { get; set; }
        public IEnumerable<FORECAST_EAC> FORECAST_EACS { get; set; }

        //data points for each datecost period
        public IEnumerable<ExoDataPoint> CurrentPeriodActualDataPoints => ActualDataPoints.Where(x => x.ActualDate >= ActualFloorDate && x.ActualDate <= CeilingDate);
        public IEnumerable<ExoDataPoint> CurrentPeriodMaterialDataPoints => MaterialDataPoints.Where(x => x.ActualDate >= ActualFloorDate && x.ActualDate <= CeilingDate);
        public IEnumerable<FORECAST_PO> CurrentPeriodForecastPOs => POAndIndirectForecastFloorDate != null ? FORECAST_POS.Where(x => x.FORECAST_DATE >= POAndIndirectForecastFloorDate && x.FORECAST_DATE <= CeilingDate).Where(x => x.FORECAST_VALUE != null) : new List<FORECAST_PO>();
        public IEnumerable<FORECAST_EAC> CurrentPeriodForecastEACs => FORECAST_EACS.Where(x => x.FORECAST_COSTS != null).Where(x => x.FORECAST_DATE >= ActualFloorDate && x.FORECAST_DATE <= CeilingDate);
        public IEnumerable<RemainingCost> CurrentPeriodIndirectCosts => POAndIndirectForecastFloorDate != null ? IndirectRemainingCosts.Where(x => x.ForecastDate.Date >= POAndIndirectForecastFloorDate && x.ForecastDate.Date <= CeilingDate) : new List<RemainingCost>();
        public IEnumerable<Common.ViewModel.Reporting.DataPoint> CurrentPeriodP6DataPoints => P6RemainingFloorDate != null ? P6RemainingDataPoints.Where(x => x.ProgressDate.Date >= P6RemainingFloorDate && x.ProgressDate.Date <= CeilingDate) : new List<Common.ViewModel.Reporting.DataPoint>();

        //relevant data points used to get unique stock item
        public IEnumerable<ExoDataPoint> RelevantActualDataPoints => ActualDataPoints.Where(x => x.ActualDate > firstViewDate);
        public IEnumerable<ExoDataPoint> RelevantMaterialDataPoints => MaterialDataPoints.Where(x => x.ActualDate > firstViewDate);
        public IEnumerable<FORECAST_PO> RelevantForecastPOs => FORECAST_POS.Where(x => x.FORECAST_DATE > firstViewDate).Where(x => x.FORECAST_VALUE != null);
        public IEnumerable<RemainingCost> RelevantIndirectCosts => IndirectRemainingCosts.Where(x => x.ForecastDate.Date > firstViewDate);

        //show actuals by summing up from beginning of time on first date
        private DateTime ActualFloorDate => Date == firstViewDate ? new DateTime(1) : FloorDate;
        //only show po forecast after actuals date without it summing up from beginning of time on first date
        private DateTime? POAndIndirectForecastFloorDate => FloorDate > firstViewDate ? FloorDate : (DateTime?)null;
        //only show p6 remaining after actuals date and have it summing up from beginning of time on first date
        private DateTime? P6RemainingFloorDate => CeilingDate >= firstForecastDate ? CeilingDate == firstForecastDate ? new DateTime(2010, 1, 1) : FloorDate : (DateTime?)null;

        public DateTime Date { get; set; }

        //not using this as a measure because user can override it
        public decimal ActualCosts => CurrentPeriodActualDataPoints.Sum(x => x.Costs);
        public decimal ActualUnits => CurrentPeriodActualDataPoints.Sum(x => x.Quantity);
        public decimal MaterialCosts => CurrentPeriodMaterialDataPoints.Sum(x => x.Costs);
        public decimal MaterialQuantity => CurrentPeriodMaterialDataPoints.Sum(x => x.Quantity);
        public decimal P6Hours => CurrentPeriodP6DataPoints.Sum(x => x.Units);
        public decimal P6Costs => CurrentPeriodP6DataPoints.Sum(x => x.Costs);
        public decimal POForecastCosts => CurrentPeriodForecastPOs.Sum(x => (decimal)x.FORECAST_VALUE);
        public decimal EACCosts => CurrentPeriodForecastEACs.Sum(x => (decimal)x.FORECAST_COSTS);
        public decimal IndirectForecastCosts => CurrentPeriodIndirectCosts.Sum(x => x.ForecastRemainingCosts);
        public decimal TotalCosts => ActualCosts + MaterialCosts + P6Costs + POForecastCosts + IndirectForecastCosts;

        //p6 costs needs to be categorised as uncommitted
        public decimal CommittedCosts => ActualCosts + MaterialCosts + POForecastCosts;
    }
}
