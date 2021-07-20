using BaseModel.Data.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using static BluePrints.Data.BluePrintsEntities;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class FullStatsBuilder : PartialStatsBuilder
    {
        protected IEnumerable<SUBJOB> projectSUBJOBS { get; set; }
        public TimeSpan ReportingInterval { get; private set; }
        public DateTime FirstAlignedDataDate { get; private set; }
        readonly DateTime CurrentDataDate;
        readonly string ProjectNumber;
        public readonly IPrimeroEntitiesUnitOfWork PrimeroUOW;

        public FullStatsBuilder(string project_number, decimal currency_conversion, TimeSpan reporting_interval, DateTime first_aligned_data_date, IEnumerable<SUBJOB> SUBJOBS, DateTime current_date_date, IPrimeroEntitiesUnitOfWork primeroUOW)
            : base(currency_conversion)
        {
            ProjectNumber = project_number;
            PrimeroUOW = primeroUOW;
            this.ReportingInterval = reporting_interval;
            this.FirstAlignedDataDate = first_aligned_data_date;
            this.CurrentDataDate = current_date_date;
            this.projectSUBJOBS = SUBJOBS;
        }

        public void BuildExoDataPoints(IPrimeroEntitiesUnitOfWork primeroUOW, ProjectSummaryStats summaryObject, bool forceRetrieveAllJobs = false, bool forceRetrieveAllUnits = false, bool forceRetrieveAllPOs = false, bool showLoadingScreen = false, bool timeOnly = false)
        {
            try
            {
                ProjectSummaryStats projectSummaryStats = summaryObject as ProjectSummaryStats;
                if (projectSummaryStats == null)
                    return;

                List<ExoDataPoint> burnedDataPoints;
                List<ExoDataPoint> materialDataPoints = new List<ExoDataPoint>();
                List<ExoDataPoint> poDataPoints = new List<ExoDataPoint>();
                List<ExoDataPoint> previousPODataPoints = new List<ExoDataPoint>();
                DateTime loopDate = FirstAlignedDataDate;

                IEnumerable<SUBJOB> subjobs = projectSUBJOBS;
                string projectNumber = ProjectNumber;

                IEnumerable<string> qualifiedSubjobs = null;
                if (subjobs != null && !forceRetrieveAllJobs)
                    qualifiedSubjobs = subjobs.Select(x => x.INTERNAL_NAME1);

                DateTime actualsDataDate = DateTime.Now;
                DateTime poDataDate = DateTime.Now;
                if (!forceRetrieveAllUnits)
                    actualsDataDate = CurrentDataDate;

                if (!forceRetrieveAllPOs)
                    poDataDate = CurrentDataDate;

                List<DateTime> alignedDataDates = ChronologicalHelpers.GenerateAlignedDatesCollection(FirstAlignedDataDate, DateTime.Now.AddYears(1), ReportingInterval);
                List<SUBJOB> missingSUBJOBS = new List<SUBJOB>();

                burnedDataPoints = BluePrintsDataUtils.GetBurned(primeroUOW, projectNumber, actualsDataDate, qualifiedSubjobs, missingSUBJOBS, CurrencyConversion, showLoadingScreen);
                DateTime previousPODataDate = new DateTime(poDataDate.Year, poDataDate.Month, 1);
                previousPODataDate = previousPODataDate.AddDays(-1);
                if(!timeOnly)
                {
                    materialDataPoints = BluePrintsDataUtils.GetMaterials(primeroUOW, projectNumber, actualsDataDate, null, CurrencyConversion, showLoadingScreen);
                    poDataPoints = BluePrintsDataUtils.GetEXOPO(primeroUOW, projectNumber, poDataDate, null, showLoadingScreen);
                    previousPODataPoints = BluePrintsDataUtils.GetEXOPO(primeroUOW, projectNumber, previousPODataDate, null, showLoadingScreen);
                }
                else
                {
                    List<ExoDataPoint> burnedDataPointsWithNarrative = burnedDataPoints.Where(x => x.Narrative != null).ToList();
                    foreach(IReportable reportable in summaryObject.Reportables)
                    {
                        List<ExoDataPoint> reportableBurnedData = burnedDataPointsWithNarrative.Where(x => x.Narrative.ToUpper() == reportable.Deliverable_Name).ToList();
                        reportable.Stats.Burned.SetData(reportableBurnedData);
                    }
                }

                foreach (SUBJOB missingSUBJOB in missingSUBJOBS)
                {
                    projectSummaryStats.AddMissingExoSubjob(missingSUBJOB);
                }

                projectSummaryStats.Burned = new Stats(summaryObject);
                projectSummaryStats.Actual = new Stats(summaryObject);
                projectSummaryStats.Material = new Stats(summaryObject);
                projectSummaryStats.PO = new Stats(summaryObject);
                projectSummaryStats.PreviousPO = new Stats(summaryObject);
                projectSummaryStats.RemainingActual = new Stats(summaryObject, true);

                projectSummaryStats.Burned.SetData(burnedDataPoints);
                projectSummaryStats.Actual.SetData(burnedDataPoints);
                projectSummaryStats.Material.SetData(materialDataPoints);
                projectSummaryStats.PO.SetData(poDataPoints);
                projectSummaryStats.PreviousPO.SetData(previousPODataPoints);

                projectSummaryStats.RemainingActual.SetRemainingActualData(projectSummaryStats.Reportables, projectSummaryStats.Burned.GetData());

                if(showLoadingScreen)
                    LoadingScreenManager.CloseLoadingScreen();
            }
            catch (Exception e)
            {
                string s = e.ToString();
            }
        }

    }

    public class PartialStatsBuilder
    {
        protected decimal CurrencyConversion { get; private set; }
        public PartialStatsBuilder(decimal currencyConversion)
        {
            CurrencyConversion = currencyConversion;
        }

        public void BuildEarnedDataPoints(IReportable reportable, decimal qtyPerUnit)
        {
            List<DataPoint> progressItemEarnedDataPoints;
            if(reportable.Stats.AllowPercentageOnZeroTotalUnits)
            {
                progressItemEarnedDataPoints = reportable.PROGRESS_ITEM_UpToCurrentDataDate.Select(x => new DataPoint()
                {
                    DeliverableGuid = reportable.OriginalEntityKey,
                    TotalUnits = reportable.Stats.TotalUnits == 0 ? BluePrintsConstants.DurationBasedTotalUnits : reportable.Stats.TotalUnits,
                    TotalCosts = reportable.Stats.TotalCosts * CurrencyConversion,
                    BudgetedUnits = reportable.Stats.BudgetedUnits,
                    BudgetedCosts = reportable.Stats.BudgetedCosts * CurrencyConversion,
                    Units = x.EARNED_UNITS,
                    Quantity = x.EARNED_UNITS * qtyPerUnit,
                    Costs = x.EARNED_UNITS * reportable.Budget_ItemRate * CurrencyConversion,
                    ProgressDate = x.EARNED_DATE,
                }).ToList();
            }
            else
            {
                progressItemEarnedDataPoints = reportable.PROGRESS_ITEM_UpToCurrentDataDate.Select(x => new DataPoint()
                {
                    DeliverableGuid = reportable.OriginalEntityKey,
                    TotalUnits = reportable.Stats.TotalUnits,
                    TotalCosts = reportable.Stats.TotalCosts * CurrencyConversion,
                    BudgetedUnits = reportable.Stats.BudgetedUnits,
                    BudgetedCosts = reportable.Stats.BudgetedCosts * CurrencyConversion,
                    Units = x.ReportingEarnedUnits,
                    Quantity = x.ReportingEarnedUnits * qtyPerUnit,
                    Costs = x.ReportingEarnedUnits * reportable.Budget_ItemRate * CurrencyConversion,
                    ProgressDate = x.EARNED_DATE,
                }).ToList();
            }


            //adjust set earned data should only be performed at this level (lowest level), summary dashboard entity will just use set data
            reportable.Stats.Earned.SetData(progressItemEarnedDataPoints);
            reportable.Stats.TenderEarned.SetData(progressItemEarnedDataPoints);
        }

        public void BuildPlannedDataPointsFromQuery(IReportable reportable, decimal weightingPortion = 1, bool isForecast = false)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<Data.DataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPoints(reportable.GUID, isForecast);
                Double weightingPortionDbl = Convert.ToDouble(weightingPortion);
                foreach (Data.DataPoint plannedDataPoint in plannedDataPoints)
                {
                    plannedDataPoint.PeriodUnits *= weightingPortionDbl;
                    plannedDataPoint.PeriodPrice *= weightingPortionDbl;
                }

                reportable.Stats.Budgeted.SetPlannedData(plannedDataPoints);
                reportable.Stats.Current.SetPlannedData(plannedDataPoints);
            }
        }

        public void BuildRemainingDataPointsFromQuery(IReportable reportable, decimal weightingPortion = 1, bool isForecast = false)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<Data.DataPoint> RemainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPoints(reportable.GUID, isForecast);
                Double weightingPortionDbl = Convert.ToDouble(weightingPortion);
                foreach (Data.DataPoint remainingDataPoint in RemainingDataPoints)
                {
                    remainingDataPoint.PeriodUnits *= weightingPortionDbl;
                    remainingDataPoint.PeriodPrice *= weightingPortionDbl;
                }

                reportable.Stats.Remaining.SetRemainingData(RemainingDataPoints, reportable.Stats.Earned.GetData());
            }
        }
    }
}
