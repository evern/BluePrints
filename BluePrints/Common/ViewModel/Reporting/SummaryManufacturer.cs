using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class BuildProjectStats
    {
        public void Manufacture(SummaryBuilder summaryBuilder)
        {
            Build(summaryBuilder);
        }

        public void Build(SummaryBuilder summaryBuilder)
        {
            summaryBuilder.BuildVariationDataPoints();
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

    public class BuildMinimalStatsForPlannedOriginalPercentage
    {
        public void Manufacture(SummaryBuilder summaryBuilder)
        {
            Build(summaryBuilder);
        }

        public void Build(SummaryBuilder summaryBuilder)
        {
            summaryBuilder.SummarizePlannedDataPoints();
            summaryBuilder.GroupAndAccumulateReportableDataPointsByPeriod();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
        }
    }

    public class BuildFullStatsIncludingPROGRESS_ITEMSummary
    {
        private BuildProjectStats buildProjectStats = new BuildProjectStats();

        public void Manufacture(SummaryBuilder summaryBuilder)
        {
            buildProjectStats.Build(summaryBuilder);
            Build(summaryBuilder);
        }

        public void Build(SummaryBuilder summaryBuilder)
        {
            summaryBuilder.GroupAndAccumulateReportableDataPointsByPeriod();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
        }
    }

    public class SummaryRollUp
    {
        public void Manufacture(SummaryBuilder summaryBuilder)
        {
            Build(summaryBuilder);
        }

        public void Build(SummaryBuilder summaryBuilder)
        {
            summaryBuilder.BuildVariationDataPoints();
            summaryBuilder.SummarizePlannedDataPoints();
            summaryBuilder.SummarizeModifiedPlannedDataPoints();
            summaryBuilder.SummarizeEarnedDataPoints();
            summaryBuilder.SummarizeRemainingDataPoints();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
            summaryBuilder.RecalculateStats();
        }
    }

    public class PROGRESS_ITEMSummaryManufacturer
    {
        public void Manufacture(SummaryBuilder summaryBuilder)
        {
            Build(summaryBuilder);
        }

        public void Build(SummaryBuilder summaryBuilder)
        {
            summaryBuilder.SummarizePlannedDataPoints();
            summaryBuilder.SummarizeEarnedDataPoints();
            summaryBuilder.SummarizeRemainingDataPoints();
            summaryBuilder.GroupAndAccumulateDataPointsByPeriod();
            summaryBuilder.GroupAndAccumulateReportableDataPointsByPeriod();
        }
    }
}