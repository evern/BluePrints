using BluePrints.Common.ViewModel.Reporting;
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

        public decimal? P6RemainingUnitsOverride { get; set; }

        public decimal Productivity { get; set; }

        public decimal P6RemainingCosts { get; set; }
        public decimal Uncommitted { get; set; }
        public decimal PreviousEAC { get; set; }
        public decimal EstimateToComplete => Outstanding + Uncommitted;
        public decimal EstimateAtCompletion => ActualCosts + Outstanding + Uncommitted;
        public decimal PeriodMovement => EstimateAtCompletion - PreviousEAC;
        //public decimal PctComplete => EstimateAtCompletion == 0 ? 1 : Actuals / EstimateAtCompletion;
        public decimal PctComplete => P6BudgetedUnits == 0 ? 1 : (P6BudgetedUnits - P6RemainingUnits) / P6BudgetedUnits;
        public decimal Variance => Budget - EstimateAtCompletion;
        public bool IsBudgetReadOnly { get; set; }
        public bool IsPOError { get; set; }
        public decimal IsPOErrorImageWidth => IsPOError ? 15 : 0;

        public decimal fallBackRate { get; set; }

        public decimal P6NominalRate => P6RemainingUnits == 0 ? fallBackRate == 0 ? 0 : fallBackRate : P6RemainingCosts / P6RemainingUnits;

        //used by detailed rows so that only P6 hour row can be edited
        public bool IsP6HoursRow { get; set; }
    }

    public class ForecastDateCost
    {
        public ForecastDateCost(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; set; }

        //not using this as a measure because user can override it
        public decimal TotalCosts { get; set; }

        public decimal ActualCosts { get; set; }
        public decimal MaterialCosts { get; set; }
        public decimal P6Hours { get; set; }
        public decimal P6Costs { get; set; }
        public decimal POForecastCosts { get; set; }

        public decimal PreloadedCosts => ActualCosts + MaterialCosts + P6Costs + POForecastCosts;
    }
}
