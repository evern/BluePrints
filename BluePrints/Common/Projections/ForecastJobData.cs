using BluePrints.ViewModels;
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
        public decimal Actuals { get; set; }
        public decimal Invoiced { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Uncommitted { get; set; }
        public decimal PreviousEAC { get; set; }
        public decimal EstimateToComplete => Outstanding + Uncommitted;
        public decimal EstimateAtCompletion => Actuals + Outstanding + Uncommitted;
        public decimal PeriodMovement => PreviousEAC - EstimateAtCompletion;
        public decimal PctComplete => EstimateAtCompletion == 0 ? 1 : Actuals / EstimateAtCompletion;
        public decimal Variance => Budget - EstimateAtCompletion;
        public bool IsBudgetReadOnly { get; set; }
    }

    public class ForecastDateCost
    {
        public ForecastDateCost(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
    }
}
