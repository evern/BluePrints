using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BluePrints.Reports;
using System.IO;
using BluePrints.Common.Reports;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Resources;
using BaseModel.ViewModel.Services;
using DevExpress.Mvvm.DataAnnotations;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Xpf.Core.ConditionalFormatting;
using System.Data;
using System.Windows.Media;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Editors;
using DevExpress.Utils.Filtering;
using System.ComponentModel.DataAnnotations;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Docking.Base;
using BaseModel.ViewModel.UndoRedo;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Timers;
using DevExpress.Xpf.Spreadsheet;
using DevExpress.Spreadsheet;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTForecastViewModelWrapper : PROJECTViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public new static PROJECTForecastViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTForecastViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTForecastViewModelWrapper()
        {

        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECASTS, FORECASTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_REGISTERS, VARIATION_REGISTERProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
        }

        private Func<IRepositoryQuery<FORECAST>, IQueryable<FORECAST>> FORECASTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<VARIATION_REGISTER>, IQueryable<VARIATION_REGISTER>> VARIATION_REGISTERProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        public bool IsLoadingForecast { get; set; }
        public bool IsHidden { get; set; }
        public ForecastSummary ForecastSummary { get; set; }
        public CriteriaOperator FilterCriteria { get; set; }
        public virtual DateTime EndSelectionDate { get; set; }
        public virtual DateTime StartSelectionDate { get; set; }
        public virtual IEnumerable<string> Subjobs { get; set; }
        protected IEnumerable<ExoTimeAuthorisation> jobLines { get; set; }
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IEnumerable<ExoSubJobProjection> exoSubJobs;
        List<string> hiddenColumnFieldNames = new List<string>();
        List<DateTime> alignedDataDateCollection;
        protected virtual IGridControlService DetailGridControlService { get { return this.GetService<IGridControlService>("DetailGridControlService"); } }
        protected virtual IGridControlService ExportGridControlService { get { return this.GetService<IGridControlService>("ExportGridControlService"); } }
        protected virtual ITableViewService ExportTableViewService { get { return this.GetService<ITableViewService>("ExportTableViewService"); } }

        protected int spreadSheetPhaseIndex = 0;
        protected int spreadSheetAreaIndex = 1;
        protected int spreadSheetSubAreaIndex = 2;
        protected int spreadSheetSubJobIndex = 3;
        protected int spreadSheetSubJobTitleIndex = 4;
        protected int spreadSheetVariationIndex = 5;
        protected int spreadSheetDisciplineIndex = 6;
        protected int spreadSheetDisciplineNameIndex = 7;
        protected int spreadSheetCommodityIndex = 8;
        protected int spreadSheetCommodityNameIndex = 9;
        protected int spreadSheetCommodityDescriptionIndex = 10;
        protected int spreadSheetCommodityUOMIndex = 11;
        protected int spreadSheetRateIndex = 12;
        protected int spreadSheetBudgetIndex = 13;
        protected int spreadSheetDateStartIndex = 14;

        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            ForecastSummary = new ForecastSummary();
            forceRetrieveAllBurned = true; //force exo burned to retrieve subjobs that aren't defined
            useProductivityFactorOnRemaining = true; //calculate remaining costs using productivity factor
            IsLoadingForecast = true;
            LoadingScreenManager.DisableLoadingScreen = false;
            shouldSeparateVariation = true;
            skipBindingSwitch = true;
            hiddenColumnFieldNames.Add(columnEntity);
            hiddenColumnFieldNames.Add(columnCalculation);
            hiddenColumnFieldNames.Add(columnCompare);
            hiddenColumnFieldNames.Add(columnChild);
            initializeSummaryStats();
            SelectedDataRows = new ObservableCollection<DataRowView>();
            StartSelectionDate = DateTime.Now;
            DetailedData = new List<ExoDataPoint>();
            alignedDataDateCollection = new List<DateTime>();
            IsHidden = true;
            delayPostLoadedTimer = true;
            //isExcelExportDataAware = false;
            masterJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            copyLine = ExoQueries.GetMasterProjectLineByJobNumber(primeroUnitOfWork, loadPROJECT.NUMBER);
        }

        private void initializeSummaryStats()
        {
            jobLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
            exoSubJobs = ExoQueries.GetNativeExoSubJobProjection(primeroUnitOfWork, loadPROJECT);
            dynamic revenueLine = ExoQueries.GetProjectRevenue(primeroUnitOfWork, loadPROJECT.NUMBER);
            if (revenueLine != null)
                ForecastSummary.Revenue = Convert.ToDecimal(revenueLine.BUDGETED_REV);

            ForecastSummary.TotalClaims = ExoQueries.GetProjectClaims(primeroUnitOfWork, loadPROJECT.NUMBER);
        }

        public override DateTime? FixedStartDate
        {
            get
            {
                //do this to prevent binding errors
                if (liveDesignProgress == null || loadPROJECT == null)
                    return DateTime.Now;

                return loadPROJECT.FORECAST_START_DATE == null ? liveDesignProgress.PROGRESS_START : (DateTime)loadPROJECT.FORECAST_START_DATE;
            }
            set
            {
                if (isCompletelyLoaded)
                {
                    loadPROJECT.FORECAST_START_DATE = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    this.RaisePropertyChanged(x => x.FixedStartDate);
                }
            }
        }

        public override DateTime? FixedDataDate
        {
            get
            {
                //do this to prevent binding errors
                if (liveDesignProgress == null || loadPROJECT == null)
                    return DateTime.Now;

                return loadPROJECT.FORECAST_DATA_DATE == null ? liveDesignProgress.DATA_DATE : (DateTime)loadPROJECT.FORECAST_DATA_DATE;
            }
            set
            {
                if (isCompletelyLoaded)
                {
                    loadPROJECT.FORECAST_DATA_DATE = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    this.RaisePropertyChanged(x => x.FixedDataDate);
                }
            }
        }

        public DateTime FixedEndDate
        {
            get
            {
                //do this to prevent binding errors
                if (liveDesignProgress == null || loadPROJECT == null)
                    return DateTime.Now;

                return loadPROJECT.FORECAST_END_DATE == null ? DateTime.Now : (DateTime)loadPROJECT.FORECAST_END_DATE;
            }
            set
            {
                if (isCompletelyLoaded)
                {
                    loadPROJECT.FORECAST_END_DATE = value;
                    PROJECTCollectionViewModel.Save(loadPROJECT);
                    this.RaisePropertyChanged(x => x.FixedDataDate);
                }
            }
        }

        protected override void onSummaryCalculateComplete()
        {
            alignedDataDateCollection.Clear();
            DetailedData.Clear();
            EntitiesUndoRedoManager.Clear();
            if(MainViewModel != null)
            {
                MainViewModel.IsPasteCellLevel = true;
                this.RaisePropertyChanged(x => x.MainViewModel.IsPasteCellLevel);
            }

            FORECASTCollectionViewModel.SetParentViewModel(this);
            VARIATION_REGISTERCollectionViewModel.SetParentViewModel(this);
            this.RaisePropertyChanged(x => x.DataPointsTable);
            LoadingScreenManager.DisableLoadingScreen = false;
            IsLoadingForecast = false;
            this.RaisePropertyChanged(x => x.IsLoadingForecast);

            post_loaded_dispatcher_timer = new Timer();
            post_loaded_dispatcher_timer.Interval = 1500;
            post_loaded_dispatcher_timer.Elapsed += post_loaded_dispatcher_timer_tick;
            post_loaded_dispatcher_timer.Start();

            base.onSummaryCalculateComplete();
        }

        public override void FullRefresh()
        {
            dataPointsTable = null;
            ForecastSummary.Reset();
            initializeSummaryStats();
            base.FullRefresh();
        }

        #region Data Points Table
        protected string columnEntity = "Entity";
        protected string columnCalculation = "Calculation";
        protected string columnCompare = "CompareEntities";
        protected string columnChild = "ChildEntities";
        DataTable dataPointsTable = null;
        DateTime firstAlignedDataDate;

        DataTable exportTable = null;
        public DataTable ExportTable
        {
            get
            {
                if (DataPointsTable == null)
                    return null;

                if(exportTable == null)
                {
                    exportTable = new DataTable();
                    List<ExoSubJobProjection> commoditySubJobs = new List<ExoSubJobProjection>();
                    exportTable.Columns.Add(columnEntity, typeof(ExoSubJobProjection));
                    exportTable.Columns.Add(columnCalculation, typeof(ForecastCalculation));
                    List<string> addedDates = new List<string>();
                    foreach (DateTime alignedDataDate in alignedDataDateCollection.Where(x => x.Date >= FixedDataDate))
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        addedDates.Add(columnFieldName);
                        exportTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach (DataRow row in dataPointsTable.Rows)
                    {
                        DataTable childTable = (DataTable)row[columnChild];
                        foreach (DataRow childRow in childTable.Rows)
                        {
                            DataRow exportRow = exportTable.NewRow();
                            exportRow[columnEntity] = (ExoSubJobProjection)childRow[columnEntity];
                            ForecastCalculation forecastCalculation = (ForecastCalculation)childRow[columnCalculation];
                            exportRow[columnCalculation] = forecastCalculation;

                            foreach(string addedDate in addedDates)
                            {
                                object dataPointsTableObj = childRow[addedDate];
                                if(forecastCalculation.Rate != 0)
                                {
                                    if (dataPointsTableObj != null)
                                    {
                                        decimal cost = 0;
                                        if (decimal.TryParse(dataPointsTableObj.ToString(), out cost))
                                        {
                                            exportRow[addedDate] = cost / forecastCalculation.Rate;
                                            continue;
                                        }
                                    }
                                }

                                exportRow[addedDate] = 0;
                            }

                            exportTable.Rows.Add(exportRow);
                        }
                    }
                }

                return exportTable;
            }
        }

        public virtual DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || AllProjectDashboards == null)
                    return null;

                if (dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    firstAlignedDataDate = ChronologicalHelpers.RewindDataDate((DateTime)FixedStartDate, (DateTime)FixedDataDate, new TimeSpan(7, 0, 0, 0));

                    IEnumerable<Stats> actualStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
                    IEnumerable<Stats> materialStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
                    IEnumerable<Stats> poStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);

                    List<ExoSubJobProjection> combinedSubJobs = new List<ExoSubJobProjection>();
                    foreach (ExoSubJobProjection exoSubJob in exoSubJobs)
                    {
                        addExoSubJob(combinedSubJobs, exoSubJob.SubJob.Code, exoSubJob.Discipline.Code, exoSubJob.Commodity.Code, exoSubJob.Variation_Code, exoSubJob.SubJob.Title, exoSubJob.Discipline.Name);
                    }

                    List<ExoDataPoint> allData = new List<ExoDataPoint>();
                    DetailedData.AddRange(actualStats.SelectMany(x => x.ExoDataPoints));
                    DetailedData.AddRange(materialStats.SelectMany(x => x.ExoDataPoints));
                    DetailedData.AddRange(poStats.SelectMany(x => x.ExoDataPoints));
                    allData.AddRange(DetailedData);
                    //allData.AddRange(poStats.SelectMany(x => x.ExoDataPoints));

                    List<string> uniqueSubjobs = allData.Select(x => x.Subjob_Name + ";" + x.Discipline_Code + ";" + x.Commodity_Code + ";" + x.Variation_Code).Distinct().ToList();
                    foreach(string uniqueSubjob in uniqueSubjobs)
                    {
                        List<string> delimited = uniqueSubjob.Split(';').ToList();
                        string subjobCode = delimited[0];
                        string disciplineCode = delimited[1];
                        string commodityCode = delimited[2];
                        string variationCode = delimited[3];

                        addExoSubJob(combinedSubJobs, subjobCode, disciplineCode, commodityCode, variationCode);
                    }

                    //lastDataDate = lastDataDate.AddDays(10 * interval.Days);
                    alignedDataDateCollection = ChronologicalHelpers.GenerateMonthEndDatesCollection(firstAlignedDataDate, (DateTime)FixedEndDate);
                    dataPointsTable.Columns.Add(columnEntity, typeof(ExoSubJobProjection));
                    dataPointsTable.Columns.Add(columnCalculation, typeof(ForecastCalculation));
                    dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));
                    dataPointsTable.Columns.Add(columnChild, typeof(DataTable));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    var groupedSubjobs = combinedSubJobs.GroupBy(x => x.SubJob.Code + x.Discipline.Code + x.Variation_Code).Select(group => new { ProgressDate = group.Key, Projection = group.ToList() });
                    foreach (var groupedSubjob in groupedSubjobs)
                    {
                        DataRow dataRow = buildDisciplineRowStats(groupedSubjob.Projection);

                        DataTable compareDataTable = new DataTable();
                        compareDataTable = dataPointsTable.Clone();

                        DataRow cloneRow = compareDataTable.NewRow();
                        setDateFieldsEmpty(cloneRow, false);

                        updateForecast(cloneRow, groupedSubjob.Projection.First(), true);
                        compareDataTable.Rows.Add(cloneRow);
                        dataRow[columnCompare] = compareDataTable;
                    }

                    this.RaisePropertyChanged(x => x.ForecastSummary);
                    this.RaisePropertyChanged(x => x.ExportTable);
                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }
        
        private void addExoSubJob(List<ExoSubJobProjection> combinedSubJobs, string subJobCode, string disciplineCode, string commodityCode, string variationCode,
            string subJobTitle = "", string disciplineName = "")
        {
            COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.CODE == commodityCode);
            string commodityCodeName = string.Empty;
            string commodityCodeDescription = string.Empty;
            string commodityCodeUOM = string.Empty;
            if (findCOMMODITY_CODE != null)
            {
                commodityCodeName = findCOMMODITY_CODE.NAME;
                commodityCodeDescription = findCOMMODITY_CODE.DESCRIPTION;
                commodityCodeUOM = findCOMMODITY_CODE.UOM;
            }

            if(subJobTitle == string.Empty)
            {
                ExoSubJobProjection findSubJobProjection = exoSubJobs == null ? null : exoSubJobs.FirstOrDefault(x => x.SubJob.Code == subJobCode);
                if (findSubJobProjection != null)
                    subJobTitle = findSubJobProjection.SubJob.Title;
            }

            if(disciplineName == string.Empty)
            {
                ExoSubJobProjection findDisciplineProjection = exoSubJobs == null ? null : exoSubJobs.FirstOrDefault(x => x.Discipline.Code == disciplineCode);
                if (findDisciplineProjection != null)
                    disciplineName = findDisciplineProjection.Discipline.Name;
            }

            if (!combinedSubJobs.Any(x => x.SubJob.Code == subJobCode && x.Discipline.Code == disciplineCode && x.Commodity.Code == commodityCode && x.Variation_Code == variationCode))
            {
                combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityCodeName, Description = commodityCodeDescription, UOM = commodityCodeUOM }, Variation_Code = normalizeVariationCode(variationCode) });
            }
        }

        /// <summary>
        /// For the purpose of presentation, variation code must always be empty
        /// But when budget is edited, findExistingOrAddNewLine will handle the difference between null and string.empty values
        /// </summary>
        private string normalizeVariationCode(string subjobVariationCode)
        {
            if (subjobVariationCode == null)
                return string.Empty;

            return subjobVariationCode;
        }

        private void setDateFieldsEmpty(DataRow dataRow, bool test)
        {
            for (int i = 0; i < dataRow.ItemArray.Count(); i++)
            {
                string columnName = dataPointsTable.Columns[i].ColumnName;
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                    dataRow[columnName] = test ? 1000m : 0.00m;
            }
        }

        public List<ExoDataPoint> DetailedData { get; set; }
        private void updateDataRowForecast(ExoSubJobProjection disciplineEntity)
        {
            DataRow dataRow = findDataRow(disciplineEntity.SubJob.Code, disciplineEntity.Discipline.Code, disciplineEntity.Commodity.Code, disciplineEntity.Variation_Code);
            if (dataRow == null)
                return;

            updateForecast(dataRow, disciplineEntity, false);
        }

        private DataRow findDataRow(string subJobCode, string disciplineCode, string commodityCode, string variationCode)
        {
            if (commodityCode == string.Empty)
            {
                return (from DataRow dr in dataPointsTable.Rows
                           where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == subJobCode && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == disciplineCode && ((ExoSubJobProjection)dr[columnEntity]).Variation_Code == variationCode
                        select dr).FirstOrDefault();
            }
            else
            {
                IEnumerable<DataTable> childTables = from DataRow dr in dataPointsTable.Rows
                                                     select (DataTable)dr[columnChild];

                IEnumerable<DataRow> childRowsCollection = childTables.SelectMany(x => x.Rows.Cast<DataRow>().ToArray());
                return (from DataRow dr in childRowsCollection
                        where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == subJobCode && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == disciplineCode && ((ExoSubJobProjection)dr[columnEntity]).Commodity.Code == commodityCode && ((ExoSubJobProjection)dr[columnEntity]).Variation_Code == variationCode
                        select dr).FirstOrDefault();
            }
        }

        private DataRow buildDisciplineRowStats(IEnumerable<ExoSubJobProjection> groupedSubJobs)
        {
            if (dataPointsTable == null)
                return null;

            if (groupedSubJobs == null || groupedSubJobs.Count() == 0)
                return null;

            ExoSubJobProjection firstSubJob = groupedSubJobs.First();
            DataRow disciplineDataRow = dataPointsTable.NewRow();
            initializeDataRow(disciplineDataRow, firstSubJob.SubJob.Code, firstSubJob.SubJob.Title, firstSubJob.Discipline.Code, firstSubJob.Discipline.Name, string.Empty, string.Empty, string.Empty, string.Empty, firstSubJob.Variation_Code);

            DataTable childDataTable = new DataTable();
            childDataTable = dataPointsTable.Clone();
            foreach (var groupedSubjob in groupedSubJobs)
            {
                DataRow cloneRow = childDataTable.NewRow();
                initializeDataRow(cloneRow, groupedSubjob.SubJob.Code, groupedSubjob.SubJob.Title, groupedSubjob.Discipline.Code, groupedSubjob.Discipline.Name, groupedSubjob.Commodity.Code, groupedSubjob.Commodity.Name, groupedSubjob.Commodity.Description, groupedSubjob.Commodity.UOM, groupedSubjob.Variation_Code);
                IEnumerable<DashboardFlatStructure> commodityDashboards = AllProjectDashboards.Where(x => x.SubjobCode == groupedSubjob.SubJob.Code && x.DisciplineCode == groupedSubjob.Discipline.Code && x.CommodityCode == groupedSubjob.Commodity.Code && x.Variation_Code == groupedSubjob.Variation_Code);

                populateDataRow(cloneRow, commodityDashboards);
                updateForecast(cloneRow, groupedSubjob, false);
                calculateUncommitted(cloneRow);
                childDataTable.Rows.Add(cloneRow);
            }

            disciplineDataRow[columnChild] = childDataTable;
            IEnumerable<DashboardFlatStructure> disciplineDashboards = AllProjectDashboards.Where(x => x.SubjobCode == firstSubJob.SubJob.Code && x.DisciplineCode == firstSubJob.Discipline.Code && x.Variation_Code == firstSubJob.Variation_Code);
            populateDataRow(disciplineDataRow, disciplineDashboards);
            ForecastCalculation forecastCalculation = (ForecastCalculation)disciplineDataRow[columnCalculation];
            ForecastSummary.Actuals += forecastCalculation.Actuals;
            ForecastSummary.Commitments += forecastCalculation.Outstanding;
            ForecastSummary.EstimateAtCompletion += forecastCalculation.EstimateAtCompletion;

            //effectively override remaining
            updateForecast(disciplineDataRow, (ExoSubJobProjection)disciplineDataRow[columnEntity], false);
            calculateUncommitted(disciplineDataRow);
            dataPointsTable.Rows.Add(disciplineDataRow);

            return disciplineDataRow;
        }

        private void initializeDataRow(DataRow dataRow, string subJobCode, string subJobTitle, string disciplineCode, string disciplineName, string commodityCode, string commodityName, string commodityDescription, string commodityUOM, string variationCode)
        {
            variationCode = normalizeVariationCode(variationCode);

            ExoSubJobProjection entity = new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityName, Description = commodityDescription, UOM = commodityUOM }, Variation_Code = variationCode };
            ForecastCalculation calculation = new ForecastCalculation();
            dataRow[columnEntity] = entity;
            dataRow[columnCalculation] = calculation;

            IEnumerable<ExoTimeAuthorisation> relevantJobLines;
            if (entity.Commodity.Code == string.Empty)
            {
                calculation.IsBudgetReadOnly = true;
                relevantJobLines = jobLines.Where(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.VariationCode == entity.Variation_Code);
            }
            else
                relevantJobLines = jobLines.Where(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code && x.VariationCode == entity.Variation_Code);

            ExoTimeAuthorisation revenueJobLine = jobLines.FirstOrDefault(x => x.SubJobCode == entity.SubJob.Code && x.StockCode == BluePrintsResources.Default_Revenue_StockCode);
            entity.ExoBudgetQty = relevantJobLines.Sum(x => x.BudgetQty);
            entity.ExoBudgetCosts = relevantJobLines.Sum(x => x.BudgetCosts);
            entity.ExoForecastRate = relevantJobLines.Sum(x => x.ForecastRate);

            if (entity.Commodity.Code == string.Empty)
            {
                ForecastSummary.Costs += entity.ExoBudgetCosts;
            }

            ////populate revenue
            //ExoTimeAuthorisation revenueLine = jobLines.FirstOrDefault(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.StockCode == BluePrintsResources.Default_Revenue_StockCode);

            calculation.Budget = entity.ExoBudgetCosts;
            calculation.Rate = entity.ExoForecastRate;

            //variation is only calculated on discipline code lines
            if (commodityCode == string.Empty)
                calculation.Variation = VARIATION_REGISTERCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.STATUS == VariationRegisterStatus.Approved).Sum(x => x.COST);
            else
                calculation.Variation = 0.00m;

            //populate previous estimate to completion
            IEnumerable<FORECAST> previousEAC = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && x.IS_EAC && x.FORECAST_DATE < FixedDataDate).OrderBy(x => x.FORECAST_DATE);
            if (previousEAC.Count() > 0)
            {
                FORECAST lastEAC = previousEAC.Last();
                if (lastEAC.FORECAST_UNITS != null)
                    calculation.PreviousEAC = (decimal)lastEAC.FORECAST_UNITS;
            }

            setDateFieldsEmpty(dataRow, false);
        }

        /// <summary>
        /// Populates data row with dashboards summary
        /// </summary>
        private void populateDataRow(DataRow dataRow, IEnumerable<DashboardFlatStructure> relevantDashboards)
        {
            ExoSubJobProjection entity = (ExoSubJobProjection)dataRow[columnEntity];
            ForecastCalculation forecastCalculation = (ForecastCalculation)dataRow[columnCalculation];
            if (relevantDashboards != null && relevantDashboards.Count() > 0)
            {
                IEnumerable<SummaryStats> summaryStats = relevantDashboards.Select(x => (SummaryStats)x.Stats);
                IEnumerable<SummaryStats> poStats = summaryStats.Where(x => x.PO != null && x.PO.DataPoints != null);

                if (poStats != null && poStats.Count() > 0)
                {
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> poDataPoints = poStats.SelectMany(x => x.PO.DataPoints);
                    forecastCalculation.Outstanding = poDataPoints.Sum(x => x.Costs);
                }

                IEnumerable<SummaryStats> remainingStats = summaryStats.Where(x => x.Remaining != null && x.Remaining.DataPoints != null);
                if (remainingStats != null && remainingStats.Count() > 0)
                {
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = remainingStats.SelectMany(x => x.Remaining.DataPoints);
                    foreach (DateTime alignedDate in alignedDataDateCollection)
                    {
                        string alignedDateField = ((DateTime)alignedDate).ToShortDateString();
                        if (dataPointsTable.Columns.Contains(alignedDateField))
                        {
                            decimal currentValue = remainingDataPoints.Where(x => x.ProgressDate.Month == alignedDate.Month && x.ProgressDate.Year == alignedDate.Year).Sum(x => x.Costs);
                            if (currentValue != 0)
                            {
                                //decimal currentRowValue = (decimal)dataRow[alignedDateField];
                                dataRow[alignedDateField] = currentValue;
                            }
                        }
                    }
                }

                IEnumerable<SummaryStats> actualStats = summaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null);
                if (actualStats != null && actualStats.Count() > 0)
                {
                    IEnumerable<ExoDataPoint> actualDataPoints = actualStats.SelectMany(x => x.Actual.ExoDataPoints);
                    forecastCalculation.Actuals += actualDataPoints.Sum(x => x.Costs);
                    forecastCalculation.Invoiced += actualDataPoints.Sum(x => x.InvoiceAmount);

                    foreach (DateTime alignedDate in alignedDataDateCollection)
                    {
                        string alignedDateField = ((DateTime)alignedDate).ToShortDateString();
                        if (dataPointsTable.Columns.Contains(alignedDateField))
                        {
                            decimal currentValue = actualDataPoints.Where(x => x.ActualDate.Month == alignedDate.Month && x.ActualDate.Year == alignedDate.Year).Sum(x => x.Costs);
                            if (currentValue != 0)
                            {
                                //decimal currentRowValue = (decimal)dataRow[alignedDateField];
                                dataRow[alignedDateField] = currentValue;
                            }
                        }
                    }
                }

                IEnumerable<SummaryStats> materialStats = summaryStats.Where(x => x.Material != null && x.Material.DataPoints != null);
                if (materialStats != null && materialStats.Count() > 0)
                {
                    IEnumerable<ExoDataPoint> materialDataPoints = materialStats.SelectMany(x => x.Material.ExoDataPoints);
                    forecastCalculation.Actuals += materialDataPoints.Sum(x => x.Costs);
                    forecastCalculation.Invoiced += materialDataPoints.Sum(x => x.InvoiceAmount);

                    foreach (DateTime alignedDate in alignedDataDateCollection)
                    {
                        string alignedDateField = ((DateTime)alignedDate).ToShortDateString();
                        if (dataPointsTable.Columns.Contains(alignedDateField))
                        {
                            decimal currentValue = materialDataPoints.Where(x => x.ActualDate.Month == alignedDate.Month && x.ActualDate.Year == alignedDate.Year).Sum(x => x.Costs);
                            if (currentValue != 0)
                            {
                                decimal currentRowValue = (decimal)dataRow[alignedDateField];
                                dataRow[alignedDateField] = currentRowValue + currentValue;
                            }
                        }
                    }
                }
            }
        }

        private void setForecastCellNull(DataRow updateRow, ExoSubJobProjection entity, string fieldName)
        {
            DateTime dateTime;
            if(DateTime.TryParse(fieldName, out dateTime))
            {
                IEnumerable<DashboardFlatStructure> relevantDashboards;
                if(entity.Commodity.Code == string.Empty)
                    relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                else
                    relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code);

                IEnumerable<Common.ViewModel.Reporting.DataPoint> dataPoints = relevantDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints);
                IEnumerable<Common.ViewModel.Reporting.DataPoint> dateSpecificDataPoints = dataPoints.Where(x => x.ProgressDate.Date == dateTime);

                if(DataPointsTable.Columns.Contains(fieldName))
                {
                    decimal sumCost = dateSpecificDataPoints.Sum(x => x.Costs);
                    if (sumCost == 0)
                        updateRow[fieldName] = 0.0m;
                    else
                        updateRow[fieldName] = sumCost;
                }

                calculateUncommitted(updateRow);
            }
        }

        private void updateForecast(DataRow dataRow, ExoSubJobProjection entity, bool isCompare)
        {
            IEnumerable<FORECAST> currentRowFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && !x.IS_EAC);

            foreach (FORECAST currentRowFORECAST in currentRowFORECASTS)
            {
                string dateField = currentRowFORECAST.FORECAST_DATE.ToShortDateString();
                DateTime parseDate;
                if (DateTime.TryParse(dateField, out parseDate))
                {
                    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= parseDate.Date);
                    if(alignedDataDate != null)
                    {
                        string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                        //put forecast history only on compare datatable
                        if ((alignedDataDate >= FixedDataDate && !isCompare) || (alignedDataDate <= FixedDataDate && isCompare))
                            if (dataPointsTable.Columns.Contains(alignedDateField))
                            {
                                if (currentRowFORECAST.FORECAST_UNITS != null)
                                    dataRow[alignedDateField] = currentRowFORECAST.FORECAST_UNITS;
                            }
                    }
                }
            }
            
            if (isCompare)
            {
                IEnumerable<DashboardFlatStructure> relevantDashboards;
                if(entity.Commodity.Code == string.Empty)
                    relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                else
                    relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code);

                IEnumerable<DashboardFlatStructure> remainingDashboards = relevantDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null);
                if (remainingDashboards != null)
                {
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = remainingDashboards.SelectMany(x => x.Stats.Remaining.DataPoints);
                    var groupByDateDataPoints = remainingDataPoints.GroupBy(x => x.ProgressDate).Select(group => new { ProgressDate = group.Key, DataPoints = group.ToList() });
                    foreach (var groupByDateDataPoint in groupByDateDataPoints)
                    {
                        DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= groupByDateDataPoint.ProgressDate.Date);
                        if (alignedDataDate != null)
                        {
                            string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                            if (dataPointsTable.Columns.Contains(alignedDateField))
                            {
                                decimal currentValue = groupByDateDataPoint.DataPoints.Sum(x => x.Costs);
                                if (currentValue != 0)
                                    dataRow[alignedDateField] = currentValue;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region View Events
        /// <summary>
        /// Because grid alternate between showing editor and focused row, use mousedown to invoke set filter
        /// </summary>
        public void MouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                TableView tableView = (TableView)e.Source;
                TableViewHitInfo hi = ((TableView)e.Source).CalcHitInfo(e.OriginalSource as DependencyObject);
                RowData clickRowData = tableView.FocusedRowData;
                if(clickRowData.Row == null)
                {
                    GridControl masterGrid = tableView.Grid;
                    var selected_cells = Enumerable.Range(0, masterGrid.VisibleRowCount)
                    .Select(x => (GridControl)masterGrid.GetDetail(x))
                    .Where(x => x != null).
                    Select(x => ((TableView)(x).View).FocusedRowData).ToList();

                    clickRowData = selected_cells.FirstOrDefault();
                }

                if(clickRowData != null)
                    setFilter((DataRowView)clickRowData.Row, hi.Column);
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }
        }

        /// <summary>
        /// Because grid alternate between showing editor and focused row, use showing editor to invoke set filter
        /// </summary>
        public void ShowingEditor(DevExpress.Xpf.Grid.ShowingEditorEventArgs e)
        {
            setFilter((DataRowView)e.Row, e.Column);
        }

        public bool IsPOColumnsVisible { get; set; }
        private void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (gridColumn == null || dataRowView == null)
                return;

            if (gridColumn.ReadOnly)
            {
                DateTime parseEndDate;
                if (DateTime.TryParse(gridColumn.ActualColumnChooserHeaderCaption.ToString(), out parseEndDate))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    parseEndDate = parseEndDate.AddDays(1).AddSeconds(-1);
                    EndSelectionDate = parseEndDate;
                    StartSelectionDate = new DateTime(EndSelectionDate.Year, EndSelectionDate.Month, 1);
                    if(parseEndDate == alignedDataDateCollection.First())
                    {
                        if(entity.Commodity.Code != string.Empty)
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'" + " And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                        else
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False'" + " And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    }
                    else
                    {
                        if(entity.Commodity.Code != string.Empty)
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'" + " And [ActualDate] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                        else
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False'" + " And [ActualDate] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    }

                    IsHidden = false;
                    IsPOColumnsVisible = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else if (gridColumn.FieldName.ToUpper().Contains("ACTUALS"))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    if(entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False'");

                    IsHidden = false;
                    IsPOColumnsVisible = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else if (gridColumn.FieldName.ToUpper().Contains("INVOICED"))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    if (entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False' AND [InvoiceAmount] > 0.0m");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'False' AND [InvoiceAmount] > 0.0m");

                    IsHidden = false;
                    IsPOColumnsVisible = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else if(gridColumn.FieldName.ToUpper().Contains("OUTSTANDING"))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    if (entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'True'");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [IsPO] = 'True'");
                    IsHidden = false;

                    IsPOColumnsVisible = true;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                    this.RaisePropertyChanged(x => x.IsPOColumnsVisible);
                }
                else
                {
                    IsHidden = true;
                }
            }
            else
                IsHidden = true;

            this.RaisePropertyChanged(x => x.IsHidden);
        }

        public void HideColumns(AutoGeneratingColumnEventArgs e)
        {
            if (hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                e.Cancel = true;
            }
            else
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    e.Column.MaxWidth = 50;
                }
                else
                {
                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
        }

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                GridControl gridControl = (GridControl)e.Source;
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    if(parsedate < FixedDataDate)
                    {
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                        e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                        e.Column.ReadOnly = true;
                    }
                    else
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;

                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c0");
                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
                else
                {
                    if (e.Column.FieldType == typeof(decimal))
                        GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, e.Column.FieldName + ": {0:c0}");

                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private bool gridSummaryItemExists(GridSummaryItemCollection gridSummaryItems, string fieldName)
        {
            for (int i = 0; i < gridSummaryItems.Count; i++)
            {
                if (gridSummaryItems[i].FieldName == fieldName)
                    return true;
            }

            return false;
        }

        public void AutoGeneratingChildPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    if (parsedate < FixedDataDate)
                    {
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateChild"] as DataTemplate;
                        e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                        e.Column.ReadOnly = true;
                    }
                    else
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateChild"] as DataTemplate;

                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
                else
                {
                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void KeyboardCopy()
        {
            System.Windows.Forms.SendKeys.SendWait("^c");
        }

        public void KeyboardPaste()
        {
            System.Windows.Forms.SendKeys.SendWait("^v");
        }

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();

            //remove tab in front
            if(newValueString != string.Empty)
            {
                if (newValueString.Substring(0, 1) == "\t")
                {
                    newValueString = newValueString.Substring(1, newValueString.Length - 1);
                }

                string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
                pasteCellData(gridControl, gridTableView, RowData);

                GridControlService.RefreshData();
                e.Handled = true;
            }

        }


        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            //EntitiesUndoRedoManager.Clear();
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = gridTableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return;
                else
                {
                    gridTableView = (TableView)selected_cells.First().Column.View;
                    gridControl = gridTableView.Grid;
                }
            }

            List<List<string>> row_data = new List<List<string>>();
            foreach (var row in RowData)
            {
                string formatRow = row;
                //remove tab in front
                if (row.Substring(0, 1) == "\t")
                {
                    formatRow = row.Substring(1, row.Length - 1);
                }

                List<string> column_data = formatRow.Split('\t').ToList();
                row_data.Add(column_data);
            }

            var grouped_results = row_data
            .SelectMany(inner => inner.Select((item, index) => new { item, index }))
            .GroupBy(i => i.index, i => i.item)
            .Select(g => g.ToList())
            .ToList();

            var selected_cells_groupby_columns = selected_cells.GroupBy(x => x.Column.FieldName).Select(group => new { FieldName = group.Key, Cells = group.ToList() });
            if (grouped_results.Count == 0)
            {
                foreach (var selected_cell in selected_cells)
                {
                    int row_handle = selected_cell.RowHandle;
                    DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                    DataRow editing_row = editing_row_view.Row;
                    basePasteData(editing_row, selected_cell.Column, string.Empty, false);
                }
            }
            else
            {
                GridCell first_selected_cell = selected_cells.First();
                GridCell last_selected_cell = selected_cells.Last();

                int first_row_handle = selected_cells.Min(x => x.RowHandle);
                int last_row_handle = selected_cells.Max(x => x.RowHandle);
                int first_row_visible_index = gridControl.GetRowVisibleIndexByHandle(first_row_handle);
                int last_row_visible_index = gridControl.GetRowVisibleIndexByHandle(last_row_handle);
                int numberOfSelectedRows = (last_row_visible_index - first_row_visible_index) + 1;
                int numberOfCopiedRows = grouped_results.First().Count;

                List<GridColumn> visible_columns = gridTableView.VisibleColumns.ToList();
                int first_column_visible_index = visible_columns.First(x => x.FieldName == first_selected_cell.Column.FieldName).VisibleIndex;
                int last_column_visible_index = visible_columns.First(x => x.FieldName == last_selected_cell.Column.FieldName).VisibleIndex;

                int numberOfSelectedColumns = (last_column_visible_index - first_column_visible_index) + 1;
                int numberOfCopiedColumns = grouped_results.Count;

                //commented out because not accurate during banded view
                //int first_column_visible_index = first_selected_cell.Column.VisibleIndex;

                int rowOffsetSelection = numberOfSelectedRows > numberOfCopiedRows ? numberOfSelectedRows : numberOfCopiedRows;
                int columnOffsetSelection = numberOfSelectedColumns > numberOfCopiedColumns ? numberOfSelectedColumns : numberOfCopiedColumns;

                int pasteValueRowOffset = 0;
                for (int rowOffset = 0; rowOffset < rowOffsetSelection; rowOffset++)
                {
                    int pasteValueColumnOffset = 0;
                    for (int columnOffset = 0; columnOffset < columnOffsetSelection; columnOffset++)
                    {
                        if (!visible_columns.Any(x => x.VisibleIndex == (first_column_visible_index + columnOffset)))
                            continue;

                        GridColumn current_column = visible_columns.First(x => x.VisibleIndex == (first_column_visible_index + columnOffset));
                        string columnValue = grouped_results[pasteValueColumnOffset][pasteValueRowOffset];

                        int current_row_visible_index = first_row_visible_index + rowOffset;
                        int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);

                        object rowObject = gridControl.GetRow(current_row_handle);
                        if (rowObject == null)
                            continue;

                        DataRowView editing_row_view = (DataRowView)rowObject;
                        DataRow editing_row = editing_row_view.Row;
                        var gg = from c in editing_row.Table.Columns.Cast<DataColumn>()
                                select c.ColumnName;
                        
                        pasteValueColumnOffset += 1;
                        if (pasteValueColumnOffset >= grouped_results.Count)
                            pasteValueColumnOffset = 0;

                        basePasteData(editing_row, current_column, columnValue, false);
                    }

                    pasteValueRowOffset += 1;
                    if (pasteValueRowOffset >= grouped_results[pasteValueColumnOffset].Count)
                        pasteValueRowOffset = 0;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
        }


        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isNewRow)
        {
            ExoSubJobProjection entity = (ExoSubJobProjection)newRow[columnEntity];
            
            if (copyColumn.FieldType == typeof(decimal))
            {
                var rgx = new Regex("[^0-9a-z\\.]");
                var cleanColumnString = rgx.Replace(pasteData, string.Empty);
                decimal decimal_value;
                if (decimal.TryParse(cleanColumnString, out decimal_value))
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], decimal_value, EntityMessageType.Changed);

                    newRow[copyColumn.FieldName] = decimal_value;
                    DateTime columnDateTime;
                    if(DateTime.TryParse(copyColumn.FieldName, out columnDateTime))
                    {
                        findExistingOrAddNewForecast(newRow, entity, columnDateTime, decimal_value, false);
                    }
                }
                else
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], DBNull.Value, EntityMessageType.Changed);

                    setForecastCellNull(newRow, entity, copyColumn.FieldName);
                    return false;
                }
            }
            else if (copyColumn.FieldType == typeof(string))
            {
                newRow[copyColumn.FieldName] = pasteData;
            }

            return true;
        }

        public void DeleteCellContent(object parameter)
        {
            //EntitiesUndoRedoManager.Clear();
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = tableView.GetSelectedCells();
            if(selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return;
                else
                {
                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            foreach (var selected_cell in selected_cells)
            {
                int row_handle = selected_cell.RowHandle;
                DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                DataRow editing_row = editing_row_view.Row;
                DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.FieldName];
                ExoSubJobProjection entity = (ExoSubJobProjection)editing_row[columnEntity];

                string columnFieldName = selected_cell.Column.FieldName;
                DateTime deleteCellDate;
                if(DateTime.TryParse(columnFieldName, out deleteCellDate))
                {
                    EntitiesUndoRedoManager.AddUndo(editing_row, columnFieldName, editing_row[columnFieldName], null, EntityMessageType.Changed);
                    setForecastCellNull(editing_row, entity, columnFieldName);
                    findExistingOrAddNewForecast(editing_row, entity, deleteCellDate, null, false);
                    //editing_row[columnFieldName] = 0.00m;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            GridControlService.RefreshData();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            // if (changedType == typeof(FORECAST))
            //{
            //    FORECAST changedFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
            //    if (changedFORECAST != null)
            //    {
            //        ExoSubJobProjection findUpdatedEntity = exoSubJobs.FirstOrDefault(x => x.SubJob.Code == changedFORECAST.SUBJOB_CODE && x.Discipline.Code == changedFORECAST.DISCIPLINE_CODE);
            //        if(findUpdatedEntity != null)
            //        {
            //            updateDataRowForecast(findUpdatedEntity);
            //        }
            //    }

            //    mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
            //}

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            DataRowView dataRowView = (DataRowView)e.Row;
            EntitiesUndoRedoManager.PauseActionId();
            commitCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value);
            EntitiesUndoRedoManager.UnpauseActionId();

            GridControlService.RefreshData();
            e.Handled = true;
        }

        protected virtual void commitCellValue(string fieldName, DataRow row, object oldValue, object newValue)
        {
            ExoSubJobProjection entity = (ExoSubJobProjection)row[columnEntity];

            if (fieldName.ToUpper() == "CALCULATION.BUDGET" || fieldName.ToUpper().Contains("ENTITY.RATE"))
            {
                bool isRate = fieldName.ToUpper().Contains("ENTITY.RATE");
                decimal newDecimalValue = 0;
                if (newValue != null && decimal.TryParse(newValue.ToString(), out newDecimalValue))
                {
                    ExoSubJobEditableProjection projection = new ExoSubJobEditableProjection(entity);
                    JOBCOST_LINES findExistingOrAddLine = ExoQueries.GetProjectLine(primeroUnitOfWork, loadPROJECT.NUMBER, projection);
                    bool isError = false;
                    if (isRate)
                        projection.Rate = newDecimalValue;
                    else
                        projection.Budget = newDecimalValue;

                    if (findExistingOrAddLine == null)
                    {
                        if (masterJob == null)
                        {
                            MessageBoxService.ShowMessage("Cannot change budget because the master job is not created for project " + loadPROJECT.NUMBER + " isn't added\nPlease contact " + BluePrintsResources.Default_CFO);
                            isError = true;
                        }
                        else if (copyLine == null)
                        {
                            MessageBoxService.ShowMessage("Cannot change budget because the master line is not created for project " + loadPROJECT.NUMBER + " isn't added\nPlease contact " + BluePrintsResources.Default_CFO);
                            isError = true;
                        }
                        else if (ExoMethods.CommitLineSubJob(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                        {
                            if (ExoMethods.CommitLineDiscipline(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                            {
                                if (ExoMethods.CommitLineCommodity(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, primeroUnitOfWork))
                                {
                                    int? maxJOBCOSTLINEID = ExoQueries.GetJOBCODELINEID(primeroUnitOfWork);
                                    JOBCOST_LINES newLine = ExoMethods.CreateNewLine(copyLine, projection, (int)maxJOBCOSTLINEID);
                                    primeroUnitOfWork.JOBCOST_LINES.Add(newLine);
                                    primeroUnitOfWork.SaveChanges();
                                    entity.LineId = newLine.SEQNO;
                                }
                                else
                                {
                                    MessageBoxService.ShowMessage("Cannot change budget because commodity code " + projection.CommodityCode + " is not added\nPlease contact " + BluePrintsResources.Default_CFO);
                                    isError = true;
                                }
                            }
                            else
                            {
                                MessageBoxService.ShowMessage("Cannot change budget because cost group " + projection.DisciplineCode + " is not added\nPlease contact " + BluePrintsResources.Default_CFO);
                                isError = true;
                            }
                        }
                        else
                        {
                            MessageBoxService.ShowMessage("Cannot change budget because subjob " + projection.SubJobCode + " is not added\nPlease contact " + BluePrintsResources.Default_CFO);
                            isError = true;
                        }

                        if (isError)
                            projection.Budget = 0;
                        else
                        {
                            DataRow disciplineRow = findDisciplineRow(entity);
                            if (disciplineRow != null)
                            {
                                recalculateChildBudget(disciplineRow);
                            }
                        }

                        projection.Update();
                    }
                    else
                    {
                        findExistingOrAddLine.QUOTE_QTY = 1;
                        if(isRate)
                            findExistingOrAddLine.QUOTE_UNITPR = Convert.ToDouble(newDecimalValue);
                        else
                            findExistingOrAddLine.ACTUAL_UNITCOST = Convert.ToDouble(newDecimalValue);

                        primeroUnitOfWork.SaveChanges();

                        DataRow disciplineRow = findDisciplineRow(entity);
                        if (disciplineRow != null)
                        {
                            recalculateChildBudget(disciplineRow);
                        }
                    }
                }
            }
            else
            {
                DateTime dateTime;
                if (DateTime.TryParse(fieldName, out dateTime))
                {
                    decimal? forecastUnits = null;
                    decimal convertUnits = 0;
                    if (newValue != null && decimal.TryParse(newValue.ToString(), out convertUnits))
                        forecastUnits = convertUnits;

                    EntitiesUndoRedoManager.AddUndo(row, fieldName, oldValue, forecastUnits, EntityMessageType.Changed);
                    findExistingOrAddNewForecast(row, entity, dateTime, forecastUnits, false);
                }
            }
        }

        private void recalculateChildBudget(DataRow disciplineRow)
        {
            DataTable childTable = (DataTable)disciplineRow[columnChild];
            decimal totalBudget = 0;
            decimal totalRate = 0;
            for (int i = 0; i < childTable.Rows.Count; i++)
            {
                DataRow childRow = childTable.Rows[i];
                ForecastCalculation childCalculation = (ForecastCalculation)childRow[columnCalculation];
                totalBudget += childCalculation.Budget;
                totalRate += childCalculation.Rate;
            }

            ForecastCalculation calculation = (ForecastCalculation)disciplineRow[columnCalculation];
            calculation.Budget = totalBudget;
            calculation.Rate = totalRate;
        }

        private void findExistingOrAddNewForecast(DataRow dataRow, ExoSubJobProjection entity, DateTime forecastDate, decimal? forecastUnits, bool isRecursive = false)
        {
            decimal? oldValue = 0.00m;
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && !x.IS_EAC);
            if (findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
                newFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
                newFORECAST.FORECAST_DATE = forecastDate.Date;
                newFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(newFORECAST);
            }
            else
            {
                oldValue = findFORECAST.FORECAST_UNITS;
                findFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(findFORECAST);
            }

            //only do this on discipline level so we don't add new forecasted units twice
            if(entity.Commodity.Code == string.Empty)
            {
                decimal? newValue = 0.00m;
                //used to ensure child row is set
                if (forecastUnits != null)
                    newValue = forecastUnits;

                dataRow[forecastDate.ToShortDateString()] = newValue;
                updateEstimateAtComplete(oldValue, newValue);
            }

            if (!isRecursive)
            {
                string dateFieldName = forecastDate.ToShortDateString();
                //need to set child forecast empty
                if (entity.Commodity.Code == string.Empty)
                {
                    DataTable childTable = (DataTable)dataRow[columnChild];
                    foreach (DataRow childRow in childTable.Rows)
                    {
                        ExoSubJobProjection childEntity = (ExoSubJobProjection)childRow[columnEntity];
                        if (childTable.Columns.Contains(dateFieldName))
                        {
                            if(!isBackgroundEdit)
                                EntitiesUndoRedoManager.AddUndo(childRow, dateFieldName, childRow[dateFieldName], 0.00m, EntityMessageType.Changed);

                            findExistingOrAddNewForecast(childRow, childEntity, forecastDate.Date, 0.00m, true);
                            //setForecastCellNull(childRow, (ExoSubJobProjection)childRow[columnEntity], dateFieldName);
                        }
                    }

                    calculateUncommitted(dataRow);
                }
                //set parent forecast empty
                else
                {
                    DataRow disciplineRow = findDisciplineRow(entity);
                    if (disciplineRow != null)
                    {
                        if (dataPointsTable.Columns.Contains(dateFieldName))
                        {
                            decimal cumulativeCosts = 0;
                            DataTable childTable = (DataTable)disciplineRow[columnChild];
                            foreach (DataRow childRow in childTable.Rows)
                            {
                                ExoSubJobProjection childEntity = (ExoSubJobProjection)childRow[columnEntity];
                                decimal childCostOnDate = 0;
                                if(childRow[dateFieldName] != DBNull.Value)
                                    childCostOnDate = (decimal)childRow[dateFieldName];
                                cumulativeCosts += childCostOnDate;
                            }

                            if (!isBackgroundEdit)
                                //only visually represents the costs but stores null in the database
                                EntitiesUndoRedoManager.AddUndo(disciplineRow, dateFieldName, disciplineRow[dateFieldName], cumulativeCosts, EntityMessageType.Changed);

                            findExistingOrAddNewForecast(disciplineRow, (ExoSubJobProjection)disciplineRow[columnEntity], forecastDate.Date, cumulativeCosts, true);
                            disciplineRow[dateFieldName] = cumulativeCosts;
                        }
                    }

                    calculateUncommitted(dataRow);
                    calculateUncommitted(disciplineRow);
                }


            }
        }

        private void updateEstimateAtComplete(decimal? oldValue, decimal? newValue)
        {
            decimal oldForecastUnits = oldValue == null ? 0 : (decimal)oldValue;
            decimal newForecastUnits = newValue == null ? 0 : (decimal)newValue;
            ForecastSummary.EstimateAtCompletion -= oldForecastUnits;
            ForecastSummary.EstimateAtCompletion += newForecastUnits;
            this.RaisePropertyChanged(x => x.ForecastSummary);
        }

        private DataRow findDisciplineRow(ExoSubJobProjection entity)
        {
            if(entity.Variation_Code == string.Empty || entity.Variation_Code == null)
            {
                return (from DataRow dr in dataPointsTable.Rows
                        where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == entity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == entity.Discipline.Code && (((ExoSubJobProjection)dr[columnEntity]).Variation_Code == string.Empty || ((ExoSubJobProjection)dr[columnEntity]).Variation_Code == null)
                        select dr).FirstOrDefault();
            }
            else
            {
                return (from DataRow dr in dataPointsTable.Rows
                        where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == entity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == entity.Discipline.Code && ((ExoSubJobProjection)dr[columnEntity]).Variation_Code == entity.Variation_Code
                        select dr).FirstOrDefault();
            }
        }

        private void findExistingOrAddNewEAC(ExoSubJobProjection entity, DateTime forecastDate, decimal eacAmount)
        {
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && x.IS_EAC);
            if (findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
                newFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
                newFORECAST.FORECAST_DATE = forecastDate.Date;
                newFORECAST.FORECAST_UNITS = eacAmount;
                newFORECAST.IS_EAC = true;
                FORECASTCollectionViewModel.Save(newFORECAST);
            }
            else
            {
                findFORECAST.FORECAST_UNITS = eacAmount;
                FORECASTCollectionViewModel.Save(findFORECAST);
            }
        }

        /// <summary>
        /// Sum uncommitted values
        /// </summary>
        private void calculateUncommitted(DataRow dataRow, bool updateDatabase = false)
        {
            ForecastCalculation calculation = (ForecastCalculation)dataRow[columnCalculation];
            DataTable dataTable = dataRow.Table;

            decimal uncommittedRecalculation = 0;
            for (int i = 0; i < dataRow.ItemArray.Count(); i++)
            {
                DataColumn dataColumn = dataTable.Columns[i];
                string columnName = dataColumn.ColumnName;
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                    if(parseDateTime >= FixedDataDate)
                        if(dataRow[columnName] != DBNull.Value && dataRow[columnName] != null)
                            if(((decimal)dataRow[columnName]) > 0)
                                uncommittedRecalculation += (decimal)dataRow[columnName];
            }

            ExoSubJobProjection entity = (ExoSubJobProjection)dataRow[columnEntity];
            if (entity.Commodity.Code == string.Empty)
            {
                if(dataRow[columnChild] != DBNull.Value)
                {
                    DataTable childTable = (DataTable)dataRow[columnChild];
                    for (int i = 0; i < dataRow.ItemArray.Count(); i++)
                    {
                        DataColumn dataColumn = dataTable.Columns[i];
                        string columnName = dataColumn.ColumnName;
                        DateTime parseDateTime;
                        if (DateTime.TryParse(columnName, out parseDateTime))
                        {
                            if (parseDateTime > FixedDataDate)
                            {
                                decimal cumulativeCostsOnDate = 0;
                                foreach (DataRow childRow in childTable.Rows)
                                {
                                    if (childRow[columnName] != DBNull.Value && childRow[columnName] != null)
                                        if (((decimal)childRow[columnName]) > 0)
                                            cumulativeCostsOnDate += (decimal)childRow[columnName];
                                }

                                if (cumulativeCostsOnDate != 0)
                                    dataRow[columnName] = cumulativeCostsOnDate;
                            }
                        }
                    }
                }
            }

            calculation.Uncommitted = uncommittedRecalculation;
        }

        public bool CanSaveEAC => isCompletelyLoaded;

        public void SaveEAC()
        {
            LoadingScreenManager.ShowLoadingScreen(DataPointsTable.Rows.Count);
            foreach(DataRow masterRow in DataPointsTable.Rows)
            {
                if (masterRow[columnChild] != DBNull.Value)
                {
                    DataTable childTable = (DataTable)masterRow[columnChild];
                    foreach(DataRow childRow in childTable.Rows)
                    {
                        ExoSubJobProjection childEntity = (ExoSubJobProjection)childRow[columnEntity];
                        ForecastCalculation childCalculation = (ForecastCalculation)childRow[columnCalculation];
                        findExistingOrAddNewEAC(childEntity, (DateTime)FixedDataDate, childCalculation.EstimateAtCompletion);
                    }
                }

                ExoSubJobProjection entity = (ExoSubJobProjection)masterRow[columnEntity];
                ForecastCalculation calculation = (ForecastCalculation)masterRow[columnCalculation];
                findExistingOrAddNewEAC(entity, (DateTime)FixedDataDate, calculation.EstimateAtCompletion);
                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
        }

        public bool CanUndo()
        {
            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanUndo();
        }

        public bool CanRedo()
        {
            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanRedo();
        }

        public void Undo()
        {
            EntitiesUndoRedoManager.Undo();
        }

        public void Redo()
        {
            EntitiesUndoRedoManager.Redo();
        }

        //Unstable
        public void ExpandAllMasterRows()
        {
            GridControlService.ExpandAllMasterRows();
        }

        public void CollapseAllMasterRows()
        {
            GridControlService.CollapseAllMasterRows();
        }

        /// <summary>
        /// Manages all undo and redo operation
        /// </summary>
        private EntitiesUndoRedoManager<DataRow> entitiesundoredomanager { get; set; }

        public EntitiesUndoRedoManager<DataRow> EntitiesUndoRedoManager
        {
            get
            {
                if (entitiesundoredomanager == null)
                    entitiesundoredomanager = new EntitiesUndoRedoManager<DataRow>(BulkPropertyUndo, BulkPropertyRedo);

                return entitiesundoredomanager;
            }
        }

        protected bool isBackgroundEdit = false;
        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object oldValue = entityProperty.OldValue;
                ExoSubJobProjection exoSubJob = (ExoSubJobProjection)entityProperty.ChangedEntity[columnEntity];
                if (oldValue == null || oldValue == DBNull.Value)
                {
                    setForecastCellNull(entityProperty.ChangedEntity, exoSubJob, entityProperty.PropertyName);
                    //oldValue = 0.00m;
                }
                else
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = oldValue;

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? oldValueDecimal = null;
                    if (entityProperty.OldValue != null)
                        oldValueDecimal = (decimal)entityProperty.OldValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, exoSubJob, parseDateTime, oldValueDecimal, true);
                }
            }

            foreach(UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                calculateUncommitted(entityProperty.ChangedEntity);
            }

            GridControlService.RefreshData();
            isBackgroundEdit = false;
        }

        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object newValue = entityProperty.NewValue;
                ExoSubJobProjection exoSubJob = (ExoSubJobProjection)entityProperty.ChangedEntity[columnEntity];
                if (newValue == null || newValue == DBNull.Value)
                {
                    setForecastCellNull(entityProperty.ChangedEntity, exoSubJob, entityProperty.PropertyName);
                    //newValue = 0.00m;
                }
                else
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? newValueDecimal = null;
                    if (entityProperty.NewValue != DBNull.Value && entityProperty.NewValue != null)
                        newValueDecimal = (decimal)entityProperty.NewValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, exoSubJob, parseDateTime, newValueDecimal, true);
                }
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                calculateUncommitted(entityProperty.ChangedEntity);
            }

            GridControlService.RefreshData();
            isBackgroundEdit = false;
        }

        public void CopyDetailWithHeader()
        {
            DetailGridControlService.CopyWithHeader();
        }

        ObservableCollection<DataRowView> selectedDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedDataRows
        {
            get
            {
                return selectedDataRows;
            }
            set
            {
                selectedDataRows = value;
            }
        }

        private DevExpress.Mvvm.IDialogService DistributionDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DistributionDialogService"); }
        }

        public bool CanDistributeUnits(object parameter)
        {
            if (!isCompletelyLoaded)
                return false;

            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return false;
                else
                {
                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            return true;
        }

        public void DistributeUnits(object parameter)
        {
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return;
                else
                {
                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            foreach (var selectedCell in selected_cells)
            {
                var gridColumn = gridControl.Columns[selectedCell.Column.FieldName];
                if (gridColumn == null || gridColumn.ReadOnly)
                {
                    MessageBoxService.ShowMessage("Your selection contains read only cell, please revise your selection");
                    return;
                }
            }

            var distributionSelectViewModel = DistributionSelectViewModel.Create(gridControl, selected_cells);
            if (DistributionDialogService.ShowDialog(MessageButton.OKCancel, "Select distribution method", "DistributionSelect", distributionSelectViewModel) == MessageResult.OK)
            {
                string newValueString = distributionSelectViewModel.ConvertToPasteData();
                string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
                pasteCellData(gridControl, tableView, RowData);
            }
        }

        public bool CanCreateExportSheet()
        {
            return ExportTable != null && FixedDataDate != null;
        }

        public void CreateExportSheet()
        {
            ExportTableViewService.ApplyBestFit();
            string ResultPath = string.Empty;
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                DateTime exportDate = (DateTime)FixedDataDate;
                MemoryStream stream = new MemoryStream();

                //copy group configuration
                GridColumnCollection exportGridColumns = ExportGridControlService.GridColumns();
                GridColumnCollection defaultGridColumns = GridControlService.GridColumns();

                foreach (GridColumn gridColumn in defaultGridColumns)
                {
                    string fieldName = gridColumn.FieldName;
                    if (exportGridColumns.Any(x => x.FieldName == fieldName))
                    {
                        exportGridColumns[fieldName].GroupIndex = gridColumn.GroupIndex;
                    }
                }

                if (ExportTableViewService.ExportToXls(stream))
                {
                    using (SpreadsheetControl spreadsheetControl = new SpreadsheetControl())
                    {
                        System.Drawing.Color editableColor = System.Drawing.Color.Yellow;
                        string costFormat = "$#,###,###";
                        string numberFormat = "#,###,###";
                        spreadsheetControl.LoadDocument(stream);
                        Worksheet ws = spreadsheetControl.Document.Worksheets[0];
                        DevExpress.Spreadsheet.Range usedRange = ws.GetUsedRange();
                        ws["$A:$XFD"].Protection.Locked = false;
                        for (int rowIndex = 0; rowIndex < usedRange.RowCount; rowIndex++)
                        {
                            bool isReadOnly = false;
                            Cell commodityCell = usedRange[rowIndex, spreadSheetCommodityIndex];
                            if (commodityCell.Value.IsEmpty)
                                isReadOnly = true;

                            Cell phaseCell = usedRange[rowIndex, spreadSheetPhaseIndex];
                            Cell areaCell = usedRange[rowIndex, spreadSheetAreaIndex];
                            Cell subAreaCell = usedRange[rowIndex, spreadSheetSubAreaIndex];
                            Cell subJobCell = usedRange[rowIndex, spreadSheetSubJobIndex];
                            Cell subJobTitleCell = usedRange[rowIndex, spreadSheetSubJobTitleIndex];
                            Cell variationCell = usedRange[rowIndex, spreadSheetVariationIndex];
                            Cell disciplineCell = usedRange[rowIndex, spreadSheetDisciplineIndex];
                            Cell disciplineNameCell = usedRange[rowIndex, spreadSheetDisciplineNameIndex];
                            Cell commodityNameCell = usedRange[rowIndex, spreadSheetCommodityNameIndex];
                            Cell commodityDescriptionCell = usedRange[rowIndex, spreadSheetCommodityDescriptionIndex];
                            Cell commodityUOMCell = usedRange[rowIndex, spreadSheetCommodityUOMIndex];
                            Cell budgetCell = usedRange[rowIndex, spreadSheetBudgetIndex];
                            Cell rateCell = usedRange[rowIndex, spreadSheetRateIndex];

                            budgetCell.NumberFormat = costFormat;
                            rateCell.NumberFormat = costFormat;
                            ws[phaseCell.GetReferenceA1()].Protection.Locked = true;
                            ws[areaCell.GetReferenceA1()].Protection.Locked = true;
                            ws[subAreaCell.GetReferenceA1()].Protection.Locked = true;
                            ws[subJobCell.GetReferenceA1()].Protection.Locked = true;
                            ws[subJobTitleCell.GetReferenceA1()].Protection.Locked = true;
                            ws[variationCell.GetReferenceA1()].Protection.Locked = true;
                            ws[disciplineCell.GetReferenceA1()].Protection.Locked = true;
                            ws[disciplineNameCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityNameCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityDescriptionCell.GetReferenceA1()].Protection.Locked = true;
                            ws[commodityUOMCell.GetReferenceA1()].Protection.Locked = true;

                            string uomFormat = numberFormat;
                            if (!commodityUOMCell.Value.IsEmpty)
                                uomFormat = numberFormat + @" """ + commodityUOMCell.Value.TextValue + @"""";

                            if (isReadOnly)
                            {
                                ws[budgetCell.GetReferenceA1()].Protection.Locked = true;
                                ws[rateCell.GetReferenceA1()].Protection.Locked = true;
                            }
                            else
                            {
                                budgetCell.FillColor = editableColor;
                                rateCell.FillColor = editableColor;

                                for (int columnIndex = spreadSheetDateStartIndex; columnIndex < usedRange.ColumnCount; columnIndex++)
                                {
                                    DateTime columnDate;
                                    Cell dateCell = usedRange[0, columnIndex];
                                    if (!DateTime.TryParse(dateCell.Value.TextValue, out columnDate))
                                        continue;

                                    Cell currentCell = usedRange[rowIndex, columnIndex];
                                    currentCell.FillColor = editableColor;
                                    currentCell.NumberFormat = uomFormat;
                                }
                            }
                        }

                        string exportPath = ResultPath + "\\" + exportDate.Year + "-" + exportDate.ToString("MMM") + "_" + loadPROJECT.NUMBER + "_Forecast" + ".xlsx";
                        try
                        {
                            
                            ws.Protect("", WorksheetProtectionPermissions.Default | WorksheetProtectionPermissions.FormatColumns | WorksheetProtectionPermissions.PivotTables | WorksheetProtectionPermissions.Sort | WorksheetProtectionPermissions.AutoFilters | WorksheetProtectionPermissions.SelectLockedCells | WorksheetProtectionPermissions.SelectUnlockedCells | WorksheetProtectionPermissions.FormatCells | WorksheetProtectionPermissions.FormatRows);

                            spreadsheetControl.Options.Behavior.Group.CollapseExpandOnProtectedSheet = DevExpress.XtraSpreadsheet.DocumentCapability.Enabled;
                            spreadsheetControl.Options.Behavior.Group.CollapseExpandOnReadOnlyControl = DevExpress.XtraSpreadsheet.DocumentCapability.Enabled;
                            spreadsheetControl.SaveDocument(exportPath);
                            Process.Start(exportPath);
                        }
                        catch
                        {
                            MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Warning);
                        }
                    }
                }
                else
                    MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Information);
            }
        }

        public bool CanImportSheet()
        {
            return ExportTable != null && FixedDataDate != null;
        }

        public void ImportSheet()
        {
            string ResultPath = string.Empty;
            FileBrowserDialogService.Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*";
            FileBrowserDialogService.FilterIndex = 1;
            FileBrowserDialogService.Title = "Import forecast sheet";
            if (FileBrowserDialogService.ShowDialog())
            {
                ResultPath = FileBrowserDialogService.GetFullFileName();

                bool speedMode = false;
                if (MessageBoxService.ShowMessage("Do you want to skip rows that have 0 rate to speed things up?", "Info", MessageButton.YesNo, MessageIcon.Question) == MessageResult.Yes)
                    speedMode = true;

                List<FORECAST> addOrEditForecast = new List<FORECAST>();
                using (SpreadsheetControl spreadsheetControl = new SpreadsheetControl())
                {
                    spreadsheetControl.LoadDocument(ResultPath);
                    Worksheet ws = spreadsheetControl.Document.Worksheets[0];
                    DevExpress.Spreadsheet.Range usedRange = ws.GetUsedRange();
                    IEnumerable<DataTable> childTables = from DataRow dr in dataPointsTable.Rows
                                                         select (DataTable)dr[columnChild];

                    IEnumerable<DataRow> childRowsCollection = childTables.SelectMany(x => x.Rows.Cast<DataRow>().ToArray());
                    EntitiesUndoRedoManager.PauseActionId();
                    LoadingScreenManager.ShowLoadingScreen(usedRange.RowCount, false);
                    for (int rowIndex = 0; rowIndex < usedRange.RowCount; rowIndex++)
                    {
                        LoadingScreenManager.Progress();
                        Cell subJobCell = usedRange[rowIndex, spreadSheetSubJobIndex];
                        if (subJobCell.Value.IsEmpty)
                            continue;

                        string loadingMessage = string.Empty;
                        string subJobCode = subJobCell.Value.TextValue;
                        loadingMessage += subJobCode;


                        Cell disciplineCell = usedRange[rowIndex, spreadSheetDisciplineIndex];
                        if (disciplineCell.Value.IsEmpty)
                            continue;

                        string disciplineCode = disciplineCell.Value.TextValue;
                        if(disciplineCode != string.Empty)
                            loadingMessage += "-" + disciplineCode;

                        Cell commodityCell = usedRange[rowIndex, spreadSheetCommodityIndex];
                        if (commodityCell.Value.IsEmpty)
                            continue;

                        string commodityCode = commodityCell.Value.TextValue;
                        if (commodityCode != string.Empty)
                            loadingMessage += "-" + commodityCode;

                        Cell variationCell = usedRange[rowIndex, spreadSheetVariationIndex];
                        if (variationCell.Value.IsEmpty)
                            continue;

                        string variationCode = variationCell.Value.TextValue;
                        if (variationCode != string.Empty)
                            loadingMessage += "-" + variationCode;

                        LoadingScreenManager.SetMessage("Processing " + loadingMessage);
                        Cell budgetCell = usedRange[rowIndex, spreadSheetBudgetIndex];
                        decimal budget = 0;
                        if (!budgetCell.Value.IsEmpty && budgetCell.Value.IsNumeric)
                            budget = Convert.ToDecimal(budgetCell.Value.NumericValue);

                        Cell rateCell = usedRange[rowIndex, spreadSheetRateIndex];
                        decimal rate = 0;
                        if (!rateCell.Value.IsEmpty && rateCell.Value.IsNumeric)
                            rate = Convert.ToDecimal(rateCell.Value.NumericValue);

                        if (speedMode && rate == 0)
                            continue;

                        DataRow dataRow = (from DataRow dr in childRowsCollection
                                where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == subJobCode && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == disciplineCode && ((ExoSubJobProjection)dr[columnEntity]).Commodity.Code == commodityCode && ((ExoSubJobProjection)dr[columnEntity]).Variation_Code == variationCode
                                select dr).FirstOrDefault();
                        
                        if (dataRow == null)
                            continue;

                        ForecastCalculation forecastCalculation = (ForecastCalculation)dataRow[columnCalculation];
                        commitCellValue("CALCULATION.BUDGET", dataRow, forecastCalculation.Budget, budget);
                        commitCellValue("ENTITY.RATE", dataRow, forecastCalculation.Rate, rate);
                        forecastCalculation.Budget = budget;
                        forecastCalculation.Rate = rate;

                        for (int columnIndex = spreadSheetDateStartIndex; columnIndex < usedRange.ColumnCount; columnIndex++)
                        {
                            DateTime columnDate;
                            Cell dateCell = usedRange[0, columnIndex];
                            if (!DateTime.TryParse(dateCell.Value.TextValue, out columnDate))
                                continue;

                            Cell currentCell = usedRange[rowIndex, columnIndex];
                            if (!currentCell.Value.IsEmpty)
                            {
                                // numeric values
                                if (currentCell.Value.IsNumeric)
                                {
                                    double newValue = currentCell.Value.NumericValue;
                                    decimal newCost = rate * Convert.ToDecimal(newValue);
                                    string dateFieldName = columnDate.ToShortDateString();
                                    if(DataPointsTable.Columns.Contains(dateFieldName))
                                    {
                                        object oldValue = dataRow[dateFieldName];
                                        EntitiesUndoRedoManager.AddUndo(dataRow, dateFieldName, oldValue, newCost, EntityMessageType.Changed);
                                        ExoSubJobProjection entity = (ExoSubJobProjection)dataRow[columnEntity];
                                        FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == columnDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && !x.IS_EAC);
                                        if (findFORECAST == null)
                                        {
                                            FORECAST findFORECASTHarder = addOrEditForecast.FirstOrDefault(x => x.FORECAST_DATE == columnDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.VARIATION_CODE == entity.Variation_Code && !x.IS_EAC);
                                            if (findFORECASTHarder == null)
                                            {
                                                FORECAST newFORECAST = new FORECAST();
                                                newFORECAST.GUID = Guid.Empty;
                                                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                                                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                                                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                                                newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
                                                newFORECAST.VARIATION_CODE = normalizeVariationCode(entity.Variation_Code);
                                                newFORECAST.FORECAST_DATE = columnDate.Date;
                                                newFORECAST.FORECAST_UNITS = newCost;
                                                addOrEditForecast.Add(newFORECAST);
                                            }
                                            else
                                                findFORECASTHarder.FORECAST_UNITS = newCost;
                                        }
                                        else
                                        {
                                            findFORECAST.FORECAST_UNITS = newCost;
                                            addOrEditForecast.Add(findFORECAST);
                                        }

                                        //used to ensure child row is set
                                        if (newCost == 0)
                                            dataRow[dateFieldName] = 0.00m;
                                        else
                                            dataRow[dateFieldName] = newCost;

                                        //commitCellValue(dateFieldName, dataRow, oldValue, newCost);
                                    }
                                }
                            }
                        }
                    }

                    FORECASTCollectionViewModel.BulkSave(addOrEditForecast);
                    LoadingScreenManager.CloseLoadingScreen();
                    EntitiesUndoRedoManager.UnpauseActionId();
                }

                exportTable = null;
                dataPointsTable = null;
                initializeSummaryStats();

                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Reloading data");
                //this will raise changes for Export Table as well
                this.RaisePropertyChanged(x => x.DataPointsTable);
                LoadingScreenManager.SetMessage("Resizing column");
                TableViewService.ApplyBestFit();
                LoadingScreenManager.CloseLoadingScreen();
                MessageBoxService.ShowMessage("Import completed", "Success!", MessageButton.OK, MessageIcon.Information);
            }
        }
        #endregion

        #region Entity Wrapper Properties
        public CollectionViewModel<FORECAST, FORECAST, Guid, IBluePrintsEntitiesUnitOfWork> FORECASTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<FORECAST, FORECAST, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<FORECAST>();
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public CollectionViewModel<VARIATION_REGISTER, VARIATION_REGISTER, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_REGISTERCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<VARIATION_REGISTER, VARIATION_REGISTER, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<VARIATION_REGISTER>();
            }
        }

        protected override string ViewName => "PROJECTForecastView_v1.00";

        public void LoadLayout()
        {
            PersistentLayoutHelper.TryDeserializeLayout(LayoutSerializationService, ViewName);
        }
        #endregion

        //#region Custom Summary
        //private decimal cumulative_total_units = 0;
        //private decimal cumulative_baseline_units = 0;
        //private decimal cumulative_current_units = 0;
        //public void CustomCommoditySummary(CustomSummaryEventArgs e)
        //{
        //    if (e.IsTotalSummary || e.IsGroupSummary)
        //    {
        //        if (e.SummaryProcess == CustomSummaryProcess.Start)
        //        {
        //            cumulative_total_units = 0;
        //        }
        //        if (e.SummaryProcess == CustomSummaryProcess.Calculate)
        //        {
        //            ExoDataPoint dataPoint = ((ExoDataPoint)e.Row);
        //            ExoTimeAuthorisation exoTime = jobLines.FirstOrDefault(x => x.SubJobCode == dataPoint.Subjob_Name && x.CommodityCode == dataPoint.Commodity_Code);
        //            if(exoTime != null)
        //                cumulative_total_units = exoTime.BudgetCosts;

        //            e.TotalValue = cumulative_total_units;
        //        }
        //    }
        //}
        //#endregion
    }

    /// <summary>
    /// Currently unused
    /// </summary>
    public class CostFilteringViewModel
    {
        [FilterLookup("Subjobs", UseBlanks = false, UseSelectAll = false)]
        [Display(Name = "Subjob")]
        public string Subjob_Name { get; set; }
        [FilterRange("StartSelectionDate", "EndSelectionDate")]
        [Display(Name = "Actual Date")]
        public DateTime ActualDate { get; set; }
    }

    public class ForecastCalculation
    {
        public decimal Budget { get; set; }
        public decimal Rate { get; set; }
        public decimal Revenue { get; set; }
        public decimal CurrentBudget => Budget + Variation;
        public decimal Variation { get; set; }
        public decimal Actuals { get; set; }
        public decimal Invoiced { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Uncommitted { get; set; }
        public decimal PreviousEAC { get; set; }
        public decimal EstimateToComplete => Outstanding + Uncommitted;
        public decimal EstimateAtCompletion => Actuals + Outstanding + Uncommitted;
        public decimal PeriodMovement => PreviousEAC - EstimateAtCompletion;
        public decimal PctComplete => EstimateAtCompletion == 0 ? 1 : Actuals / EstimateAtCompletion;
        public decimal Variance => Budget - EstimateAtCompletion;
        public bool IsBudgetReadOnly { get; set; }
    }

    public class ForecastSummary
    {
        /// <summary>
        /// Reset all settable figures to 0
        /// </summary>
        public void Reset()
        {
            Revenue = 0;
            Costs = 0;
            TotalClaims = 0;
            Actuals = 0;
            Commitments = 0;
            EstimateAtCompletion = 0;
        }

        public decimal Revenue { get; set; }
        public decimal Costs { get; set; }
        public decimal Margin => Revenue - Costs;
        public decimal Margin_Percent => Revenue == 0 ? 0 : Margin / Revenue;
        public decimal TotalClaims { get; set; }
        public decimal Actuals { get; set; }
        public decimal Commitments { get; set; }
        public decimal EstimateAtCompletion { get; set; }
    }
}