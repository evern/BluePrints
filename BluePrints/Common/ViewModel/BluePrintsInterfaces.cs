using BluePrints.Common.ViewModel.Reporting;
using System.Collections.Generic;

namespace BluePrints.Common.ViewModel
{
    public interface IHaveSummary : IHaveStats
    {
        void BuildStats(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1, bool forceRetrieveAllJobs = false, bool forceRetrieveAllUnits = false, List<StatsCalculationType> calcTypes = null, bool useProductivityFactorOnRemaining = false);
        void RecalculateStats(bool isCosts, bool showLoadingScreen);
    }

    public interface IHaveStats
    {
        ProgressStats Stats { get; set; }
    }
}
