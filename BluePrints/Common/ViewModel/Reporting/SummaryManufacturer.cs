using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProjectSummarizingFactory
    {
        public void Manufacture(PROJECTSummaryBuilder summaryBuilder, bool showProgress = true)
        {
            if(showProgress)
            {
                int maxProgress = 0;
                maxProgress += summaryBuilder.GetSummarizeVariationDataPointsMaxProgress();
                maxProgress += summaryBuilder.GetSetReportablesP6StartUnitsMaxProgress();
                maxProgress += summaryBuilder.GetSummarizePlannedDataPointsMaxProgress();
                maxProgress += summaryBuilder.GetSummarizeModifiedPlannedDataPointsMaxProgress();
                maxProgress += summaryBuilder.GetSummarizeEarnedDataPointsMaxProgress();
                maxProgress += summaryBuilder.GetSummarizeBurnedDataPointsMaxProgress();
                maxProgress += summaryBuilder.GetSummarizeRemainingDataPointsMaxProgress();
                maxProgress += summaryBuilder.GetSummarizeActualDataPointsMaxProgress();
                maxProgress += summaryBuilder.GetGroupAndAccumulateDataPointsByPeriodMaxProgress();
                maxProgress += summaryBuilder.GetGroupAndAccumulateReportableDataPointsByPeriodMaxProgress();
                LoadingScreenManager.ShowLoadingScreen(maxProgress);
            }

            summaryBuilder.SetReportablesP6StartUnits();
            summaryBuilder.SummarizeVariationDataPoints();
            summaryBuilder.SummarizePlannedDataPoints();
            summaryBuilder.SummarizeModifiedPlannedDataPoints();
            summaryBuilder.SummarizeEarnedDataPoints();
            summaryBuilder.SummarizeBurnedDataPoints();
            summaryBuilder.SummarizeActualDataPoints();
            summaryBuilder.SummarizeRemainingDataPoints();
            summaryBuilder.GroupAndAccumulateReportableDataPointsByPeriod();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
            summaryBuilder.RecalculateStats();
        }
    }

    public class Progress_ItemPlannedOnlySummarizingFactory
    {
        public void Manufacture(PROJECTSummaryBuilder summaryBuilder)
        {
            int maxProgress = 0;
            maxProgress += summaryBuilder.GetSetReportablesP6StartUnitsMaxProgress();
            maxProgress += summaryBuilder.GetSummarizePlannedDataPointsMaxProgress();
            maxProgress += summaryBuilder.GetGroupAndAccumulateReportableDataPointsByPeriodMaxProgress();
            maxProgress += summaryBuilder.GetGroupAndAccumulateDataPointsByPeriodMaxProgress();

            LoadingScreenManager.ShowLoadingScreen(maxProgress);
            summaryBuilder.SetReportablesP6StartUnits();
            summaryBuilder.SummarizePlannedDataPoints();
            summaryBuilder.GroupAndAccumulateReportableDataPointsByPeriod();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
        }
    }

    public class WorkpackSummarizingFactory
    {
        public void Manufacture(GroupPROJECTReportablesByWorkpackBuilder summaryBuilder)
        {
            //Not required, too fast
            //int maxProgress = 0;
            //maxProgress += summaryBuilder.GetSummarizeVariationDataPointsMaxProgress();
            //maxProgress += summaryBuilder.GetSummarizePlannedDataPointsMaxProgress();
            //maxProgress += summaryBuilder.GetSummarizeModifiedPlannedDataPointsMaxProgress();
            //maxProgress += summaryBuilder.GetSummarizeEarnedDataPointsMaxProgress();
            //maxProgress += summaryBuilder.GetSummarizeRemainingDataPointsMaxProgress();
            //maxProgress += summaryBuilder.GetGroupAndAccumulateDataPointsByPeriodMaxProgress();

            //LoadingScreenManager.ShowLoadingScreen(maxProgress);
            summaryBuilder.SummarizeVariationDataPoints();
            summaryBuilder.SummarizePlannedDataPoints();
            summaryBuilder.SummarizeModifiedPlannedDataPoints();
            summaryBuilder.SummarizeEarnedDataPoints();
            summaryBuilder.SummarizeRemainingDataPoints();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
            summaryBuilder.RecalculateStats();
        }
    }

    public class DataPointsBuildingFactory
    {
        public void Manufacture(IBuildDataPoints dataPointsBuilder, ReportableObject reportableObject)
        {
            dataPointsBuilder.BuildVariationAdjustments(reportableObject);
            dataPointsBuilder.BuildVariationAdjustments(reportableObject);
            dataPointsBuilder.BuildPlannedDataPoints(reportableObject, PROJECTSummaryBuilder.AssignmentLoadType.Original);
            dataPointsBuilder.BuildPlannedDataPoints(reportableObject, PROJECTSummaryBuilder.AssignmentLoadType.Modified);
            dataPointsBuilder.BuildEarnedDataPoints(reportableObject);
            dataPointsBuilder.BuildRemainingDataPoints(reportableObject);
            dataPointsBuilder.BuildCumulativeSummary(reportableObject);
        }
    }
}