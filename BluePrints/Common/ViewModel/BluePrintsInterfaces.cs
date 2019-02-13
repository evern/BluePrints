using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.Common.ViewModel
{
    public interface IHaveSummary : IHaveStats
    {
        void BuildStats(bool showLoadingScreen = true, bool isCosts = false, decimal weightingPortion = 1, bool forceRetrieveAllBurned = false, bool earnOnly = false, bool useProductivityFactorOnRemaining = false, decimal maxProductivityFactorOnRemaining = 3);
        void RecalculateStats(bool isCosts);
    }

    public interface IHaveStats
    {
        ProgressStats Stats { get; set; }
    }
}
