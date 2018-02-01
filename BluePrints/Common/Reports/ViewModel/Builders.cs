using BaseModel.Data.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
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
        readonly IPrimeroEntitiesUnitOfWork PrimeroUOW;

        public FullStatsBuilder(string project_number, decimal currency_conversion, TimeSpan reporting_interval, DateTime first_aligned_data_date, IEnumerable<SUBJOB> SUBJOBS, DateTime current_date_date, IPrimeroEntitiesUnitOfWork primeroUOW = null)
            : base(currency_conversion)
        {
            ProjectNumber = project_number;
            PrimeroUOW = primeroUOW == null ? PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork() : primeroUOW;
            this.ReportingInterval = reporting_interval;
            this.FirstAlignedDataDate = first_aligned_data_date;
            this.CurrentDataDate = current_date_date;
            this.projectSUBJOBS = SUBJOBS;
        }

        public void BuildExoDataPoints(ProjectSummaryStats summaryObject, ExoBurnedFilterType filterType)
        {
            try
            {
                ProjectSummaryStats projectSummaryStats = summaryObject as ProjectSummaryStats;
                if (projectSummaryStats == null)
                    return;

                ObservableCollection<ExoDataPoint> burnedDataPoints = new ObservableCollection<ExoDataPoint>();
                ObservableCollection<ExoDataPoint> actualDataPoints = new ObservableCollection<ExoDataPoint>();

                DateTime loopDate = FirstAlignedDataDate;

                IEnumerable<SUBJOB> subjobs = projectSUBJOBS;
                string projectNumber = ProjectNumber;

                IEnumerable<string> qualifiedSubjobs;
                if (subjobs == null)
                    qualifiedSubjobs = new List<string>();
                else
                {
                    if (filterType == ExoBurnedFilterType.All)
                        qualifiedSubjobs = subjobs.Select(x => x.INTERNAL_NAME1);
                    else if (filterType == ExoBurnedFilterType.Design)
                        qualifiedSubjobs = subjobs.Select(x => x.INTERNAL_NAME1);
                    else
                        qualifiedSubjobs = subjobs.Select(x => x.INTERNAL_NAME1);

                    //Can't do this because legacy subjob exists
                    //else if (filterType == ExoBurnedFilterType.Design)
                    //    qualifiedSubjobs = subjobs.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design).Select(x => x.INTERNAL_NAME1);
                    //else
                    //    qualifiedSubjobs = subjobs.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Construct).Select(x => x.INTERNAL_NAME1);
                }

                var PrimeroUnitOfWork = PrimeroUOW;
                var jobTransactions = from JOBTRANS in PrimeroUnitOfWork.JOB_TRANSACTIONS
                                      join JOBCOST_HDR2 in PrimeroUnitOfWork.JOBCOST_HDR
                                      on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                      join JOBCOST_HDR1 in PrimeroUnitOfWork.JOBCOST_HDR
                                      on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                      join JOBCOST_RESOURCE in PrimeroUnitOfWork.JOBCOST_RESOURCE
                                      on JOBTRANS.STAFFNO equals JOBCOST_RESOURCE.SEQNO
                                      join JOB_COSTGROUPS in PrimeroUnitOfWork.JOB_COSTGROUPS
                                      on JOBTRANS.COST_GROUP equals JOB_COSTGROUPS.SEQNO
                                      join JOB_COSTTYPES in PrimeroUnitOfWork.JOB_COSTTYPES
                                      on JOBTRANS.COST_TYPE equals JOB_COSTTYPES.SEQNO
                                      where JOBCOST_HDR2.JOBCODE == projectNumber && JOBTRANS.TRANSTYPE == "T" && JOBTRANS.LINE_STATUS != "X" && JOBTRANS.TRANSDATE <= CurrentDataDate
                                      select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.TITLE, JOB_COSTGROUPS.COSTDESC, COSTDESC3 = JOB_COSTTYPES.COSTDESC };

                var exoSubjobs = from JOBCOST_HDR in PrimeroUnitOfWork.JOBCOST_HDR
                                   where JOBCOST_HDR.JOBCODE.Contains(projectNumber)
                                   select new { JOBCOST_HDR.TITLE, JOBCOST_HDR.JOBCODE };

                var exoSubjobsList = exoSubjobs.ToList();

                //double units = (double)jobTransactions.Where(x => x.QUANTITY != null).Sum(x => x.QUANTITY);
                //string s = units.ToString();
                //foreach (SUBJOB subjob in subjobs)
                //{
                //    var exoSubjob = exoSubjobsList.FirstOrDefault(x => x.JOBCODE == subjob.INTERNAL_NAME1);
                //    if (exoSubjob == null)
                //    {
                //        projectSummaryStats.AddMissingExoSubjob(subjob);
                //    }
                //}

                var jobTransactionsList = jobTransactions.ToList();
                if (jobTransactionsList.Count == 0)
                    return;

                List<DateTime> alignedDataDates = ChronologicalHelpers.GenerateAlignedDatesCollection(FirstAlignedDataDate, jobTransactionsList.Max(x => x.TRANSDATE).Value, ReportingInterval);
                HashSet<string> missingSubJobs = new HashSet<string>();
                foreach (var jobTransaction in jobTransactionsList)
                {
                    if (qualifiedSubjobs.Contains(jobTransaction.JOBCODE))
                    {
                        ExoDataPoint burnedDataPoint = new ExoDataPoint();
                        burnedDataPoint.BudgetedUnits = 0;
                        burnedDataPoint.BudgetedCosts = 0;
                        burnedDataPoint.Units = (decimal)jobTransaction.QUANTITY;
                        burnedDataPoint.Costs = (decimal)jobTransaction.LINETOTAL * this.CurrencyConversion;
                        burnedDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE);
                        burnedDataPoint.Subjob_Name = jobTransaction.JOBCODE;
                        burnedDataPoint.ResourceName = jobTransaction.RESOURCENAME;
                        burnedDataPoint.Quantity = (decimal)jobTransaction.QUANTITY;
                        burnedDataPoint.Role = jobTransaction.TITLE;
                        burnedDataPoint.CostGroup = jobTransaction.COSTDESC;
                        burnedDataPoint.CostType = jobTransaction.COSTDESC3;

                        burnedDataPoints.Add(burnedDataPoint);

                        ExoDataPoint actualDataPoint = new ExoDataPoint();
                        DataUtils.ShallowCopy(actualDataPoint, burnedDataPoint);
                        actualDataPoint.Costs = jobTransaction.LINECOST == null ? 0 : (decimal)jobTransaction.LINECOST;
                        actualDataPoints.Add(actualDataPoint);
                    }
                    else
                        missingSubJobs.Add(jobTransaction.JOBCODE);
                }

                foreach(string missingSubJob in missingSubJobs)
                {
                    SUBJOB newSUBJOB = new SUBJOB();
                    newSUBJOB.INTERNAL_NAME1 = missingSubJob;
                    newSUBJOB.MissingQuantity = Convert.ToDecimal(jobTransactionsList.Where(x => x.JOBCODE == missingSubJob && x.QUANTITY != null).Sum(x => x.QUANTITY));
                    projectSummaryStats.AddMissingExoSubjob(newSUBJOB);
                }

                projectSummaryStats.Burned = new Stats(summaryObject);
                projectSummaryStats.Actual = new Stats(summaryObject);

                projectSummaryStats.Burned.SetData(burnedDataPoints);
                projectSummaryStats.Actual.SetData(actualDataPoints);
                //LoadingScreenManager.Progress();
            }
            catch(Exception e)
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

        public void BuildEarnedDataPoints(IReportable reportable)
        {
            IEnumerable<DataPoint> progressItemEarnedDataPoints = reportable.PROGRESS_ITEM_UpToCurrentDataDate.Select(x => new DataPoint()
            {
                BudgetedUnits = reportable.Stats.BudgetedUnits,
                BudgetedCosts = reportable.Stats.BudgetedCosts * CurrencyConversion,
                Units = x.EARNED_UNITS,
                Costs = x.EARNED_UNITS * reportable.Budget_ItemRate * CurrencyConversion,
                ProgressDate = x.EARNED_DATE,
            }).ToArray();
            reportable.Stats.Earned.SetData(new ObservableCollection<DataPoint>(progressItemEarnedDataPoints));
        }

        public void BuildPlannedDataPointsFromQuery(IReportable reportable, decimal weightingPortion = 1)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_PlannedDataPoint> plannedDataPoints = bluePrintDataContext.QueryDeliverablePlannedDataPoints(reportable.EntityKey);
                Double weightingPortionDbl = Convert.ToDouble(weightingPortion);
                foreach (StoredProcedure_PlannedDataPoint plannedDataPoint in plannedDataPoints)
                {
                    plannedDataPoint.PeriodPlannedUnits *= weightingPortionDbl;
                    plannedDataPoint.PeriodPlannedPrice *= weightingPortionDbl;
                }

                reportable.Stats.Budgeted.SetPlannedData(plannedDataPoints);
                reportable.Stats.Current.SetPlannedData(plannedDataPoints);
            }
        }

        public void BuildRemainingDataPointsFromQuery(IReportable reportable, decimal weightingPortion = 1)
        {
            using (BluePrintsEntities bluePrintDataContext = new BluePrintsEntities())
            {
                List<StoredProcedure_RemainingDataPoint> RemainingDataPoints = bluePrintDataContext.QueryDeliverableRemainingDataPoints(reportable.EntityKey);
                Double weightingPortionDbl = Convert.ToDouble(weightingPortion);
                foreach (StoredProcedure_RemainingDataPoint remainingDataPoint in RemainingDataPoints)
                {
                    remainingDataPoint.PeriodRemainingUnits *= weightingPortionDbl;
                    remainingDataPoint.PeriodRemainingPrice *= weightingPortionDbl;
                }

                reportable.Stats.Remaining.SetRemainingData(RemainingDataPoints, reportable.Stats.Earned.DataPoints);
            }
        }
    }
}
