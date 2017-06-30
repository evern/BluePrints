using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public interface IReportable : IHaveStats
    {
        IDeliverable Deliverable { get; }
    }

    public interface IDeliverable
    {
        string ReportableItem_Name { get; }
        string Commodity_Code { get; }
        string Stock_Code { get; }
        Guid? Workpack_Guid { get; }

        decimal TotalHoursIncludeByDuration { get; }
        decimal EstimatedHours { get; }
        decimal TotalHours { get; }
        decimal EstimatedCosts { get; }
        decimal TotalCosts { get; }
        Guid Original_Guid { get; }
        decimal ItemRate { get; }
    }
}
