using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProjectSummarizingFactory
    {
        public void Manufacture(PROJECTSummaryBuilder summaryBuilder)
        {
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
            summaryBuilder.SummarizeVariationDataPoints();
            summaryBuilder.SummarizePlannedDataPoints();
            summaryBuilder.SummarizeModifiedPlannedDataPoints();
            summaryBuilder.SummarizeEarnedDataPoints();
            summaryBuilder.SummarizeRemainingDataPoints();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
            summaryBuilder.RecalculateStats();
        }
    }
}