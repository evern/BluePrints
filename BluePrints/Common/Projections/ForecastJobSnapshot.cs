using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.ViewModels;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class ForecastJobSnapshot : CodesValidationModel, IHaveDisciplineDesc, IForecastViewModel
    {
        public List<ForecastDateSnapshot> DateCosts { get; set; }
        private readonly UniqueForecastJob uniqueForecastJob;
        public ForecastJobSnapshot()
        {
            DateCosts = new List<ForecastDateSnapshot>();
        }

        public ForecastJobSnapshot(UniqueForecastJob uniqueForecastJob, List<ExoSubJobProjection> projectLines, IEnumerable<FORECAST_JOB_SETTING> FORECAST_JOB_SETTINGCollection, IEnumerable<FORECAST_EAC> FORECAST_EACCollection, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, DateTime previousEACDataDate, bool isBudgetReadOnly)
        {
            this.uniqueForecastJob = uniqueForecastJob;
            DateCosts = new List<ForecastDateSnapshot>();
            PopulateCompulsoryDataForForecastJobSnapshot(FORECAST_EACCollection, COMMODITY_CODECollection, previousEACDataDate);
            SubJobCode = uniqueForecastJob.SUBJOB_CODE;
            DisciplineCode = uniqueForecastJob.DISCIPLINE_CODE;
            CommodityCode = uniqueForecastJob.COMMODITY_CODE;
            VariationCode = uniqueForecastJob.VARIATION_CODE;
            Budget = uniqueForecastJob.BudgetCosts;
            Outstanding = uniqueForecastJob.POOutstandingCosts;
            P6BudgetedUnits = uniqueForecastJob.P6BudgetHours;
            P6RemainingUnits = uniqueForecastJob.P6RemainingHours;
            P6RemainingCosts = uniqueForecastJob.P6RemainingCosts;
            TenderBudget = uniqueForecastJob.TenderBudget;
            JobErrorMessage = uniqueForecastJob.ErrorMessage;
            ActualUnits = uniqueForecastJob.ActualCollection.Sum(x => x.FORECAST_QTY);
            ActualCosts = uniqueForecastJob.ActualCollection.Sum(x => x.FORECAST_COST);
            EarnedUnits = uniqueForecastJob.EarnedCollection.Sum(x => x.FORECAST_QTY);
            PORemainingCosts = uniqueForecastJob.POCollection.Sum(x => x.FORECAST_COST);
            ProgressETC = uniqueForecastJob.ProgressETCCollection.Sum(x => x.FORECAST_QTY);
            ActualCostsPreviousDataDate = uniqueForecastJob.ActualCollection.Sum(x => x.FORECAST_COST);
            ActualUnitsPreviousDataDate = uniqueForecastJob.ActualCollection.Sum(x => x.FORECAST_QTY);
            ActualCostsPostDataDate = uniqueForecastJob.FutureActualCollection.Sum(x => x.FORECAST_COST);
            ActualUnitsPostDataDate = uniqueForecastJob.FutureActualCollection.Sum(x => x.FORECAST_QTY);
            IsBudgetReadOnly = isBudgetReadOnly;
            PopulateLookupAttributes(projectLines, FORECAST_JOB_SETTINGCollection);
        }

        public void PopulateLookupAttributes(List<ExoSubJobProjection> projectLines, IEnumerable<FORECAST_JOB_SETTING> FORECAST_JOB_SETTINGCollection)
        {
            if (VariationCode == null || VariationCode == string.Empty)
                ExoJob = projectLines.FirstOrDefault(x => x.SubJobCode == SubJobCode && x.DisciplineCode == DisciplineCode && x.CommodityCode == CommodityCode && (x.VariationCode == null || x.VariationCode == string.Empty));
            else
                ExoJob = projectLines.FirstOrDefault(x => x.SubJobCode == SubJobCode && x.DisciplineCode == DisciplineCode && x.CommodityCode == CommodityCode && x.VariationCode == VariationCode);

            if (ExoJob == null)
            {
                ExoJob = new ExoSubJobProjection() { SubJobCode = SubJobCode, SubJobTitle = string.Empty, DisciplineCode = DisciplineCode, DisciplineName = string.Empty, CommodityCode = CommodityCode, CommodityName = string.Empty, CommodityDescription = string.Empty, CommodityUOM = string.Empty, VariationCode = VariationCode };
            }

            //set whether productivity is floating
            if (FORECAST_JOB_SETTINGCollection.Where(x => x.SUBJOB_CODE == SubJobCode && x.DISCIPLINE_CODE == DisciplineCode && x.COMMODITY_CODE == CommodityCode && x.VARIATION_CODE == VariationCode && x.IS_FLOATING_PRODUCTIVITY).Count() > 0)
                IsProductivityFloating = true;
        }

        public void PopulateCompulsoryDataForForecastJobSnapshot(IEnumerable<FORECAST_EAC> FORECAST_EACCollection, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, DateTime previousEACDataDate)
        {
            ForecastHelper.PopulateEAC(this, FORECAST_EACCollection, previousEACDataDate);
            PopulateCommodityCodes(COMMODITY_CODECollection);
        }

        public void RefreshErrorMessage(ExoSubJobProjection projectLine, IQueryable<X_JOBCOST_LINES_AUDIT> X_JOBCOST_LINES_AUDITQueryable)
        {
            if(uniqueForecastJob != null)
            {
                uniqueForecastJob.ProjectLine = projectLine;
                JobErrorMessage = uniqueForecastJob.ErrorMessage;
                uniqueForecastJob.UpdateErrorMessage(X_JOBCOST_LINES_AUDITQueryable);
                JobErrorMessage = uniqueForecastJob.ErrorMessage;
            }
        }

        public string PhaseCode => BluePrintsDataUtils.GetPhaseCode(SubJobCode);

        public string AreaCode => BluePrintsDataUtils.GetAreaCode(SubJobCode);

        public string SubAreaCode => BluePrintsDataUtils.GetSubAreaCode(SubJobCode);
        public ExoSubJobProjection ExoJob { get; set; }

        public string SubJobCode { get; set; }
        public string DisciplineCode { get; set; }
        public string DisciplineDesc { get; set; }
        public string CommodityCode { get; set; }
        public string CommodityName => ExoJob == null ? string.Empty : ExoJob.CommodityName;
        public string VariationCode { get; set; }
        public string DropDownPhase { get; set; }
        public decimal TenderBudget { get; set; }
        public decimal Budget { get; set; }
        public decimal P6BudgetedUnits { get; set; }
        public decimal BudgetVariance => TenderBudget - Budget;
        public decimal ActualCosts { get; set; }
        public decimal ActualUnits { get; set; }
        public string CompareMask { get; set; }
        public bool IsBudgetReadOnly { get; set; }

        #region IForecastViewModel
        //used by detailed rows so that only P6 hour row can be edited
        public bool IsP6HoursRow { get; set; }
        public decimal DropDownIndirectBudget { get; set; }

        #region Rate Members
        public decimal P6RemainingUnits { get; set; }
        public decimal P6RemainingCosts { get; set; }
        public decimal P6NominalRate => P6RemainingUnits == 0 ? 0 : P6RemainingCosts / P6RemainingUnits;
        public bool IsProductivityFloating { get; set; }
        #endregion

        public bool IsCommodityCodeError => !IsCommodityCodeValid;
        public decimal IsCommodityCodeErrorImageWidth => IsCommodityCodeError ? 15 : 0;
        public decimal IsErrorMessageImageWidth => JobErrorMessage == null || JobErrorMessage == string.Empty ? 0 : 15;
        public string JobErrorMessage { get; set; }
        public decimal Productivity { get; set; }
        //store P6 units either native or from override
        public decimal? P6RemainingUnitsOverride { get; set; }
        public decimal ProgressETC { get; set; }
        public IEnumerable<IForecastDateCostViewModel> ForecastDateCosts => DateCosts;
        #endregion

        #region PO Errors
        public decimal PORemainingCosts { get; set; }
        public decimal Outstanding { get; set; }
        public decimal PreviousOutstanding { get; set; }
        public bool IsPOError { get; set; }
        public decimal IsPOErrorImageWidth => IsPOError ? 15 : 0;
        #endregion

        public decimal? EarnedUnits { get; set; }
        public decimal Uncommitted { get; set; }
        public decimal CurrentUncommitted { get; set; }
        public decimal OriginalUncommitted { get; set; }
        public decimal ActualUnitsPostDataDate { get; set; }
        public decimal ActualCostsPostDataDate { get; set; }
        public decimal ActualUnitsPreviousDataDate { get; set; }
        public decimal ActualCostsPreviousDataDate { get; set; }
        public decimal PctCompleteCosts => EstimateAtCompletion == 0 ? 1 : ActualCosts / EstimateAtCompletion;
        public decimal PctCompleteUnits => P6BudgetedUnits == 0 ? 1 : (P6BudgetedUnits - P6RemainingUnits) / P6BudgetedUnits;
        public decimal PctComplete => IsProcurement ? PctCompleteCosts : PctCompleteUnits;

        public decimal CurrentProductivity
        {
            get
            {
                if (EarnedUnits != null && ActualUnits != 0)
                {
                    if (EarnedUnits > 0)
                        return (decimal)EarnedUnits / ActualUnits;
                    else
                        return 0.00m;
                }
                else
                    return 0.00m;
            }
        }

        public bool IsContingency
        {
            get
            {
                if (SubJobCode == null || SubJobCode == string.Empty)
                    return false;

                return CommodityCode == BluePrintsResources.ContingencyCostType;
            }
        }

        public bool IsProcurement
        {
            get
            {
                if (SubJobCode == null || SubJobCode == string.Empty)
                    return false;

                return SubJobCode.ToUpper().Contains("P");
            }
        }

        public bool IsConstruction
        {
            get
            {
                if (SubJobCode == null || SubJobCode == string.Empty)
                    return false;

                return SubJobCode.ToUpper().Contains("C");
            }
        }

        public bool IsDesign
        {
            get
            {
                if (SubJobCode == null || SubJobCode == string.Empty)
                    return false;

                return SubJobCode.ToUpper().Contains("D");
            }
        }

        public bool IsIndirect
        {
            get
            {
                if (SubJobCode == null || SubJobCode == string.Empty)
                    return false;

                return SubJobCode.ToUpper().Contains("I");
            }
        }

        Dictionary<string, decimal> poStockCodeAttributes = null;
        public Dictionary<string, decimal> POStockCodeAttributes
        {
            get
            {
                if (poStockCodeAttributes == null)
                {
                    poStockCodeAttributes = new Dictionary<string, decimal>();
                    var groupByStockCodeSnapshots = DateCosts.SelectMany(x => x.POForecastSnapshots).GroupBy(x => x.STOCK_CODE).Select(group => new { StockCode = group.Key, Budget = group.First().PROJECT_BUDGET });
                    foreach (var groupByStockCodeSnapshot in groupByStockCodeSnapshots)
                    {
                        poStockCodeAttributes.Add(groupByStockCodeSnapshot.StockCode, groupByStockCodeSnapshot.Budget);
                    }
                }

                return poStockCodeAttributes;
            }
        }


        Dictionary<string, decimal> indirectStockCodeAttributes = null;
        public Dictionary<string, decimal> IndirectStockCodeAttributes
        {
            get
            {
                if (indirectStockCodeAttributes == null)
                {
                    indirectStockCodeAttributes = new Dictionary<string, decimal>();
                    var groupByStockCodeSnapshots = DateCosts.SelectMany(x => x.IndirectForecastSnapshots).GroupBy(x => x.STOCK_CODE).Select(group => new { StockCode = group.Key, Budget = group.First().PROJECT_BUDGET });
                    foreach (var groupByStockCodeSnapshot in groupByStockCodeSnapshots)
                    {
                        indirectStockCodeAttributes.Add(groupByStockCodeSnapshot.StockCode, groupByStockCodeSnapshot.Budget);
                    }
                }

                return indirectStockCodeAttributes;
            }
        }

        Dictionary<string, decimal> actualStockCodeAttributes = null;
        public Dictionary<string, decimal> ActualStockCodeAttributes
        {
            get
            {
                if (actualStockCodeAttributes == null)
                {
                    actualStockCodeAttributes = new Dictionary<string, decimal>();
                    var groupByStockCodeSnapshots = DateCosts.SelectMany(x => x.ActualForecastSnapshots).GroupBy(x => x.STOCK_CODE).Select(group => new { StockCode = group.Key, Budget = group.First().PROJECT_BUDGET });
                    foreach (var groupByStockCodeSnapshot in groupByStockCodeSnapshots)
                    {
                        actualStockCodeAttributes.Add(groupByStockCodeSnapshot.StockCode, groupByStockCodeSnapshot.Budget);
                    }
                }

                return actualStockCodeAttributes;
            }
        }

        public string ErrorMessageIdentificationCode
        {
            get
            {
                string subJobCode = SubJobCode == null || SubJobCode == string.Empty ? "(Missing)" : SubJobCode;
                string disciplineCode = DisciplineCode == null || DisciplineCode == string.Empty ? "(Missing)" : DisciplineCode;
                string commodityCode = CommodityCode == null || CommodityCode == string.Empty ? "(Missing)" : CommodityCode;

                return subJobCode + "-" + disciplineCode + "-" + commodityCode;
            }
        }

        #region Summaries
        public decimal EstimateToComplete => Outstanding + Uncommitted;
        public decimal OriginalEstimateAtCompletion => ActualCosts + Outstanding + OriginalUncommitted;
        public decimal EstimateAtCompletion => ActualCosts + Outstanding + Uncommitted;
        public decimal CurrentEstimateAtCompletion => ActualCosts + Outstanding + CurrentUncommitted;
        public decimal Variance => Budget - EstimateAtCompletion;
        public decimal TotalCommitment => ActualCosts + Outstanding;
        public decimal PeriodMovement => PreviousEAC - EstimateAtCompletion;
        public decimal PercentagePeriodMovement => PreviousEAC == 0 ? 0 : PeriodMovement / PreviousEAC;
        public decimal? TotalCommitmentPreviousSaved { get; set; }
        public decimal TotalCommitmentPrevious => TotalCommitmentPreviousSaved != null ? (decimal)TotalCommitmentPreviousSaved : ActualCostsPreviousDataDate + PreviousOutstanding;
        public decimal TotalCommitmentDifference => TotalCommitment - TotalCommitmentPrevious;
        public decimal PreviousEAC { get; set; }
        #endregion

        #region Codes Validation
        protected override string disciplineCodePropertyName => BindableBase.GetPropertyName(() => new ForecastJobSnapshot().DisciplineCode);

        protected override string commodityCodePropertyName => BindableBase.GetPropertyName(() => new ForecastJobSnapshot().CommodityCode);

        protected override string stockCodePropertyName => string.Empty;

        protected override string exoBudgetPropertyName => string.Empty;

        protected override string subJobCode => SubJobCode;

        protected override string disciplineCode => DisciplineCode;

        protected override string commodityCode => CommodityCode;

        protected override string stockCode => string.Empty;

        protected override decimal exoBudget => 0;

        protected override decimal budget => Budget;

        protected override bool isLineExists => false;

        protected override bool ignoreBudgetError => true;

        protected override string variationCode => VariationCode; 
        #endregion
    }

    public class ForecastDateSnapshot : IForecastDateCostViewModel
    {
        public readonly DateTime MonthStartDate;
        public readonly DateTime MonthEndDate;
        private readonly DateTime firstViewDate;
        private readonly DateTime firstForecastDate;
        readonly List<FORECAST_JOB_HOUR_SNAPSHOT> byDateForecastJobHourSnapshots;
        public ForecastDateSnapshot(IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> byDataDateForecastJobHourSnapshots, DateTime firstViewDate, DateTime date, DateTime dataDate)
        {
            QueryDate = date; 
            
            this.firstViewDate = firstViewDate;
            this.firstForecastDate = dataDate;

            firstForecastDate = new DateTime(dataDate.Date.Year, dataDate.Date.Month, 1).AddMonths(2).AddDays(-1);
            MonthStartDate = new DateTime(date.Date.Year, date.Date.Month, 1);
            MonthEndDate = MonthStartDate.AddMonths(1).AddDays(-1);

            this.byDateForecastJobHourSnapshots = byDataDateForecastJobHourSnapshots.Where(x => x.FORECAST_DATE != null &&  ((DateTime)x.FORECAST_DATE).Month == QueryDate.Month && ((DateTime)x.FORECAST_DATE).Year == QueryDate.Year).ToList();
        }


        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> POForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.ForecastPO);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> IndirectForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.ForecastIndirect);

        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> ActualForecastSnapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual);
        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> P6Snapshots => byDateForecastJobHourSnapshots.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.P6Remaining);

        public DateTime QueryDate { get; set; }
        public decimal POOutstandingCosts => POForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal IndirectForecastCosts => IndirectForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal ActualCosts => ActualForecastSnapshots.Sum(x => x.FORECAST_COST);
        public decimal P6Hours
        {
            get
            {
                if (P6Snapshots.Count() == 0)
                    return 0;

                return P6Snapshots.Sum(x => x.FORECAST_QTY);
            }

        }

        public decimal P6Costs
        {
            get
            {
                if (P6Snapshots.Count() == 0)
                    return 0;

                return P6Snapshots.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal P6Quantities => P6Snapshots.Sum(x => x.FORECAST_QTY);

        public decimal TotalCosts
        {
            get
            {
                return ActualCosts + POOutstandingCosts + IndirectForecastCosts + P6Costs;
            }
        }

        //p6 costs needs to be categorised as uncommitted
        public decimal CommittedCosts => ActualCosts + POOutstandingCosts;
    }

}
