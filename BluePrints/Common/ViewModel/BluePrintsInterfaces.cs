using BluePrints.Common.ViewModel.Reporting;
using System.Collections.Generic;

namespace BluePrints.Common.ViewModel
{
    public interface IHaveSummary : IHaveStats
    {
        void BuildStats(DashboardEXOQueryType dashboardEXOQueryType = DashboardEXOQueryType.TimeAndMaterial, bool showLoadingScreen = true, decimal weightingPortion = 1, bool forceRetrieveAllJobs = false, bool forceRetrieveAllUnits = false, bool forceRetrieveAllPOs = false, List<StatsCalculationType> calcTypes = null, bool useProductivityFactorOnRemaining = false, bool IsVariationSeparated = false, bool IsByWeek = false);
        void RecalculateStats(bool isCosts, bool showLoadingScreen);
    }

    public interface IHaveStats
    {
        ProgressStats Stats { get; set; }
    }
}
