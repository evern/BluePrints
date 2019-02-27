using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace BluePrints.ViewModels
{
    public class PROJECTSummaryViewModelWrapper : PROJECTForecastViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECTSummaryViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTSummaryViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTSummaryViewModelWrapper(unitOfWorkFactory));
        }


        /// Initializes a new instance of the PROJECTSummaryViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTSummaryViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTSummaryViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            isExcelExportDataAware = false;
        }

        #region Database Operations
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IP6EntitiesUnitOfWork p6UnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
        }

        PROJECT_SUMMARY_SETTING loadProject_Summary_Setting;
        public PROJECT_SUMMARY_SETTING PROJECT_SUMMARY_SETTINGS => loadProject_Summary_Setting;
        protected PROGRESS live_PROGRESS;
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECASTS, FORECASTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_SUMMARIES, PROJECT_SUMMARYProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_SUMMARY_SETTINGS, PROJECT_SUMMARY_SETTINGProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            base.addEntitiesLoader();
        }

        private Func<IRepositoryQuery<FORECAST>, IQueryable<FORECAST>> FORECASTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<PROJECT_SUMMARY>, IQueryable<PROJECT_SUMMARY>> PROJECT_SUMMARYProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJECT_SUMMARY_SETTING>, IQueryable<PROJECT_SUMMARY_SETTING>> PROJECT_SUMMARY_SETTINGProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Project_Summary.ToString());
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            loadProject_Summary_Setting = PROJECT_SUMMARY_SETTINGCollection.FirstOrDefault();
            return base.OnMainViewModelLoaded(entities);
        }

        private DataTable summaryDataPointsTable;
        public override DataTable DataPointsTable
        {
            get
            {
                if(base.DataPointsTable != null)
                {
                    if(summaryDataPointsTable == null)
                    {
                        summaryDataPointsTable = new DataTable();
                        summaryDataPointsTable.Columns.Add(columnEntity, typeof(ProjectSummary));

                        populateRow(StaticSummaryRowTypes.Indirect_Man_Hours);
                        populateRow(StaticSummaryRowTypes.Direct_Man_Hours);
                        populateRow(StaticSummaryRowTypes.Costs);
                        updateDataTableGlobalColumns();
                    }
                }

                return summaryDataPointsTable;
            }
        }

        private void updateDataTableGlobalColumns()
        {
            if (summaryDataPointsTable == null || loadProject_Summary_Setting == null)
                return;

            IEnumerable<DataRow> dataRowCollection = from DataRow dr in summaryDataPointsTable.Rows
                                                     select dr;

            foreach(DataRow dataRow in dataRowCollection)
            {
                ProjectSummary entity = (ProjectSummary)dataRow[columnEntity];
                entity.Unapproved_EOT = loadProject_Summary_Setting.UNAPPROVED_EOT_DAYS;
                entity.Contract_Completion_Date = loadProject_Summary_Setting.CONTRACT_COMPLETION_DATE;
            }
        }

        private DataRow findRow(StaticSummaryRowTypes rowType)
        {
            IEnumerable<DataRow> dataRowCollection = from DataRow dr in summaryDataPointsTable.Rows
                                                     select dr;

            return dataRowCollection.FirstOrDefault(x => ((ProjectSummary)x[columnEntity]).RowType == rowType);
        }

        private void populateRow(StaticSummaryRowTypes rowType, bool isUpdate = false)
        {
            DataTable baseDataTable = base.DataPointsTable;
            IEnumerable<DataRow> baseDataRows = from DataRow dr in baseDataTable.Rows
                                                     select dr;

            DataRow newRow;
            if (!isUpdate)
                newRow = summaryDataPointsTable.NewRow();
            else
                newRow = findRow(rowType);

            if (newRow == null)
                return;

            List<DataRow> filteredDataRows = new List<DataRow>();
            List<DashboardFlatStructure> filteredDashboards = new List<DashboardFlatStructure>();
            IEnumerable<DataRow> indirectRows = baseDataRows.Where(x => BluePrintsDataUtils.GetPhaseCode(((ExoSubJobProjection)x[columnEntity]).SubJob.Code).Contains(BluePrintsResources.IndirectPhaseCode));
            IEnumerable<DataRow> procurementRows = baseDataRows.Where(x => BluePrintsDataUtils.GetPhaseCode(((ExoSubJobProjection)x[columnEntity]).SubJob.Code).Contains(BluePrintsResources.ProcurementPhaseCode));
            IEnumerable<DataRow> directRows = baseDataRows.Where(x => BluePrintsDataUtils.GetPhaseCode(((ExoSubJobProjection)x[columnEntity]).SubJob.Code).Contains(BluePrintsResources.DirectPhaseCode));
            IEnumerable<DataRow> designRows = baseDataRows.Where(x => BluePrintsDataUtils.GetPhaseCode(((ExoSubJobProjection)x[columnEntity]).SubJob.Code).Contains(BluePrintsResources.DesignPhaseCode));

            IEnumerable<DashboardFlatStructure> indirectDashboards = AllProjectDashboards.Where(x => BluePrintsDataUtils.GetPhaseCode(x.SubjobCode).Contains(BluePrintsResources.IndirectPhaseCode));
            IEnumerable<DashboardFlatStructure> procurementDashboards = AllProjectDashboards.Where(x => BluePrintsDataUtils.GetPhaseCode(x.SubjobCode).Contains(BluePrintsResources.ProcurementPhaseCode));
            IEnumerable<DashboardFlatStructure> designDashboards = AllProjectDashboards.Where(x => BluePrintsDataUtils.GetPhaseCode(x.SubjobCode).Contains(BluePrintsResources.DesignPhaseCode));
            IEnumerable<DashboardFlatStructure> directDashboards = AllProjectDashboards.Where(x => BluePrintsDataUtils.GetPhaseCode(x.SubjobCode).Contains(BluePrintsResources.DirectPhaseCode));

            IEnumerable<Stats> indirectActualStats = indirectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
            IEnumerable<Stats> procurementActualStats = procurementDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
            IEnumerable<Stats> designActualStats = designDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
            IEnumerable<Stats> directActualStats = directDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
            IEnumerable<Stats> directDesignRemainingStats = designDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null).Select(x => x.Stats.Remaining);
            IEnumerable<Stats> directDesignPlannedStats = designDashboards.Where(x => x.Stats != null && x.Stats.Budgeted != null).Select(x => x.Stats.Budgeted);
            IEnumerable<Stats> directDesignEarnedStats = designDashboards.Where(x => x.Stats != null && x.Stats.Earned != null).Select(x => x.Stats.Earned);

            List<Tuple<string, string>> fieldNamesLookup = new List<Tuple<string, string>>();
            //fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Total_Budget), GetPropertyName(() => new ProjectSummary().ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Construction_Budget), GetPropertyName(() => new ProjectSummary().Construction_Budget_ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Construction_Period_Planned), GetPropertyName(() => new ProjectSummary().Construction_Planned_ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Construction_Earned), GetPropertyName(() => new ProjectSummary().Construction_Earned_ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Construction_Remaining), GetPropertyName(() => new ProjectSummary().Construction_Remaining_ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Total_Remaining), GetPropertyName(() => new ProjectSummary().Total_Remaining_ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Original_Contract_Value), GetPropertyName(() => new ProjectSummary().ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Unapproved_EOT), GetPropertyName(() => new ProjectSummary().ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Approved_Variation), GetPropertyName(() => new ProjectSummary().ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Unapproved_Variation), GetPropertyName(() => new ProjectSummary().ReadOnly)));
            fieldNamesLookup.Add(new Tuple<string, string>(GetPropertyName(() => new ProjectSummary().Contract_Completion_Date), GetPropertyName(() => new ProjectSummary().ReadOnly)));

            ProjectSummary entity = new ProjectSummary();
            entity.Lookup = fieldNamesLookup;
            switch (rowType)
            {
                case StaticSummaryRowTypes.Indirect_Man_Hours:
                    filteredDataRows.AddRange(indirectRows);
                    filteredDataRows.AddRange(procurementRows);
                    filteredDashboards.AddRange(indirectDashboards);
                    filteredDashboards.AddRange(procurementDashboards);
                    break;
                case StaticSummaryRowTypes.Direct_Man_Hours:
                    filteredDataRows.AddRange(directRows);
                    filteredDataRows.AddRange(designRows);
                    filteredDashboards.AddRange(directDashboards);
                    filteredDashboards.AddRange(designDashboards);
                    break;
                default:
                    filteredDataRows.AddRange(indirectRows);
                    filteredDataRows.AddRange(procurementRows);
                    filteredDataRows.AddRange(directRows);
                    filteredDataRows.AddRange(designRows);
                    filteredDashboards.AddRange(indirectDashboards);
                    filteredDashboards.AddRange(procurementDashboards);
                    filteredDashboards.AddRange(directDashboards);
                    filteredDashboards.AddRange(designDashboards);
                    break;
            }

            IEnumerable<ForecastCalculation> filteredForecastCalculaton = filteredDataRows.Select(x => (ForecastCalculation)x[columnCalculation]);
            IEnumerable<ExoSubJobProjection> filteredSubJobs = filteredDataRows.Select(x => (ExoSubJobProjection)x[columnEntity]);
            PROJECT_SUMMARY PROJECT_SUMMARY = PROJECT_SUMMARYCollection.FirstOrDefault(x => x.PHASE_TYPE == rowType);

            decimal? designEarned = 0;
            decimal? constructionEarned = 0;
            decimal totalEarned = 0;
            decimal? designPlanned = 0;
            decimal? constructionPlanned = 0;
            decimal totalPlanned = 0;
            decimal? designBudget = 0;
            decimal? constructionBudget = 0;
            decimal totalBudget = 0;
            decimal? designRemaining = 0;
            decimal? constructionRemaining = 0;
            decimal totalRemaining = 0;
            decimal actual = 0;
            decimal originalContractValue = 0;
            decimal approvedVariations = 0;
            bool totalBudgetReadOnly = true;
            bool totalRemainingReadOnly = true;
            bool constructionBudgetReadOnly = true;
            bool constructionRemainingReadOnly = true;
            bool constructionPlannedReadOnly = true;
            bool constructionEarnedReadOnly = true;

            if (PROJECT_SUMMARY != null)
            {
                originalContractValue = PROJECT_SUMMARY.ORI_CONTRACT == null ? 0 : (decimal)PROJECT_SUMMARY.ORI_CONTRACT;
                approvedVariations = PROJECT_SUMMARY.APPROVED_VAR == null ? 0 : (decimal)PROJECT_SUMMARY.APPROVED_VAR;

                entity.Original_Contract_Value = originalContractValue;
                entity.Approved_Variation = approvedVariations;
                entity.Unapproved_Variation = PROJECT_SUMMARY.UNAPPROVED_VAR == null ? 0 : (decimal)PROJECT_SUMMARY.UNAPPROVED_VAR;
            }

            if (rowType == StaticSummaryRowTypes.Costs)
            {
                //designBudget = directDesignPlannedStats.Where(x => x.DataPoints != null).Sum(x => x.DataPoints.Sum(y => y.BudgetedCosts));
                designBudget = null;
                constructionBudget = null;
                //totalBudget = filteredForecastCalculaton.Sum(x => x.Budget);
                totalBudget = originalContractValue + approvedVariations;
                //designRemaining = directDesignRemainingStats.Where(x => x.DataPoints != null).Sum(x => x.DataPoints.Sum(y => y.Costs));
                designRemaining = null;
                constructionRemaining = null;
                totalRemaining = filteredForecastCalculaton.Sum(x => x.EstimateToComplete);
                //designPlanned = directDesignPlannedStats.Where(x => x.CurrentPeriodCumulativeDataPoint != null).Sum(x => x.CurrentPeriodCumulativeDataPoint.Costs);

                designPlanned = null;
                constructionPlanned = null;
                totalPlanned = filteredForecastCalculaton.Sum(x => x.PreviousEAC);

                actual = filteredForecastCalculaton.Sum(x => x.Actuals);
                //designEarned = directDesignEarnedStats.Where(x => x.CurrentPeriodCumulativeDataPoint != null).Sum(x => x.CurrentPeriodCumulativeDataPoint.Costs);
                designEarned = null;
                constructionEarned = null;
                totalEarned = ForecastSummary.TotalClaims;
            }
            else
            {
                if (rowType == StaticSummaryRowTypes.Indirect_Man_Hours)
                {
                    actual = indirectActualStats.Sum(x => x.ExoDataPoints.Sum(y => y.Units)) + procurementActualStats.Sum(x => x.ExoDataPoints.Sum(y => y.Units));
                    IEnumerable<TASK> indirectTASKS = TASKS(BluePrintsResources.P6_Procurement_ACTVCODE);
                    constructionBudget = null;
                    constructionRemaining = null;
                    constructionPlanned = null;
                    constructionEarned = null;
                    totalEarned = actual;

                    totalBudget = originalContractValue + approvedVariations;
                    if (LoadP6PROJECT != null)
                    {
                        //totalBudget = indirectTASKS.Where(x => x.target_work_qty != null).Sum(x => (decimal)x.target_work_qty);
                        totalPlanned = getPeriodCumulativePlanned(indirectTASKS);
                        totalRemaining = indirectTASKS.Where(x => x.remain_work_qty != null).Sum(x => (decimal)x.remain_work_qty);
                    }
                    else
                    {
                        //totalBudgetReadOnly = false;
                        totalRemainingReadOnly = false;
                        if (PROJECT_SUMMARY != null)
                        {
                            //totalBudget = PROJECT_SUMMARY.BUDGET_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.BUDGET_UNITS;
                            totalPlanned = PROJECT_SUMMARY.PLANNED_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.PLANNED_UNITS;
                            totalRemaining = PROJECT_SUMMARY.FORECAST_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.FORECAST_UNITS;
                        }
                    }
                }
                else
                {
                    actual = designActualStats.Sum(x => x.ExoDataPoints.Sum(y => y.Units)) + directActualStats.Sum(x => x.ExoDataPoints.Sum(y => y.Units));

                    if(LoadP6PROJECT != null)
                    {
                        IEnumerable<TASK> directTASKS = TASKS(BluePrintsResources.P6_Construction_ACTVCODE);
                        //constructionBudget = directTASKS.Where(x => x.target_work_qty != null).Sum(x => (decimal)x.target_work_qty);
                        constructionRemaining = directTASKS.Where(x => x.remain_work_qty != null).Sum(x => (decimal)x.remain_work_qty);
                        constructionEarned = directTASKS.Where(x => x.act_work_qty != null).Sum(x => (decimal)x.act_work_qty);
                        constructionPlanned = getPeriodCumulativePlanned(directTASKS);

                        IEnumerable<TASK> designTASKS = TASKS(BluePrintsResources.P6_Design_ACTVCODE);
                        //designBudget = designTASKS.Where(x => x.target_work_qty != null).Sum(x => (decimal)x.target_work_qty);
                        designRemaining = designTASKS.Where(x => x.remain_work_qty != null).Sum(x => (decimal)x.remain_work_qty);
                        designEarned = designTASKS.Where(x => x.act_work_qty != null).Sum(x => (decimal)x.act_work_qty);
                        designPlanned = getPeriodCumulativePlanned(designTASKS);
                    }
                    else
                    {
                        //designBudget = directDesignPlannedStats.Where(x => x.DataPoints != null).Sum(x => x.DataPoints.Sum(y => y.BudgetedUnits));
                        designRemaining = directDesignRemainingStats.Where(x => x.DataPoints != null).Sum(x => x.DataPoints.Sum(y => y.Units));
                        designEarned = directDesignEarnedStats.Where(x => x.CurrentPeriodCumulativeDataPoint != null).Sum(x => x.CurrentPeriodCumulativeDataPoint.Units);
                        designPlanned = directDesignPlannedStats.Where(x => x.CurrentPeriodCumulativeDataPoint != null).Sum(x => x.CurrentPeriodCumulativeDataPoint.Units);

                        //constructionBudgetReadOnly = false;
                        constructionRemainingReadOnly = false;
                        constructionEarnedReadOnly = false;
                        constructionPlannedReadOnly = false;
                        if (PROJECT_SUMMARY != null)
                        {
                            //constructionBudget = PROJECT_SUMMARY.BUDGET_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.BUDGET_UNITS;
                            constructionRemaining = PROJECT_SUMMARY.FORECAST_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.FORECAST_UNITS;
                            constructionEarned = PROJECT_SUMMARY.EARNED_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.EARNED_UNITS;
                            constructionPlanned = PROJECT_SUMMARY.PLANNED_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.PLANNED_UNITS;
                        }
                    }

                    totalBudget = originalContractValue + approvedVariations;
                    //totalBudget = (decimal)designBudget + (decimal)constructionBudget;
                    totalRemaining = (decimal)designRemaining + (decimal)constructionRemaining;
                    totalEarned = (decimal)designEarned + (decimal)constructionEarned;
                    totalPlanned = (decimal)designPlanned + (decimal)constructionPlanned;
                }
            }


            decimal forecast = actual + totalRemaining;
            decimal currentContractValue = originalContractValue + approvedVariations;

            entity.RowType = rowType;
            entity.Design_Budget = designBudget;
            entity.Construction_Budget = constructionBudget;
            entity.Total_Budget = totalBudget;
            entity.Design_Remaining = designRemaining;
            entity.Construction_Remaining = constructionRemaining;
            entity.Total_Remaining = totalRemaining;
            entity.EAC = forecast;
            entity.Total_Actuals = actual;
            entity.Design_Earned = designEarned;
            entity.Construction_Earned = constructionEarned;
            entity.Total_Earned = totalEarned;
            entity.Design_Period_Planned = designPlanned;
            entity.Construction_Period_Planned = constructionPlanned;
            entity.Total_Period_Planned = totalPlanned;

            IEnumerable<BluePrints.Common.ViewModel.Reporting.DataPoint> remainingDataPoints = filteredDashboards.Where(x => x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints);
            DateTime lastForecastDate = FORECASTCollection.Count() == 0 ? DateTime.Now : FORECASTCollection.Max(x => x.FORECAST_DATE);
            DateTime lastRemainingDate = remainingDataPoints.Count() == 0 ? DateTime.Now : remainingDataPoints.Max(x => x.ProgressDate);

            entity.Forecast_Completion_Date = lastForecastDate < lastRemainingDate ? lastRemainingDate : lastForecastDate;
            entity.Mask = rowType == StaticSummaryRowTypes.Costs ? "c0" : "n0";
            entity.SPI = totalPlanned == 0 ? 0 : totalEarned / totalPlanned;
            entity.CPI = actual == 0 ? 0 : totalEarned / actual;
            entity.Current_Contract_Value = currentContractValue;
            entity.GPM = currentContractValue == 0 ? 0 : (currentContractValue - forecast) / currentContractValue;
            entity.Construction_TotalBudget_ReadOnly = totalBudgetReadOnly;
            entity.Total_Remaining_ReadOnly = totalRemainingReadOnly;
            entity.Construction_Budget_ReadOnly = constructionBudgetReadOnly;
            entity.Construction_Planned_ReadOnly = constructionPlannedReadOnly;
            entity.Construction_Remaining_ReadOnly = constructionRemainingReadOnly;
            entity.Construction_Earned_ReadOnly = constructionEarnedReadOnly;
            entity.ReadOnly = false;

            newRow[columnEntity] = entity;
            if(!isUpdate)
                summaryDataPointsTable.Rows.Add(newRow);
        }
        #endregion

        #region View Events
        protected override void commitCellValue(string fieldName, DataRow row, object oldValue, object newValue)
        {
            fieldName = fieldName.Replace(string.Concat(columnEntity, "."), "");
            if (fieldName == GetPropertyName(() => new ProjectSummary().Unapproved_EOT))
                UnapprovedEOT = (decimal)newValue;
            else if (fieldName == GetPropertyName(() => new ProjectSummary().Contract_Completion_Date))
                ContractCompletion = (DateTime)newValue;
            else
            {
                StaticSummaryRowTypes rowType = getRowType(row, fieldName);
                findExistingOrAddPROJECT_SUMMARY(rowType, fieldName, newValue);
            }

            //since undo deletion cannot be performed, undo will always be added as changed
            EntitiesUndoRedoManager.AddUndo(row, fieldName, oldValue, newValue, EntityMessageType.Changed);
        }

        private PROJECT_SUMMARY findExistingOrAddPROJECT_SUMMARY(StaticSummaryRowTypes rowType, string fieldName, object newValue)
        {
            fieldName = fieldName.Replace(string.Concat(columnEntity, "."), "");
            PROJECT_SUMMARY findPROJECT_SUMMARY = PROJECT_SUMMARYCollectionViewModel.Entities.FirstOrDefault(x => x.PHASE_TYPE == rowType);
            if (findPROJECT_SUMMARY == null)
            {
                findPROJECT_SUMMARY = new PROJECT_SUMMARY();
                findPROJECT_SUMMARY.GUID = Guid.Empty;
                findPROJECT_SUMMARY.GUID_PROJECT = loadPROJECT.GUID;
                findPROJECT_SUMMARY.PHASE_TYPE = rowType;
            }

            UpdatePROJECT_SUMMARYProperties(findPROJECT_SUMMARY, fieldName, newValue);
            PROJECT_SUMMARYCollectionViewModel.Save(findPROJECT_SUMMARY);
            populateRow(rowType, true);
            return findPROJECT_SUMMARY;
        }

        private void findExistingOrAddPROJECT_SUMMARY_SETTINGS()
        {
            PROJECT_SUMMARY_SETTING findPROJECT_SUMMARY_SETTING = PROJECT_SUMMARY_SETTINGCollection.FirstOrDefault(x => x.GUID_PROJECT == loadPROJECT.GUID);
            if (findPROJECT_SUMMARY_SETTING == null)
            {
                findPROJECT_SUMMARY_SETTING = new PROJECT_SUMMARY_SETTING();
                findPROJECT_SUMMARY_SETTING.GUID = Guid.Empty;
                findPROJECT_SUMMARY_SETTING.GUID_PROJECT = loadPROJECT.GUID;
            }

            findPROJECT_SUMMARY_SETTING.UNAPPROVED_EOT_DAYS = loadProject_Summary_Setting.UNAPPROVED_EOT_DAYS;
            findPROJECT_SUMMARY_SETTING.CONTRACT_COMPLETION_DATE = loadProject_Summary_Setting.CONTRACT_COMPLETION_DATE;

            PROJECT_SUMMARY_SETTINGCollectionViewModel.Save(findPROJECT_SUMMARY_SETTING);
            loadProject_Summary_Setting = findPROJECT_SUMMARY_SETTING;

            updateDataTableGlobalColumns();
            GridControlService.RefreshData();
            this.RaisePropertyChanged(x => x.PROJECT_SUMMARY_SETTINGS);
        }

        private void UpdatePROJECT_SUMMARYProperties(PROJECT_SUMMARY project_summary, string fieldName, object newValue)
        {
            if(fieldName == GetPropertyName(() => new ProjectSummary().Original_Contract_Value))
                project_summary.ORI_CONTRACT = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Approved_Variation))
                project_summary.APPROVED_VAR = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Unapproved_Variation))
                project_summary.UNAPPROVED_VAR = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Construction_Budget))
                project_summary.BUDGET_UNITS = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Construction_Earned))
                project_summary.EARNED_UNITS = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Construction_Period_Planned))
                project_summary.PLANNED_UNITS = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Construction_Remaining))
                project_summary.FORECAST_UNITS = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Total_Budget))
                project_summary.BUDGET_UNITS = (decimal)newValue;
            if(fieldName == GetPropertyName(() => new ProjectSummary().Total_Remaining))
                project_summary.FORECAST_UNITS = (decimal)newValue;
        }

        private decimal getPeriodCumulativePlanned(IEnumerable<TASK> TASKS)
        {
            decimal periodCumulativePlanned = 0;
            if (LoadP6PROJECT.last_recalc_date != null)
            {
                foreach (TASK task in TASKS)
                {
                    if (task.target_drtn_hr_cnt == null || task.target_start_date == null || task.target_end_date == null)
                        continue;

                    DateTime startDate = (DateTime)task.target_start_date;
                    DateTime endDate = (DateTime)task.target_end_date;

                    TimeSpan totalDays = endDate - startDate;
                    decimal totalWorkingHours = (decimal)task.target_drtn_hr_cnt;
                    if (totalWorkingHours == 0)
                        continue;

                    decimal totalUnits = task.target_work_qty == null ? 0 : (decimal)task.target_work_qty;
                    decimal taskTotalDays = Convert.ToDecimal(totalDays.TotalDays);
                    //get pro-rated working days per day
                    decimal workingHoursPerDays = totalWorkingHours / taskTotalDays;
                    DateTime lastRecalcDate = ((DateTime)LoadP6PROJECT.last_recalc_date);
                    TimeSpan elapsedTimespan = lastRecalcDate - startDate;
                    if(elapsedTimespan.TotalDays > 0)
                    {
                        decimal elapsedDays = Convert.ToDecimal(elapsedTimespan.TotalDays);
                        decimal elapsedWorkingHours = elapsedDays * workingHoursPerDays;
                        decimal pctComplete = elapsedWorkingHours / totalWorkingHours;

                        periodCumulativePlanned += (pctComplete * totalUnits);
                    }
                }
            }

            return periodCumulativePlanned;
        }

        private StaticSummaryRowTypes getRowType(DataRow row, string fieldName)
        {
            return ((ProjectSummary)row[columnEntity]).RowType;
        }

        public override void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                if (entityProperty.PropertyName == GetPropertyName(() => new ProjectSummary().Unapproved_EOT))
                    UnapprovedEOT = (decimal?)entityProperty.OldValue;
                else if (entityProperty.PropertyName == GetPropertyName(() => new ProjectSummary().Contract_Completion_Date))
                    ContractCompletion = (DateTime?)entityProperty.OldValue;
                else
                {
                    StaticSummaryRowTypes rowType = getRowType(entityProperty.ChangedEntity, entityProperty.PropertyName);
                    object oldValue = entityProperty.OldValue;
                    if (oldValue == null || oldValue == DBNull.Value)
                        oldValue = 0.00m;

                    entityProperty.ChangedEntity[entityProperty.PropertyName] = oldValue;
                    findExistingOrAddPROJECT_SUMMARY(rowType, entityProperty.PropertyName, oldValue);
                }
            }

            GridControlService.RefreshData();
            isBackgroundEdit = false;
        }

        public override void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                if (entityProperty.PropertyName == GetPropertyName(() => new ProjectSummary().Unapproved_EOT))
                    UnapprovedEOT = (decimal?)entityProperty.NewValue;
                else if (entityProperty.PropertyName == GetPropertyName(() => new ProjectSummary().Contract_Completion_Date))
                    ContractCompletion = (DateTime?)entityProperty.NewValue;
                else
                {
                    StaticSummaryRowTypes rowType = getRowType(entityProperty.ChangedEntity, entityProperty.PropertyName);
                    object newValue = entityProperty.NewValue;
                    if (newValue == null || newValue == DBNull.Value)
                        newValue = 0;

                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;
                    findExistingOrAddPROJECT_SUMMARY(rowType, entityProperty.PropertyName, newValue);
                }
            }

            GridControlService.RefreshData();
            isBackgroundEdit = false;
        }

        public override void FullRefresh()
        {
            summaryDataPointsTable = null;
            base.FullRefresh();
        }

        [NumericMask(Mask = "#0 h", UseAsDisplayFormat = true)]
        public decimal? UnapprovedEOT
        {
            get
            {
                if (PROJECT_SUMMARY_SETTINGS == null)
                    return 0;

                return PROJECT_SUMMARY_SETTINGS.UNAPPROVED_EOT_DAYS;
            }
            set
            {
                if (PROJECT_SUMMARY_SETTINGS == null)
                    loadProject_Summary_Setting = new PROJECT_SUMMARY_SETTING();

                PROJECT_SUMMARY_SETTINGS.UNAPPROVED_EOT_DAYS = value;
                findExistingOrAddPROJECT_SUMMARY_SETTINGS();
            }
        }

        public DateTime? ContractCompletion
        {
            get
            {
                if (PROJECT_SUMMARY_SETTINGS == null)
                    loadProject_Summary_Setting = new PROJECT_SUMMARY_SETTING();

                return PROJECT_SUMMARY_SETTINGS.CONTRACT_COMPLETION_DATE;
            }
            set
            {
                if (PROJECT_SUMMARY_SETTINGS == null)
                    loadProject_Summary_Setting = new PROJECT_SUMMARY_SETTING();

                PROJECT_SUMMARY_SETTINGS.CONTRACT_COMPLETION_DATE = value;
                findExistingOrAddPROJECT_SUMMARY_SETTINGS();
            }
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTSummaryViewModelWrapper"; }
        }

        public IEnumerable<FORECAST> FORECASTCollection
        {
            get
            {
                return GetEntities<FORECAST>();
            }
        }

        public IEnumerable<PROJECT_SUMMARY> PROJECT_SUMMARYCollection
        {
            get
            {
                return GetEntities<PROJECT_SUMMARY>();
            }
        }

        //hide design and construction specific stats when P6 project is found
        public bool ShowStats
        {
            get
            {
                return LoadP6PROJECT == null;
            }
        }

        protected P6Data.PROJECT LoadP6PROJECT
        {
            get
            {
                if (liveDesignProgress == null || liveDesignProgress.P6PROGRESS_NAME == string.Empty)
                    return null;
                else
                    return p6UnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == liveDesignProgress.P6PROGRESS_NAME);
            }
        }

        List<TASK> taskCollection;
        public IEnumerable<TASK> TASKCollection
        {
            get
            {
                if(taskCollection == null)
                {
                    if (LoadP6PROJECT == null)
                        taskCollection = new List<TASK>();
                    else
                        taskCollection = p6UnitOfWork.TASK.Where(x => x.proj_id == LoadP6PROJECT.proj_id).ToList();
                }

                return taskCollection;
            }
        }

        public IEnumerable<P6Data.PROJECT> P6PROJECTCollection
        {
            get
            {
                return GetEntities<P6Data.PROJECT>();
            }
        }

        public CollectionViewModel<PROJECT_SUMMARY, PROJECT_SUMMARY, Guid, IBluePrintsEntitiesUnitOfWork> PROJECT_SUMMARYCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<PROJECT_SUMMARY, PROJECT_SUMMARY, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_SUMMARY>();
            }
        }

        public IEnumerable<PROJECT_SUMMARY_SETTING> PROJECT_SUMMARY_SETTINGCollection
        {
            get
            {
                return GetEntities<PROJECT_SUMMARY_SETTING>();
            }
        }

        public CollectionViewModel<PROJECT_SUMMARY_SETTING, PROJECT_SUMMARY_SETTING, Guid, IBluePrintsEntitiesUnitOfWork> PROJECT_SUMMARY_SETTINGCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<PROJECT_SUMMARY_SETTING, PROJECT_SUMMARY_SETTING, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_SUMMARY_SETTING>();
            }
        }

        private IEnumerable<TASK> TASKS(string phaseActvCode)
        {
            if (LoadP6PROJECT == null)
                return new List<TASK>();

            List<TASK> returnTaskCollection = new List<TASK>();
            foreach (TASK task in TASKCollection)
            {
                if (task.delete_date != null)
                    continue;

                if (task.TASKACTV.Any(taskact => taskact.ACTVCODE != null && taskact.ACTVCODE.short_name.ToUpper() == phaseActvCode))
                    returnTaskCollection.Add(task);
            }

            return returnTaskCollection.AsQueryable();
        }

        public override bool CanEditReport()
        {
            return !IsLoadingForecast;
        }

        public override void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Project_Summary);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public override bool CanViewReport()
        {
            return !IsLoadingForecast;
        }

        public override void ViewReport()
        {
            XtraReportProjectSummary summaryReport = new XtraReportProjectSummary();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    summaryReport.LoadLayout(sw.BaseStream);
                }
            }

            List<ProjectSummary> projectSummaries = new List<ProjectSummary>();
            IEnumerable<DataRow> dataRowCollection = from DataRow dr in summaryDataPointsTable.Rows
                                                     select dr;

            foreach (var dataRow in dataRowCollection)
            {
                ProjectSummary rowSummary = (ProjectSummary)dataRow[columnEntity];
                projectSummaries.Add(rowSummary);
            }

            summaryReport.AssignProperties(projectSummaries, loadPROJECT.NUMBER, FixedDataDate);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = summaryReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            summaryReport.RequestParameters = false;
            summaryReport.CreateDocument(true);
            previewWindow.Show();
        }
        #endregion
    }
}