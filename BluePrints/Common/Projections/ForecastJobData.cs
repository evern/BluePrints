using BluePrints.Common.ViewModel.Reporting;
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
    public class ForecastJobData
    {
        public ForecastJobData()
        {
            DateCosts = new List<ForecastDateCost>();
        }

        public override string ToString()
        {
            if (Projection == null)
                return string.Empty;

            if (Projection.SubJob != null && Projection.Discipline != null && Projection.Commodity != null)
                return Projection.SubJob.Code + "-" + Projection.Discipline.Code + "-" + Projection.Commodity.Code;
            else if (Projection.SubJob != null && Projection.Discipline != null)
                return Projection.SubJob.Code + "-" + Projection.Discipline.Code;
            else if (Projection.SubJob != null)
                return Projection.SubJob.Code;
            else
                return string.Empty;
        }

        //used for compare data table to show type of cost that adds up to total forecast cost
        public string DropDownPhase { get; set; }
        public string CompareMask { get; set; }

        public ExoSubJobProjection Projection { get; set; }

        public List<ForecastDateCost> DateCosts { get; set; }

        public List<ForecastJobData> CommodityJobs { get; set; }

        public void SetBudgetCost(decimal budgetCost)
        {
            if(Projection == null)
                throw new NotImplementedException();

            Budget = budgetCost;
            Projection.ExoBudgetCosts = budgetCost;
        }

        public void SetForecastRate(decimal forecastRate)
        {
            if (Projection == null)
                throw new NotImplementedException();

            Rate = forecastRate;
            Projection.ExoForecastRate = forecastRate;
        }

        public decimal Budget { get; set; }
        public decimal Rate { get; set; }
        public decimal Revenue { get; set; }
        public decimal CurrentBudget => Budget + Variation;
        public decimal Variation { get; set; }
        public decimal ActualCosts { get; set; }
        public decimal ActualUnits { get; set; }
        public decimal Invoiced { get; set; }
        public decimal Outstanding { get; set; }
        public decimal P6BudgetedUnits { get; set; }
        public decimal P6RemainingUnits { get; set; }
        public decimal? EarnedUnits { get; set; }

        public decimal? P6RemainingUnitsOverride { get; set; }

        public bool IsProductivityFloating { get; set; }

        public decimal Productivity { get; set; }

        public decimal CurrentProductivity { get; set; }

        public decimal P6RemainingCosts { get; set; }
        public decimal PORemainingCosts { get; set; }
        public decimal Commitments => PORemainingCosts + P6RemainingCosts;
        public decimal OriginalUncommitted { get; set; }
        public decimal Uncommitted { get; set; }
        public decimal CurrentUncommitted { get; set; }
        public decimal PreviousEAC { get; set; }
        public decimal EstimateToComplete => Outstanding + Uncommitted;
        public decimal OriginalEstimateAtCompletion => ActualCosts + Outstanding + OriginalUncommitted;
        public decimal EstimateAtCompletion => ActualCosts + Outstanding + Uncommitted;
        public decimal CurrentEstimateAtCompletion => ActualCosts + Outstanding + CurrentUncommitted;
        public decimal PeriodMovement => EstimateAtCompletion - PreviousEAC;
        public bool IsProcurement
        {
            get
            {
                if (Projection == null || Projection.SubJob == null || Projection.SubJob.Code == string.Empty)
                    return false;

                return Projection.SubJob.Code.ToUpper().Contains("P");
            }
        }

        public decimal PctCompleteCosts => EstimateAtCompletion == 0 ? 1 : ActualCosts / EstimateAtCompletion;
        public decimal PctCompleteUnits => P6BudgetedUnits == 0 ? 1 : (P6BudgetedUnits - P6RemainingUnits) / P6BudgetedUnits;
        public decimal PctComplete => IsProcurement ? PctCompleteCosts : PctCompleteUnits;
        public decimal Variance => Budget - EstimateAtCompletion;
        public bool IsBudgetReadOnly { get; set; }
        public bool IsPOError { get; set; }
        public decimal IsPOErrorImageWidth => IsPOError ? 15 : 0;

        public RATE FallBackRate { get; set; }

        public decimal P6NominalRate => P6RemainingUnits == 0 ? FallBackRate.RATE1 == null ? 0 : (decimal)FallBackRate.RATE1 : P6RemainingCosts / P6RemainingUnits;

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
    }

    public class ForecastDateCost
    {
        public ForecastDateCost(DateTime date, bool isWeeks)
        {
            Date = date;
            if (isWeeks)
            {
                FloorDate = date.Date.AddDays(-6);
                CeilingDate = date.Date;
            }
            else
            {
                FloorDate = new DateTime(date.Date.Year, date.Date.Month, 1);
                CeilingDate = FloorDate.AddMonths(1).AddDays(-1);
            }
        }

        public DateTime ActualFloorDate { get; set; }
        public DateTime RemainingFloorDate { get; set; }
        public DateTime FloorDate { get; set; }
        public DateTime CeilingDate { get; set; }
        public DateTime Date { get; set; }
        //not using this as a measure because user can override it
        public decimal TotalCosts { get; set; }
        public decimal ActualCosts { get; set; }
        public decimal MaterialCosts { get; set; }
        public decimal P6Hours { get; set; }
        public decimal P6Costs { get; set; }
        public decimal POForecastCosts { get; set; }
        public decimal WeeklyForecastCosts { get; set; }

        //weekly forecast costs is uncommitted costs
        //public decimal CommittedCosts => ActualCosts + MaterialCosts + P6Costs + POForecastCosts + WeeklyForecastCosts;
        //public decimal CommittedCosts => ActualCosts + MaterialCosts + P6Costs + POForecastCosts;

        //p6 costs needs to be categorised as uncommitted
        public decimal CommittedCosts => ActualCosts + MaterialCosts + POForecastCosts;
    }
}
