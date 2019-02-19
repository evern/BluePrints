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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
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

        private enum Fields
        {
            RowType,
            Design_Budget,
            Design_Remaining,
            Total_Actuals,
            EAC,
            Design_Earned,
            Total_Earned,
            Design_Period_Planned,
            Total_Period_Planned,
            SPI,
            CPI,
            Current_Contract_Value,
            GPM,
            Forecast_Completion_Date,
            Mask,
            Construction_TotalBudget_ReadOnly,
            Construction_Budget_ReadOnly,
            Construction_Planned_ReadOnly,
            Construction_Earned_ReadOnly,
            Construction_Remaining_ReadOnly,
            Total_Remaining_ReadOnly,
            Always_Read_Only,
            Lookup
        }

        private enum EditableFields
        {
            Total_Budget,
            Construction_Budget,
            Construction_Period_Planned,
            Construction_Earned,
            Construction_Remaining,
            Total_Remaining,
            Original_Contract_Value,
            Approved_Variation,
            Unapproved_Variation
        }

        private enum GlobalEditableFields
        {
            Unapproved_EOT,
            Contract_Completion_Date,
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
                        summaryDataPointsTable.Columns.Add(Fields.RowType.ToString(), typeof(StaticSummaryRowTypes));
                        summaryDataPointsTable.Columns.Add(Fields.Design_Budget.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Construction_Budget.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Total_Budget.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.Design_Remaining.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Construction_Remaining.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Total_Remaining.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.Total_Actuals.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.EAC.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.Design_Earned.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Construction_Earned.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.Total_Earned.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.Design_Period_Planned.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Construction_Period_Planned.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.Total_Period_Planned.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.SPI.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.CPI.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Original_Contract_Value.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Approved_Variation.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.Current_Contract_Value.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(Fields.GPM.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(EditableFields.Unapproved_Variation.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(GlobalEditableFields.Unapproved_EOT.ToString(), typeof(decimal));
                        summaryDataPointsTable.Columns.Add(GlobalEditableFields.Contract_Completion_Date.ToString(), typeof(DateTime));
                        summaryDataPointsTable.Columns.Add(Fields.Forecast_Completion_Date.ToString(), typeof(DateTime));
                        summaryDataPointsTable.Columns.Add(Fields.Mask.ToString(), typeof(string));
                        summaryDataPointsTable.Columns.Add(Fields.Construction_TotalBudget_ReadOnly.ToString(), typeof(bool));
                        summaryDataPointsTable.Columns.Add(Fields.Total_Remaining_ReadOnly.ToString(), typeof(bool));
                        summaryDataPointsTable.Columns.Add(Fields.Construction_Budget_ReadOnly.ToString(), typeof(bool));
                        summaryDataPointsTable.Columns.Add(Fields.Construction_Planned_ReadOnly.ToString(), typeof(bool));
                        summaryDataPointsTable.Columns.Add(Fields.Construction_Earned_ReadOnly.ToString(), typeof(bool));
                        summaryDataPointsTable.Columns.Add(Fields.Construction_Remaining_ReadOnly.ToString(), typeof(bool));
                        summaryDataPointsTable.Columns.Add(Fields.Lookup.ToString(), typeof(List<Tuple<string, string>>));
                        summaryDataPointsTable.Columns.Add(Fields.Always_Read_Only.ToString(), typeof(bool));


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
                dataRow[GlobalEditableFields.Unapproved_EOT.ToString()] = loadProject_Summary_Setting.UNAPPROVED_EOT_DAYS == null ? 0 : loadProject_Summary_Setting.UNAPPROVED_EOT_DAYS;
                dataRow[GlobalEditableFields.Contract_Completion_Date.ToString()] = loadProject_Summary_Setting.CONTRACT_COMPLETION_DATE;
            }
        }

        private DataRow findRow(StaticSummaryRowTypes rowType)
        {
            IEnumerable<DataRow> dataRowCollection = from DataRow dr in summaryDataPointsTable.Rows
                                                     select dr;

            return dataRowCollection.FirstOrDefault(x => (StaticSummaryRowTypes)x[Fields.RowType.ToString()] == rowType);
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
            IEnumerable<DataRow> directRows = baseDataRows.Where(x => BluePrintsDataUtils.GetPhaseCode(((ExoSubJobProjection)x[columnEntity]).SubJob.Code).Contains(BluePrintsResources.DirectPhaseCode));
            IEnumerable<DataRow> designRows = baseDataRows.Where(x => BluePrintsDataUtils.GetPhaseCode(((ExoSubJobProjection)x[columnEntity]).SubJob.Code).Contains(BluePrintsResources.DesignPhaseCode));

            IEnumerable<DashboardFlatStructure> indirectDashboards = AllProjectDashboards.Where(x => BluePrintsDataUtils.GetPhaseCode(x.SubjobCode).Contains(BluePrintsResources.IndirectPhaseCode));
            IEnumerable<DashboardFlatStructure> designDashboards = AllProjectDashboards.Where(x => BluePrintsDataUtils.GetPhaseCode(x.SubjobCode).Contains(BluePrintsResources.DesignPhaseCode));
            IEnumerable<DashboardFlatStructure> directDashboards = AllProjectDashboards.Where(x => BluePrintsDataUtils.GetPhaseCode(x.SubjobCode).Contains(BluePrintsResources.DirectPhaseCode));

            IEnumerable<Stats> indirectActualStats = indirectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
            IEnumerable<Stats> directDesignRemainingStats = designDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null).Select(x => x.Stats.Remaining);
            IEnumerable<Stats> directDesignPlannedStats = designDashboards.Where(x => x.Stats != null && x.Stats.Budgeted != null).Select(x => x.Stats.Budgeted);
            IEnumerable<Stats> directDesignEarnedStats = designDashboards.Where(x => x.Stats != null && x.Stats.Earned != null).Select(x => x.Stats.Earned);

            List<Tuple<string, string>> fieldNamesLookup = new List<Tuple<string, string>>();
            fieldNamesLookup.Add(new Tuple<string, string>(EditableFields.Total_Budget.ToString(), Fields.Construction_TotalBudget_ReadOnly.ToString()));
            fieldNamesLookup.Add(new Tuple<string, string>(EditableFields.Construction_Budget.ToString(), Fields.Construction_Budget_ReadOnly.ToString()));
            fieldNamesLookup.Add(new Tuple<string, string>(EditableFields.Construction_Period_Planned.ToString(), Fields.Construction_Planned_ReadOnly.ToString()));
            fieldNamesLookup.Add(new Tuple<string, string>(EditableFields.Construction_Earned.ToString(), Fields.Construction_Earned_ReadOnly.ToString()));
            fieldNamesLookup.Add(new Tuple<string, string>(EditableFields.Construction_Remaining.ToString(), Fields.Construction_Remaining_ReadOnly.ToString()));
            fieldNamesLookup.Add(new Tuple<string, string>(EditableFields.Total_Remaining.ToString(), Fields.Total_Remaining_ReadOnly.ToString()));
            fieldNamesLookup.Add(new Tuple<string, string>(GlobalEditableFields.Unapproved_EOT.ToString(), Fields.Always_Read_Only.ToString()));
            fieldNamesLookup.Add(new Tuple<string, string>(GlobalEditableFields.Contract_Completion_Date.ToString(), Fields.Always_Read_Only.ToString()));
            foreach (EditableFields fields in (EditableFields[])Enum.GetValues(typeof(EditableFields)))
            {
                fieldNamesLookup.Add(new Tuple<string, string>(fields.ToString(), Fields.Always_Read_Only.ToString()));
            }

            newRow[Fields.Lookup.ToString()] = fieldNamesLookup;
            switch (rowType)
            {
                case StaticSummaryRowTypes.Indirect_Man_Hours:
                    filteredDataRows.AddRange(indirectRows);
                    filteredDashboards.AddRange(indirectDashboards);
                    break;
                case StaticSummaryRowTypes.Direct_Man_Hours:
                    filteredDataRows.AddRange(directRows);
                    filteredDataRows.AddRange(designRows);
                    filteredDashboards.AddRange(directDashboards);
                    filteredDashboards.AddRange(designDashboards);
                    break;
                default:
                    filteredDataRows.AddRange(indirectRows);
                    filteredDataRows.AddRange(directRows);
                    filteredDataRows.AddRange(designRows);
                    filteredDashboards.AddRange(indirectDashboards);
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

            if (rowType == StaticSummaryRowTypes.Costs)
            {
                //designBudget = directDesignPlannedStats.Where(x => x.DataPoints != null).Sum(x => x.DataPoints.Sum(y => y.BudgetedCosts));
                designBudget = null;
                constructionBudget = null;
                totalBudget = filteredForecastCalculaton.Sum(x => x.Budget);
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
                actual = indirectActualStats.Sum(x => x.ExoDataPoints.Sum(y => y.Units));
                if (rowType == StaticSummaryRowTypes.Indirect_Man_Hours)
                {
                    IEnumerable<TASK> indirectTASKS = TASKS(BluePrintsResources.P6_Procurement_ACTVCODE);
                    constructionBudget = null;
                    constructionRemaining = null;
                    constructionPlanned = null;
                    constructionEarned = null;
                    totalEarned = actual;

                    if(LoadP6PROJECT != null)
                    {
                        totalBudget = indirectTASKS.Where(x => x.target_work_qty != null).Sum(x => (decimal)x.target_work_qty);
                        totalPlanned = getPeriodCumulativePlanned(indirectTASKS);
                        totalRemaining = indirectTASKS.Where(x => x.remain_work_qty != null).Sum(x => (decimal)x.remain_work_qty);
                    }
                    else
                    {
                        totalBudgetReadOnly = false;
                        totalRemainingReadOnly = false;
                        if (PROJECT_SUMMARY != null)
                        {
                            totalBudget = PROJECT_SUMMARY.BUDGET_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.BUDGET_UNITS;
                            totalPlanned = PROJECT_SUMMARY.PLANNED_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.PLANNED_UNITS;
                            totalRemaining = PROJECT_SUMMARY.FORECAST_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.FORECAST_UNITS;
                        }
                    }
                }
                else
                {
                    designBudget = directDesignPlannedStats.Where(x => x.DataPoints != null).Sum(x => x.DataPoints.Sum(y => y.BudgetedUnits));
                    designRemaining = directDesignRemainingStats.Where(x => x.DataPoints != null).Sum(x => x.DataPoints.Sum(y => y.Units));
                    designEarned = directDesignEarnedStats.Where(x => x.CurrentPeriodCumulativeDataPoint != null).Sum(x => x.CurrentPeriodCumulativeDataPoint.Units);
                    designPlanned = directDesignPlannedStats.Where(x => x.CurrentPeriodCumulativeDataPoint != null).Sum(x => x.CurrentPeriodCumulativeDataPoint.Units);

                    if(LoadP6PROJECT != null)
                    {
                        IEnumerable<TASK> directTASKS = TASKS(BluePrintsResources.P6_Construction_ACTVCODE);
                        constructionBudget = directTASKS.Where(x => x.target_work_qty != null).Sum(x => (decimal)x.target_work_qty);
                        constructionRemaining = directTASKS.Where(x => x.remain_work_qty != null).Sum(x => (decimal)x.remain_work_qty);
                        constructionEarned = directTASKS.Where(x => x.act_work_qty != null).Sum(x => (decimal)x.act_work_qty);
                        constructionPlanned = getPeriodCumulativePlanned(directTASKS);
                    }
                    else
                    {
                        constructionBudgetReadOnly = false;
                        constructionRemainingReadOnly = false;
                        constructionEarnedReadOnly = false;
                        constructionPlannedReadOnly = false;
                        if (PROJECT_SUMMARY != null)
                        {
                            constructionBudget = PROJECT_SUMMARY.BUDGET_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.BUDGET_UNITS;
                            constructionRemaining = PROJECT_SUMMARY.FORECAST_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.FORECAST_UNITS;
                            constructionEarned = PROJECT_SUMMARY.EARNED_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.EARNED_UNITS;
                            constructionPlanned = PROJECT_SUMMARY.PLANNED_UNITS == null ? 0 : (decimal)PROJECT_SUMMARY.PLANNED_UNITS;
                        }
                    }


                    totalBudget = (decimal)designBudget + (decimal)constructionBudget;
                    totalRemaining = (decimal)designRemaining + (decimal)constructionRemaining;
                    totalEarned = (decimal)designEarned + (decimal)constructionEarned;
                    totalPlanned = (decimal)designPlanned + (decimal)constructionPlanned;
                }
            }

            if (PROJECT_SUMMARY != null)
            {
                originalContractValue = PROJECT_SUMMARY.ORI_CONTRACT == null ? 0 : (decimal)PROJECT_SUMMARY.ORI_CONTRACT;
                approvedVariations = PROJECT_SUMMARY.APPROVED_VAR == null ? 0 : (decimal)PROJECT_SUMMARY.APPROVED_VAR;

                newRow[EditableFields.Original_Contract_Value.ToString()] = originalContractValue;
                newRow[EditableFields.Approved_Variation.ToString()] = approvedVariations;
                newRow[EditableFields.Unapproved_Variation.ToString()] = PROJECT_SUMMARY.UNAPPROVED_VAR == null ? 0 : PROJECT_SUMMARY.UNAPPROVED_VAR;
            }

            decimal forecast = actual + totalRemaining;
            decimal currentContractValue = originalContractValue + approvedVariations;

            newRow[Fields.RowType.ToString()] = rowType;
            if (designBudget != null) newRow[Fields.Design_Budget.ToString()] = designBudget;
            if (constructionBudget != null) newRow[EditableFields.Construction_Budget.ToString()] = constructionBudget;
            newRow[EditableFields.Total_Budget.ToString()] = totalBudget;
            if (designRemaining != null) newRow[Fields.Design_Remaining.ToString()] = designRemaining;
            if (constructionRemaining != null) newRow[EditableFields.Construction_Remaining.ToString()] = constructionRemaining;
            newRow[EditableFields.Total_Remaining.ToString()] = totalRemaining;
            newRow[Fields.EAC.ToString()] = forecast;
            newRow[Fields.Total_Actuals.ToString()] = actual;
            if (designEarned != null) newRow[Fields.Design_Earned.ToString()] = designEarned;
            if (constructionEarned != null) newRow[EditableFields.Construction_Earned.ToString()] = constructionEarned;
            newRow[Fields.Total_Earned.ToString()] = totalEarned;
            if (designPlanned != null) newRow[Fields.Design_Period_Planned.ToString()] = designPlanned;
            if (constructionPlanned != null) newRow[EditableFields.Construction_Period_Planned.ToString()] = constructionPlanned;
            newRow[Fields.Total_Period_Planned.ToString()] = totalPlanned;
            IEnumerable<BluePrints.Common.ViewModel.Reporting.DataPoint> remainingDataPoints = filteredDashboards.Where(x => x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints);
            DateTime lastForecastDate = FORECASTCollection.Count() == 0 ? DateTime.Now : FORECASTCollection.Max(x => x.FORECAST_DATE);
            DateTime lastRemainingDate = remainingDataPoints.Count() == 0 ? DateTime.Now : remainingDataPoints.Max(x => x.ProgressDate);
            newRow[Fields.Forecast_Completion_Date.ToString()] = lastForecastDate < lastRemainingDate ? lastRemainingDate : lastForecastDate;
            newRow[Fields.Mask.ToString()] = rowType == StaticSummaryRowTypes.Costs ? "c0" : "n0";

            newRow[Fields.SPI.ToString()] = totalPlanned == 0 ? 0 : totalEarned / totalPlanned;
            newRow[Fields.CPI.ToString()] = actual == 0 ? 0 : totalEarned / actual;
            newRow[Fields.Current_Contract_Value.ToString()] = currentContractValue;
            newRow[Fields.GPM.ToString()] = currentContractValue == 0 ? 0 : (currentContractValue - forecast) / currentContractValue;
            newRow[Fields.Construction_TotalBudget_ReadOnly.ToString()] = totalBudgetReadOnly;
            newRow[Fields.Total_Remaining_ReadOnly.ToString()] = totalRemainingReadOnly;
            newRow[Fields.Construction_Budget_ReadOnly.ToString()] = constructionBudgetReadOnly;
            newRow[Fields.Construction_Planned_ReadOnly.ToString()] = constructionPlannedReadOnly;
            newRow[Fields.Construction_Remaining_ReadOnly.ToString()] = constructionRemainingReadOnly;
            newRow[Fields.Construction_Earned_ReadOnly.ToString()] = constructionEarnedReadOnly;
            newRow[Fields.Always_Read_Only.ToString()] = false;

            if(!isUpdate)
                summaryDataPointsTable.Rows.Add(newRow);
        }
        #endregion

        #region View Events
        protected override void commitCellValue(string fieldName, DataRow row, object oldValue, object newValue)
        {

            if (fieldName == GlobalEditableFields.Unapproved_EOT.ToString())
                UnapprovedEOT = (decimal)newValue;
            else if (fieldName == GlobalEditableFields.Contract_Completion_Date.ToString())
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
            EditableFields editableFieldName = (EditableFields)Enum.Parse(typeof(EditableFields), fieldName);
            switch (editableFieldName)
            {
                case EditableFields.Original_Contract_Value:
                    project_summary.ORI_CONTRACT = (decimal)newValue;
                    break;
                case EditableFields.Approved_Variation:
                    project_summary.APPROVED_VAR = (decimal)newValue;
                    break;
                case EditableFields.Unapproved_Variation:
                    project_summary.UNAPPROVED_VAR = (decimal)newValue;
                    break;
                case EditableFields.Construction_Budget:
                    project_summary.BUDGET_UNITS = (decimal)newValue;
                    break;
                case EditableFields.Construction_Earned:
                    project_summary.EARNED_UNITS = (decimal)newValue;
                    break;
                case EditableFields.Construction_Period_Planned:
                    project_summary.PLANNED_UNITS = (decimal)newValue;
                    break;
                case EditableFields.Construction_Remaining:
                    project_summary.FORECAST_UNITS = (decimal)newValue;
                    break;
                case EditableFields.Total_Budget:
                    project_summary.BUDGET_UNITS = (decimal)newValue;
                    break;
                case EditableFields.Total_Remaining:
                    project_summary.FORECAST_UNITS = (decimal)newValue;
                    break;
            }
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
            StaticSummaryRowTypes rowType = (StaticSummaryRowTypes)row[Fields.RowType.ToString()];
            return rowType;
        }

        public override void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                if (entityProperty.PropertyName == GlobalEditableFields.Unapproved_EOT.ToString())
                    UnapprovedEOT = (decimal?)entityProperty.OldValue;
                else if (entityProperty.PropertyName == GlobalEditableFields.Contract_Completion_Date.ToString())
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
                if (entityProperty.PropertyName == GlobalEditableFields.Unapproved_EOT.ToString())
                    UnapprovedEOT = (decimal?)entityProperty.NewValue;
                else if (entityProperty.PropertyName == GlobalEditableFields.Contract_Completion_Date.ToString())
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
        #endregion
    }
}