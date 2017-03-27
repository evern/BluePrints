using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public abstract class SummaryBuilder
    {
        SummarizableObject summaryObject;
        public SummarizableObject SummaryObject
        {
            get { return summaryObject; }
            set { summaryObject = value; }
        }

        public abstract void SummarizeVariationDataPoints();
        public abstract void SetReportablesP6StartUnits();
        public abstract void SummarizePlannedDataPoints();
        public abstract void SummarizeModifiedPlannedDataPoints();
        public abstract void SummarizeEarnedDataPoints();
        public abstract void SummarizeBurnedDataPoints();
        public abstract void SummarizeRemainingDataPoints();
        public abstract void SummarizeActualDataPoints();
        public abstract void GroupAndAccumulateDataPointsByPeriod();
        public abstract void GroupAndAccumulateReportableDataPointsByPeriod();


        public void RecalculateStats(bool isCosts = false)
        {
            SummaryObject.RecalculateStats(isCosts);
        }
    }

    public class GroupPROJECTReportablesByWorkpackBuilder : SummaryBuilder
    {
        public GroupPROJECTReportablesByWorkpackBuilder(WORKPACK_Dashboard WORKPACKDashboard, PROJECT_Dashboard PROJECTDashboard)
        {
            WORKPACKDashboard.ReportableObjects = PROJECTDashboard.ReportableObjects.Where(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK == WORKPACKDashboard.GUID);
            string activeWORKPACKName;
            if (PROJECTDashboard.PROJECT.USELEGACYWORKPACK)
                activeWORKPACKName = WORKPACKDashboard.WORKPACK.INTERNAL_NAME1;
            else
                activeWORKPACKName = WORKPACKDashboard.WORKPACK.INTERNAL_NAME2;

            IEnumerable<ProgressInfo> workpackBurnedDataPoints = PROJECTDashboard.NonCumulative_BurnedDataPoints.Where(x => x.WorkpackName == activeWORKPACKName).OrderByDescending(x => x.ProgressDate);
            IEnumerable<ProgressInfo> workpackActualDataPoints = PROJECTDashboard.NonCumulative_ActualDataPoints.Where(x => x.WorkpackName == activeWORKPACKName).OrderByDescending(x => x.ProgressDate);
            WORKPACKDashboard.NonCumulative_BurnedDataPoints = new ObservableCollection<ProgressInfo>(workpackBurnedDataPoints);
            WORKPACKDashboard.NonCumulative_ActualDataPoints = new ObservableCollection<ProgressInfo>(workpackActualDataPoints);
            WORKPACKDashboard.FirstAlignedDataDate = PROJECTDashboard.FirstAlignedDataDate;
            WORKPACKDashboard.LiveBASELINE = PROJECTDashboard.LiveBASELINE;
            WORKPACKDashboard.LivePROGRESS = PROJECTDashboard.LivePROGRESS;
            WORKPACKDashboard.ReportingDataDate = PROJECTDashboard.ReportingDataDate;
            WORKPACKDashboard.IntervalPeriod = PROJECTDashboard.IntervalPeriod;
            this.SummaryObject = WORKPACKDashboard;
        }

        public override void SummarizeVariationDataPoints()
        {
            SummaryObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_VariationAdjustments));
        }

        public override void SummarizePlannedDataPoints()
        {
            SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
        }

        public override void SummarizeModifiedPlannedDataPoints()
        {
            SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        public override void SummarizeEarnedDataPoints()
        {
            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_EarnedDataPoints));
        }

        public override void SetReportablesP6StartUnits()
        {
            throw new InvalidOperationException("there is no need to set reportables p6 start units from ReportableObjects.");
        }

        public override void SummarizeActualDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative actual data points from ReportableObjects.");
        }

        public override void SummarizeBurnedDataPoints()
        {
            throw new InvalidOperationException("there is no need to roll up non cumulative burned data points from ReportableObjects.");
        }

        public override void SummarizeRemainingDataPoints()
        {
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingCurrentDataPoints));
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
        }

        public override void GroupAndAccumulateDataPointsByPeriod()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(SummaryObject);
        }

        public override void GroupAndAccumulateReportableDataPointsByPeriod()
        {
            throw new InvalidOperationException("there is no need to summarize progress data points.");
        }
    }

    public class PROJECTSummaryBuilder : SummaryBuilder
    {
        IBluePrintsEntitiesUnitOfWork BluePrintsUnitOfWork { get; set; }
        IP6EntitiesUnitOfWork P6UnitOfWork { get; set; }
        decimal CurrencyConversion { get; set; }

        ProjectReportableDataPointsBuilder dataPointsBuilder;
        public ProjectReportableDataPointsBuilder DataPointsBuilder
        {
            get { return dataPointsBuilder; }
            set { dataPointsBuilder = value; }
        }

        public PROJECTSummaryBuilder(SummarizableObject summaryObject, IEnumerable<WORKPACK> WORKPACKS, IBluePrintsEntitiesUnitOfWork BluePrintsUOW = null, IP6EntitiesUnitOfWork P6UOW = null)
        {
            if (summaryObject.LivePROGRESS == null || summaryObject.LiveBASELINE == null)
                return;
            
            this.CurrencyConversion = summaryObject.LiveBASELINE.PROJECT.CURRENCYCONVERSION;
            this.SummaryObject = summaryObject;
            this.SummaryObject.ReportingDataDate = this.SummaryObject.LivePROGRESS.DATA_DATE;

            if (BluePrintsUOW == null)
                BluePrintsUOW = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                this.BluePrintsUnitOfWork = BluePrintsUOW;

            if (P6UOW == null)
                this.P6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            else
                this.P6UnitOfWork = P6UOW;

            this.SummaryObject.IntervalPeriod = ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(SummaryObject.LivePROGRESS);
            this.SummaryObject.FirstAlignedDataDate = ISupportProgressReportingExtensions.GenerateFirstAlignedDataDate(SummaryObject.LivePROGRESS);
            IEnumerable<VARIATION_ITEMProjection> variation_itemProjection = GetVARIATION_ITEMProjection();
            IEnumerable<WORKPACK> currentWORKPACKS = WORKPACKS.ToList();

            DataPointsBuilder = new ProjectReportableDataPointsBuilder(SummaryObject.IntervalPeriod, SummaryObject.ReportingDataDate, SummaryObject.FirstAlignedDataDate, this.CurrencyConversion, variation_itemProjection, currentWORKPACKS, P6UnitOfWork, summaryObject.LiveBASELINE.P6BASELINE_NAME, summaryObject.LiveBASELINE.P6MODBASELINE_NAME, SummaryObject.LivePROGRESS.P6PROGRESS_NAME);
        }

        public override void GroupAndAccumulateDataPointsByPeriod()
        {
            ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(this.SummaryObject);
        }

        public override void GroupAndAccumulateReportableDataPointsByPeriod()
        {
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(reportableObject, this.SummaryObject.FirstAlignedDataDate, this.SummaryObject.IntervalPeriod);
            }
        }

        public override void SummarizePlannedDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, false);
            SummarizePlannedDataPointsByType(true);
        }

        public override void SummarizeModifiedPlannedDataPoints()
        {
            //PlannedDataPointsBuilderFromDatabase(CURRENTPROJECT.NUMBER, true);
            SummarizePlannedDataPointsByType(false);
        }

        private void SummarizePlannedDataPointsFromDatabase(string ProjectNumber, bool isOriginal)
        {
            BluePrintsEntities bluePrintDataContext = new BluePrintsEntities();
            ObjectResult<StoredProcedure_DeliverablesDataPoints> deliverablesDataPoints = bluePrintDataContext.GetDataPointsByProject(ProjectNumber, isOriginal);
            SummarizePlannedDataPointsByType(isOriginal, deliverablesDataPoints.ToList());
        }

        private void SummarizePlannedDataPointsByType(bool isOriginal, IEnumerable<StoredProcedure_DeliverablesDataPoints> DataPointsCollection = null)
        {
            AssignmentLoadType assignmentLoadType = isOriginal == true ? AssignmentLoadType.Original : assignmentLoadType = AssignmentLoadType.Modified;

            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                DataPointsBuilder.BuildPlannedDataPoints(reportableObject, assignmentLoadType, DataPointsCollection);
            }

            if (isOriginal)
                SummaryObject.NonCumulative_OriginalDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_OriginalDataPoints));
            else
                SummaryObject.NonCumulative_PlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_PlannedDataPoints));
        }

        /// <summary>
        /// Calculates each baselineItem earned data point while populating aggregate non cumulative earned data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void SummarizeEarnedDataPoints()
        {
            Dictionary<Guid, decimal> workpackAssignedUnits = new Dictionary<Guid, decimal>();
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                DataPointsBuilder.BuildEarnedDataPoints(reportableObject);
            }

            SummaryObject.NonCumulative_EarnedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_EarnedDataPoints));
        }

        public override void SummarizeRemainingDataPoints()
        {
            //BuildProductivity();
            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                DataPointsBuilder.BuildRemainingDataPoints(reportableObject);
            }

            //extract all data points out to be used as an overall summary
            SummaryObject.NonCumulative_RemainingPlannedDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_RemainingPlannedDataPoints));
            SummaryObject.NonCumulative_RemainingCurrentDataPoints = new ObservableCollection<ProgressInfo>(SummaryObject.ReportableObjects.SelectMany(progressItem => progressItem.NonCumulative_RemainingCurrentDataPoints));
        }

        public override void SetReportablesP6StartUnits()
        {
            ISupportProgressReportingExtensions.SetWorkpackAssignmentStartUnit(this.SummaryObject.ReportableObjects);
        }

        private IEnumerable<VARIATION_ITEMProjection> GetVARIATION_ITEMProjection()
        {
            IQueryable<VARIATION> approvedVARIATION = this.SummaryObject.VARIATIONS.Where(x => x.APPROVED != null).AsQueryable();
            List<VARIATION_ITEMProjection> variation_itemProjection = new List<VARIATION_ITEMProjection>();
            foreach(VARIATION variation in approvedVARIATION)
            {
                foreach(VARIATION_ITEM variation_item in variation.VARIATION_ITEM)
                {
                    if (variation_item.ACTION != VariationAction.Add && variation_item.ACTION != VariationAction.Append)
                        continue;

                    variation_itemProjection.Add(new VARIATION_ITEMProjection() { VARIATION_ITEM = variation_item, APPROVED = variation.APPROVED });
                }
            }

            return variation_itemProjection;
        }

        public override void SummarizeVariationDataPoints()
        {
            if (this.SummaryObject.VARIATIONS == null)
                return;

            foreach (ReportableObject reportableObject in SummaryObject.ReportableObjects)
            {
                DataPointsBuilder.BuildVariationAdjustments(reportableObject);
            }
            
            SummaryObject.NonCumulative_VariationAdjustments = new ObservableCollection<VariationAdjustment>(SummaryObject.ReportableObjects.SelectMany(x => x.NonCumulative_VariationAdjustments));
        }

        /// <summary>
        /// Calculates each baselineItem burned/actual data point while populating aggregate non cumulative burned/actual data points
        /// </summary>
        /// <returns>Non cumulative earned progress data points</returns>
        public override void SummarizeBurnedDataPoints()
        {
            ObservableCollection<ProgressInfo> nonCumulative_BurnedDataPoints = new ObservableCollection<ProgressInfo>();
            DateTime firstAlignedDataDate = SummaryObject.FirstAlignedDataDate;
            TimeSpan progressInterval = SummaryObject.IntervalPeriod;
            DateTime loopDate = firstAlignedDataDate;

            IEnumerable<WORKPACK> workpacks = SummaryObject.LiveBASELINE.PROJECT.WORKPACK.ToList();
            string projectNumber = SummaryObject.LiveBASELINE.PROJECT.NUMBER;

            IEnumerable<string> qualifiedWorkpacks = workpacks == null ? new List<string>() : workpacks.Select(x => x.INTERNAL_NAME1);
            var PrimeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var jobTransactions = from JOBTRANS in PrimeroUnitOfWork.JOB_TRANSACTIONS
                                  join JOBCOST_HDR2 in PrimeroUnitOfWork.JOBCOST_HDR
                                  on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                  join JOBCOST_HDR1 in PrimeroUnitOfWork.JOBCOST_HDR
                                  on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                  join JOBCOST_RESOURCE in PrimeroUnitOfWork.JOBCOST_RESOURCE
                                  on JOBTRANS.STAFFNO equals JOBCOST_RESOURCE.SEQNO
                                  where JOBCOST_HDR2.JOBCODE == projectNumber && JOBTRANS.TRANSTYPE == "T" && JOBTRANS.LINE_STATUS != "X"
                                  select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, JOBCOST_RESOURCE.RESOURCENAME };

            var exoWorkpacks = from JOBCOST_HDR in PrimeroUnitOfWork.JOBCOST_HDR
                               where JOBCOST_HDR.JOBCODE.Contains(projectNumber)
                               select new { JOBCOST_HDR.TITLE, JOBCOST_HDR.JOBCODE };

            var exoWorkpacksList = exoWorkpacks.ToList();
            SummaryObject.missingExoWorkpacks = new List<WORKPACK>();
            foreach (WORKPACK workpack in workpacks)
            {
                var exoWorkpack = exoWorkpacksList.FirstOrDefault(x => x.JOBCODE == workpack.INTERNAL_NAME1 || x.JOBCODE == workpack.INTERNAL_NAME2);
                if (exoWorkpack == null)
                {
                    SummaryObject.missingExoWorkpacks.Add(workpack);
                }
            }

            var jobTransactionsList = jobTransactions.ToList();
            if (jobTransactionsList.Count == 0)
                return;

            List<DateTime> alignedDataDates = ISupportProgressReportingExtensions.GenerateAlignedDatesCollection(firstAlignedDataDate, jobTransactionsList.Max(x => x.TRANSDATE).Value, progressInterval);
            foreach (var jobTransaction in jobTransactionsList)
            {
                if (qualifiedWorkpacks.Contains(jobTransaction.JOBCODE))
                {
                    nonCumulative_BurnedDataPoints.Add(new ProgressInfo()
                    {
                        BudgetedUnits = 0,
                        BudgetedCosts = 0,
                        Units = (decimal)jobTransaction.QUANTITY,
                        Costs = (decimal)jobTransaction.LINETOTAL * this.CurrencyConversion,
                        Actuals = jobTransaction.LINECOST == null ? 0 : (decimal)jobTransaction.LINECOST,
                        ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE),
                        BaselineItemGuid = Guid.Empty,
                        WorkpackName = jobTransaction.JOBCODE,
                        ResourceName = jobTransaction.RESOURCENAME,
                        Quantity = (decimal)jobTransaction.QUANTITY
                    });
                }
            }

            
            SummaryObject.NonCumulative_BurnedDataPoints = nonCumulative_BurnedDataPoints;
        }

        public override void SummarizeActualDataPoints()
        {
            List<ProgressInfo> convertBurnedToActualDataPoints = new List<ProgressInfo>();
            SummaryObject.NonCumulative_BurnedDataPoints.ToList().ForEach(dataPoint => convertBurnedToActualDataPoints.Add(new ProgressInfo()
            {
                BudgetedCosts = dataPoint.BudgetedCosts,
                BudgetedUnits = dataPoint.BudgetedUnits,
                Costs = dataPoint.Actuals,
                Actuals = dataPoint.Actuals,
                ProgressDate = dataPoint.ProgressDate,
                BaselineItemGuid = dataPoint.BaselineItemGuid,
                Units = dataPoint.Units,
                WorkpackGuid = dataPoint.WorkpackGuid,
                WorkpackName = dataPoint.WorkpackName,
                ResourceName = dataPoint.ResourceName,
                Quantity = dataPoint.Quantity
            }));

            SummaryObject.NonCumulative_ActualDataPoints = new ObservableCollection<ProgressInfo>(convertBurnedToActualDataPoints);
        }


        public enum DataPointsType
        {
            Planned = 0,
            Earned = 1,
            Remaining = 2
        }

        public enum AssignmentLoadType
        {
            Original,
            Modified,
            Both
        }
    }
}
