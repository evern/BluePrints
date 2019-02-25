using BluePrints.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class ProjectSummary
    {
        public StaticSummaryRowTypes RowType { get; set; }
        public decimal? Design_Budget { get; set; }
        public decimal? Construction_Budget { get; set; }
        public decimal? Total_Budget { get; set; }
        public decimal? Design_Remaining { get; set; }
        public decimal? Construction_Remaining { get; set; }
        public decimal? Total_Remaining { get; set; }
        public decimal? Total_Actuals { get; set; }
        public decimal? EAC { get; set; }
        public decimal? Design_Earned { get; set; }
        public decimal? Construction_Earned { get; set; }
        public decimal? Total_Earned { get; set; }
        public decimal? Design_Period_Planned { get; set; }
        public decimal? Construction_Period_Planned { get; set; }
        public decimal? Total_Period_Planned { get; set; }
        public decimal? SPI { get; set; }
        public decimal? CPI { get; set; }
        public decimal? Original_Contract_Value { get; set; }
        public decimal? Approved_Variation { get; set; }
        public decimal? Current_Contract_Value { get; set; }
        public decimal? GPM { get; set; }
        public decimal? Unapproved_Variation { get; set; }
        public decimal? Unapproved_EOT { get; set; }
        public DateTime? Contract_Completion_Date { get; set; }
        public DateTime? Forecast_Completion_Date { get; set; }
        public string Mask { get; set; }
        public bool Construction_TotalBudget_ReadOnly { get; set; }
        public bool Total_Remaining_ReadOnly { get; set; }
        public bool Construction_Budget_ReadOnly { get; set; }
        public bool Construction_Planned_ReadOnly { get; set; }
        public bool Construction_Earned_ReadOnly { get; set; }
        public bool Construction_Remaining_ReadOnly { get; set; }
        public List<Tuple<string, string>> Lookup { get; set; }
        public bool ReadOnly { get; set; }

        public string Display_RowType => EnumHelper<StaticSummaryRowTypes>.GetDisplayValue(RowType);
        public string Display_Total_Budget => String.Format("{0:" + Mask + "}", Total_Budget);
        public string Display_Total_Remaining => String.Format("{0:" + Mask + "}", Total_Remaining);
        public string Display_Total_Actuals => String.Format("{0:" + Mask + "}", Total_Actuals);
        public string Display_EAC => String.Format("{0:" + Mask + "}", EAC);
        public string Display_Total_Earned => String.Format("{0:" + Mask + "}", Total_Earned);
        public string Display_Total_Period_Planned => String.Format("{0:" + Mask + "}", Total_Period_Planned);
        public string Display_Original_Contract_Value => String.Format("{0:" + Mask + "}", Original_Contract_Value);
        public string Display_Approved_Variation => String.Format("{0:" + Mask + "}", Approved_Variation);
        public string Display_Current_Contract_Value => String.Format("{0:" + Mask + "}", Current_Contract_Value);
        public string Display_Unapproved_Variation => String.Format("{0:" + Mask + "}", Unapproved_Variation);
    }
}
