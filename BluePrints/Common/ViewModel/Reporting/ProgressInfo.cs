using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProgressInfo
    {
        public Guid BaselineItemGuid { get; set; }
        public DateTime ProgressDate { get; set; }
        public decimal Units { get; set; }
        public decimal Costs { get; set; }
        //Used to store actuals while storing burn
        public decimal Actuals { get; set; }
        public decimal BudgetedUnits { get; set; }
        public decimal BudgetedCosts { get; set; }
        public Guid WorkpackGuid { get; set; }
        public string WorkpackName { get; set; }
        public string ResourceName { get; set; }
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
