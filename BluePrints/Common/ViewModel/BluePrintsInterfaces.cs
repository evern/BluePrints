using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.Common.ViewModel
{
    public interface IHaveSummary : IHaveStats
    {
        void BuildStats(bool showLoadingScreen = true, bool isCosts = false);
        void RecalculateStats(bool isCosts);
    }

    public interface IHaveStats
    {
        ProgressStats Stats { get; set; }
    }
}
