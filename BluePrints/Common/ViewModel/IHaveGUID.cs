using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel
{
    public interface IHaveGUID
    {
        Guid GUID { get; set; }
    }

    public interface IHaveSummary : IHaveStats
    {
        void BuildStats();
        void RecalculateStats(bool isCosts);
    }

    public interface IHaveStats
    {
        ProgressStats Stats { get; set; }
    }
}
