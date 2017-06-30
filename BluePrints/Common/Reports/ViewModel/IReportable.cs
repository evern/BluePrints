using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IReportable : IHaveStats
    {
        string Commodity_Code { get; set; }
        string Stock_Code { get; set; }
        string Workpack_Guid { get; set; }
        string ReportableItem_Name { get; set; }
        bool IsPlannedFromP6 { get; set; }
        bool IsRemainingFromP6 { get; set; }
    }
}
