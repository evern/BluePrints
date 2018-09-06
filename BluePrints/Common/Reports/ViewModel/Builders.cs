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
                ObservableCollection<ExoDataPoint> materialDataPoints = new ObservableCollection<ExoDataPoint>();
                ObservableCollection<ExoDataPoint> poDataPoints = new ObservableCollection<ExoDataPoint>();
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
                var pos = from PURCHORD_LINES in PrimeroUnitOfWork.PURCHORD_LINES
                          join PURCHORD_HDR in PrimeroUnitOfWork.PURCHORD_HDR
                          on PURCHORD_LINES.HDR_SEQNO equals PURCHORD_HDR.SEQNO
                          join CR_ACCS in PrimeroUnitOfWork.CR_ACCS
                          on PURCHORD_HDR.ACCNO equals CR_ACCS.ACCNO
                          join JOBCOST_HDR in PrimeroUnitOfWork.JOBCOST_HDR
                          on PURCHORD_LINES.JOBNO equals JOBCOST_HDR.JOBNO
                          join JOBCOST_HDR2 in PrimeroUnitOfWork.JOBCOST_HDR
                          on JOBCOST_HDR.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                          join JOB_COSTTYPES in PrimeroUnitOfWork.JOB_COSTTYPES
                          on PURCHORD_LINES.COSTTYPE equals JOB_COSTTYPES.SEQNO
                          join JOB_COSTGROUPS in PrimeroUnitOfWork.JOB_COSTGROUPS
                          on PURCHORD_LINES.COSTGROUP equals JOB_COSTGROUPS.SEQNO
                          where PURCHORD_LINES.ORD_QUANT > PURCHORD_LINES.SUP_QUANT && PURCHORD_HDR.STATUS != 2 && JOBCOST_HDR2.JOBCODE == projectNumber
                          select new { PURCHORD_LINES.STOCKCODE, PURCHORD_LINES.DESCRIPTION, PURCHORD_HDR.SEQNO, PURCHORD_LINES.LINETOTAL, CR_ACCS.NAME, JOBCOST_HDR.JOBCODE, JOBCOST_HDR.TITLE, COSTTYPEDESC = JOB_COSTTYPES.COSTDESC, COSTGROUPDESC = JOB_COSTGROUPS.COSTDESC, PURCHORD_LINES.ORD_QUANT, PURCHORD_LINES.SUP_QUANT, PURCHORD_LINES.UNITPRICE, PURCHORD_HDR.STATUS, PURCHORD_HDR.DUEDATE, PURCHORD_HDR.ORDERDATE };
                
                var jobMaterials = from X_JOB_TRANSACTIONS_DETAIL in PrimeroUnitOfWork.X_JOB_TRANSACTIONS_DETAILS
                                   join JOBCOST_HDR in PrimeroUnitOfWork.JOBCOST_HDR
                                   on X_JOB_TRANSACTIONS_DETAIL.jobno equals JOBCOST_HDR.JOBNO
                                   join JOBCOST_HDR2 in PrimeroUnitOfWork.JOBCOST_HDR
                                   on JOBCOST_HDR.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                   join DR_ACCS in PrimeroUnitOfWork.DR_ACCS
                                   on JOBCOST_HDR.ACCNO equals DR_ACCS.ACCNO
                                   join STOCK_ITEMS in PrimeroUnitOfWork.STOCK_ITEMS
                                   on X_JOB_TRANSACTIONS_DETAIL.stockcode equals STOCK_ITEMS.STOCKCODE
                                   join GLP in PrimeroUnitOfWork.GLACCS
                                   on STOCK_ITEMS.PURCH_GL_CODE equals GLP.ACCNO
                                   join GLCOS in PrimeroUnitOfWork.GLACCS
                                   on STOCK_ITEMS.COS_GL_CODE equals GLCOS.ACCNO
                                   where X_JOB_TRANSACTIONS_DETAIL.linecharge == 0 && X_JOB_TRANSACTIONS_DETAIL.transtype == "C" && JOBCOST_HDR2.JOBCODE == projectNumber
                                   select new { X_JOB_TRANSACTIONS_DETAIL.jobno, X_JOB_TRANSACTIONS_DETAIL.master_jobno, X_JOB_TRANSACTIONS_DETAIL.jobcode, X_JOB_TRANSACTIONS_DETAIL.transdate, X_JOB_TRANSACTIONS_DETAIL.transtype, X_JOB_TRANSACTIONS_DETAIL.stockcode, X_JOB_TRANSACTIONS_DETAIL.description, X_JOB_TRANSACTIONS_DETAIL.quantity, X_JOB_TRANSACTIONS_DETAIL.unitcost, X_JOB_TRANSACTIONS_DETAIL.UNITPRICE, X_JOB_TRANSACTIONS_DETAIL.LINECOST, X_JOB_TRANSACTIONS_DETAIL.linecharge, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL_INCTAX, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL_TAX, X_JOB_TRANSACTIONS_DETAIL.LINE_STATUS, X_JOB_TRANSACTIONS_DETAIL.CostType, X_JOB_TRANSACTIONS_DETAIL.CostTypeDesc, X_JOB_TRANSACTIONS_DETAIL.Typeshortcode, X_JOB_TRANSACTIONS_DETAIL.COST_GROUP, X_JOB_TRANSACTIONS_DETAIL.CostGroupDesc, X_JOB_TRANSACTIONS_DETAIL.GroupShortcode, X_JOB_TRANSACTIONS_DETAIL.branchno, X_JOB_TRANSACTIONS_DETAIL.LINE_SOURCE, X_JOB_TRANSACTIONS_DETAIL.SOURCE_SEQNO, X_JOB_TRANSACTIONS_DETAIL.PO_LINESEQNO, X_JOB_TRANSACTIONS_DETAIL.POno, X_JOB_TRANSACTIONS_DETAIL.invseqno, X_JOB_TRANSACTIONS_DETAIL.refno, X_JOB_TRANSACTIONS_DETAIL.name, X_JOB_TRANSACTIONS_DETAIL.invno, X_JOB_TRANSACTIONS_DETAIL.CostActual, X_JOB_TRANSACTIONS_DETAIL.glcode, X_JOB_TRANSACTIONS_DETAIL.accno, JOBCOST_HDR.QUOTEDATE, JOBCOST_HDR.STARTDATE, JOBCOST_HDR.DUEDATE, JOBCOST_HDR.CUSTORDNO, JOBCOST_HDR.TITLE, NAME_2 = DR_ACCS.NAME, MasterJobcode = JOBCOST_HDR2.JOBCODE, STOCK_ITEMS.PURCH_GL_CODE, PurchGLName = GLP.NAME, STOCK_ITEMS.COS_GL_CODE, COSGlName = GLCOS.NAME };

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
                var jobTransactionsList = jobTransactions.ToList();

                
                if (jobTransactionsList.Count == 0)
                    return;

                List<DateTime> alignedDataDates = ChronologicalHelpers.GenerateAlignedDatesCollection(FirstAlignedDataDate, DateTime.Now.AddYears(1), ReportingInterval);
                HashSet<string> missingSubJobs = new HashSet<string>();
                foreach (var jobTransaction in jobTransactionsList)
                {
                    if (qualifiedSubjobs.Contains(jobTransaction.JOBCODE))
                    {
                        if (!jobTransaction.COSTDESC3.Substring(0, 3).Contains("G99") && !jobTransaction.COSTDESC3.Substring(0, 3).Contains("010"))
                        {
                            ExoDataPoint burnedDataPoint = new ExoDataPoint();
                            burnedDataPoint.BudgetedUnits = 0;
                            burnedDataPoint.BudgetedCosts = 0;
                            burnedDataPoint.Units = (decimal)jobTransaction.QUANTITY;
                            burnedDataPoint.Costs = (decimal)jobTransaction.LINETOTAL * this.CurrencyConversion;
                            burnedDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE);
                            burnedDataPoint.ActualDate = jobTransaction.TRANSDATE;
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
                    }
                    else
                        missingSubJobs.Add(jobTransaction.JOBCODE);
                }

                foreach (var jobMaterial in jobMaterials)
                {
                    if (!jobMaterial.CostGroupDesc.Substring(0, 3).Contains("G99") && !jobMaterial.CostGroupDesc.Substring(0, 3).Contains("010"))
                    {
                        ExoDataPoint materialDataPoint = new ExoDataPoint();
                        string s;
                        if (jobMaterial.description.Contains("Computer monitor"))
                            s = string.Empty;
                        materialDataPoint.BudgetedUnits = 0;
                        materialDataPoint.BudgetedCosts = 0;
                        materialDataPoint.Units = (decimal)jobMaterial.quantity;
                        materialDataPoint.Costs = (decimal)jobMaterial.LINECOST * this.CurrencyConversion;
                        materialDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobMaterial.transdate);
                        materialDataPoint.ActualDate = jobMaterial.transdate;
                        materialDataPoint.Subjob_Name = jobMaterial.jobcode;
                        materialDataPoint.ResourceName = string.Empty;
                        materialDataPoint.Quantity = (decimal)jobMaterial.quantity;
                        materialDataPoint.Description = jobMaterial.description;
                        materialDataPoint.Description2 = jobMaterial.name;
                        materialDataPoint.InvoiceNo = jobMaterial.invno;
                        materialDataPoint.CostGroup = jobMaterial.CostGroupDesc;
                        materialDataPoint.CostType = jobMaterial.CostTypeDesc;
                        materialDataPoint.Cost_GLName = jobMaterial.COSGlName;
                        materialDataPoint.Purchase_GLName = jobMaterial.PurchGLName;
                        Debug.Print(jobMaterial.description + ";" + materialDataPoint.Costs.ToString());
                        materialDataPoints.Add(materialDataPoint);
                    }
                }

                foreach (var po in pos)
                {
                    if (!po.COSTGROUPDESC.Substring(0, 3).Contains("G99") && !po.COSTGROUPDESC.Substring(0, 3).Contains("010"))
                    {
                        ExoDataPoint poDataPoint = new ExoDataPoint();
                        poDataPoint.BudgetedUnits = 0;
                        poDataPoint.BudgetedCosts = 0;
                        poDataPoint.Units = ((decimal)po.ORD_QUANT) - ((decimal)po.SUP_QUANT);
                        poDataPoint.Costs = poDataPoint.Units * ((decimal)po.UNITPRICE);
                        poDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= po.ORDERDATE);
                        poDataPoint.ActualDate = po.ORDERDATE;
                        poDataPoint.Subjob_Name = po.JOBCODE;
                        poDataPoint.ResourceName = string.Empty;
                        poDataPoint.Quantity = poDataPoint.Units;
                        poDataPoint.Description = po.DESCRIPTION;
                        poDataPoint.Description2 = po.NAME;
                        poDataPoint.InvoiceNo = string.Empty;
                        poDataPoint.CostGroup = po.COSTGROUPDESC;
                        poDataPoint.CostType = po.COSTTYPEDESC;
                        poDataPoint.Cost_GLName = string.Empty;
                        poDataPoint.Purchase_GLName = string.Empty;
                        poDataPoints.Add(poDataPoint);
                    }
                }

                foreach (string missingSubJob in missingSubJobs)
                {
                    SUBJOB newSUBJOB = new SUBJOB();
                    newSUBJOB.INTERNAL_NAME1 = missingSubJob;
                    newSUBJOB.MissingQuantity = Convert.ToDecimal(jobTransactionsList.Where(x => x.JOBCODE == missingSubJob && x.QUANTITY != null).Sum(x => x.QUANTITY));
                    projectSummaryStats.AddMissingExoSubjob(newSUBJOB);
                }

                projectSummaryStats.Burned = new Stats(summaryObject);
                projectSummaryStats.Actual = new Stats(summaryObject);
                projectSummaryStats.Material = new Stats(summaryObject);
                projectSummaryStats.PO = new Stats(summaryObject);
                projectSummaryStats.RemainingActual = new Stats(summaryObject, true);

                projectSummaryStats.Burned.SetData(burnedDataPoints);
                projectSummaryStats.Actual.SetData(actualDataPoints);
                projectSummaryStats.Material.SetData(materialDataPoints);
                projectSummaryStats.PO.SetData(poDataPoints);

                projectSummaryStats.RemainingActual.SetRemainingActualData(projectSummaryStats.Reportables, projectSummaryStats.Burned.GetData());
                //LoadingScreenManager.Progress();
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
            IEnumerable<DataPoint> progressItemEarnedDataPoints = reportable.PROGRESS_ITEM_UpToCurrentDataDate.Select(x => new DataPoint()
            {
                BudgetedUnits = reportable.Stats.BudgetedUnits,
                BudgetedCosts = reportable.Stats.BudgetedCosts * CurrencyConversion,
                Units = x.EARNED_UNITS,
                Quantity = x.EARNED_UNITS * qtyPerUnit,
                Costs = x.EARNED_UNITS * reportable.Budget_ItemRate * CurrencyConversion,
                ProgressDate = x.EARNED_DATE,
            }).ToArray();
            reportable.Stats.Earned.SetData(new ObservableCollection<DataPoint>(progressItemEarnedDataPoints));
            reportable.Stats.TenderEarned.SetData(new ObservableCollection<DataPoint>(progressItemEarnedDataPoints));
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
