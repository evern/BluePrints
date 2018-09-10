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
        }

        private Func<IRepositoryQuery<FORECAST>, IQueryable<FORECAST>> FORECASTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        public bool IsLoadingForecast { get; set; }
        public bool IsHidden { get; set; }
        public CriteriaOperator FilterCriteria { get; set; }
        public virtual DateTime EndSelectionDate { get; set; }
        public virtual DateTime StartSelectionDate { get; set; }
        public virtual IEnumerable<string> Subjobs { get; set; }
        IEnumerable<ExoTimeAuthorisation> jobLines { get; set; }
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        IEnumerable<ExoSubJobProjection> exoSubJobs;
        List<string> hiddenColumnFieldNames = new List<string>();
        List<DateTime> alignedDataDateCollection;
        protected virtual IGridControlService DetailGridControlService { get { return this.GetService<IGridControlService>("DetailGridControlService"); } }
        protected override void resolveParameters(object parameter)
        {
            IsLoadingForecast = true;
            LoadingScreenManager.DisableLoadingScreen = true;
            base.resolveParameters(parameter);
            skipBindingSwitch = true;
            hiddenColumnFieldNames.Add(columnEntity);
            hiddenColumnFieldNames.Add(columnCalculation);
            hiddenColumnFieldNames.Add(columnChild);
            jobLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
            exoSubJobs = ExoQueries.GetNativeExoSubJobProjection(primeroUnitOfWork, loadPROJECT);
            SelectedDataRows = new ObservableCollection<DataRowView>();
            StartSelectionDate = DateTime.Now;
            DetailedData = new List<ExoDataPoint>();
            alignedDataDateCollection = new List<DateTime>();
            IsHidden = true;
            delayPostLoadedTimer = true;
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
                    FullRefresh();
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
                    FullRefresh();
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
                    FullRefresh();
                }
            }
        }

        protected override void onSummaryCalculateComplete()
        {
            alignedDataDateCollection.Clear();
            DetailedData.Clear();
            EntitiesUndoRedoManager.Clear();
            MainViewModel.IsPasteCellLevel = true;
            this.RaisePropertyChanged(x => x.MainViewModel.IsPasteCellLevel);
            FORECASTCollectionViewModel.SetParentViewModel(this);
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
            base.FullRefresh();
        }

        private void refreshDataTable()
        {
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        #region Data Points Table
        string columnEntity = "Entity";
        string columnCalculation = "Calculation";
        string columnChild = "ChildEntities";
        DataTable dataPointsTable = null;
        DateTime firstAlignedDataDate;

        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || AllProjectDashboards == null)
                    return null;

                if (dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    TimeSpan interval = new TimeSpan(7, 0, 0, 0);
                    firstAlignedDataDate = ChronologicalHelpers.RewindDataDate((DateTime)FixedStartDate, (DateTime)FixedDataDate, new TimeSpan(7, 0, 0, 0));

                    IEnumerable<Stats> actualStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
                    IEnumerable<Stats> materialStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
                    IEnumerable<Stats> poStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);

                    //IEnumerable<Stats> remainingStats = AllProjectDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null).Select(x => x.Stats.Remaining);
                    //DateTime lastDataDate = FixedEndDate == null ?  DateTime.Now : (DateTime)FixedEndDate;
                    //if(remainingStats.Count() > 0)
                    //    lastDataDate = remainingStats.Max(x => x.EndDate);

                    List<ExoSubJobProjection> combinedSubJobs = new List<ExoSubJobProjection>();
                    combinedSubJobs.AddRange(exoSubJobs.Select(x => new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = x.SubJob.Code }, Discipline = new PrimeroDiscipline() { Code = x.Discipline.Code }, Commodity = new PrimeroCommodity() { Code = x.Commodity.Code } }));

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
                        string subjobName = delimited[0];
                        string disciplineName = delimited[1];
                        string commodityName = delimited[2];
                        string variationCode = delimited[3];

                        if(!combinedSubJobs.Any(x => x.SubJob.Code == subjobName && x.Discipline.Code == disciplineName && x.Commodity.Code == commodityName && x.Variation_Code == variationCode))
                        {
                            combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subjobName }, Discipline = new PrimeroDiscipline() { Code = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityName }, Variation_Code = variationCode });
                        }
                    }

                    //lastDataDate = lastDataDate.AddDays(10 * interval.Days);
                    alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, (DateTime)FixedEndDate, interval);
                    dataPointsTable.Columns.Add(columnEntity, typeof(ExoSubJobProjection));
                    dataPointsTable.Columns.Add(columnCalculation, typeof(ForecastCalculation));
                    dataPointsTable.Columns.Add(columnChild, typeof(DataTable));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    var groupedSubjobs = combinedSubJobs.GroupBy(x => x.SubJob.Code + x.Discipline.Code + x.Variation_Code).Select(group => new { ProgressDate = group.Key, Projection = group.ToList() });
                    foreach (var groupedSubjob in groupedSubjobs)
                    {
                        DataRow dataRow = buildRowStats(groupedSubjob.Projection, false);

                        DataTable childDataTable = new DataTable();
                        childDataTable = dataPointsTable.Clone();

                        DataRow cloneRow = childDataTable.NewRow();
                        setDateFieldsEmpty(cloneRow, false);

                        updateForecast(cloneRow, groupedSubjob.Projection.First(), true);
                        childDataTable.Rows.Add(cloneRow);
                        dataRow[columnChild] = childDataTable;
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
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
        private DataRow buildSingleRowStat(ExoSubJobProjection subjob, bool isUpdate)
        {
            List<ExoSubJobProjection> exoSubJobProjections = new List<ExoSubJobProjection>();
            exoSubJobProjections.Add(subjob);
            return buildRowStats(exoSubJobProjections, isUpdate);
        }

        private DataRow buildRowStats(IEnumerable<ExoSubJobProjection> groupedSubJobs, bool isUpdate)
        {
            if (dataPointsTable == null)
                return null;

            if (groupedSubJobs == null || groupedSubJobs.Count() == 0)
                return null;

            ExoSubJobProjection entity = groupedSubJobs.First();
            DataRow findExistingOrNewDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == entity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == entity.Discipline.Code && ((ExoSubJobProjection)dr[columnEntity]).Variation_Code == entity.Variation_Code
                              select dr).FirstOrDefault();

            ForecastCalculation forecastCalculation = new ForecastCalculation();
            if (findExistingOrNewDataRow == null)
            {
                //during update the entity must be found
                if (isUpdate)
                    return null;

                findExistingOrNewDataRow = dataPointsTable.NewRow();

                //variation budget cannot be found on exo
                if(entity.Variation_Code == string.Empty)
                {
                    IEnumerable<ExoTimeAuthorisation> relevantJobLines = jobLines.Where(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                    entity.ExoBudgetQty = relevantJobLines.Sum(x => x.BudgetQty);
                    entity.ExoBudgetCosts = relevantJobLines.Sum(x => x.BudgetCosts);
                }

                findExistingOrNewDataRow[columnEntity] = entity;
                findExistingOrNewDataRow[columnCalculation] = forecastCalculation;

                forecastCalculation.Budget = entity.ExoBudgetCosts;
                setDateFieldsEmpty(findExistingOrNewDataRow, false);

                dataPointsTable.Rows.Add(findExistingOrNewDataRow);
            }
            else if (isUpdate)
            {
                updateForecast(findExistingOrNewDataRow, entity, false);
                return findExistingOrNewDataRow;
            }

            forecastCalculation = (ForecastCalculation)findExistingOrNewDataRow[columnCalculation];
            IEnumerable<DashboardFlatStructure> relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.Variation_Code == entity.Variation_Code);
            if (relevantDashboards != null && relevantDashboards.Count() > 0)
            {
                IEnumerable<SummaryStats> summaryStats = relevantDashboards.Select(x => (SummaryStats)x.Stats);
                IEnumerable<SummaryStats> poStats = summaryStats.Where(x => x.PO != null && x.PO.DataPoints != null);

                if(poStats != null && poStats.Count() > 0)
                {
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> poDataPoints = poStats.SelectMany(x => x.PO.DataPoints);
                    forecastCalculation.Outstanding = poDataPoints.Sum(x => x.Costs);
                }

                if(entity.Variation_Code == string.Empty)
                {
                    IEnumerable<SummaryStats> remainingStats = summaryStats.Where(x => x.Remaining != null && x.Remaining.DataPoints != null);
                    if (remainingStats != null && remainingStats.Count() > 0)
                    {
                        IEnumerable<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = remainingStats.SelectMany(x => x.Remaining.DataPoints);
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
                                        findExistingOrNewDataRow[alignedDateField] = currentValue;
                                }
                            }
                        }
                    }
                }

                IEnumerable<SummaryStats> actualStats = summaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null);
                if (actualStats != null && actualStats.Count() > 0)
                {
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> actualDataPoints = actualStats.SelectMany(x => x.Actual.DataPoints);
                    forecastCalculation.Actuals += actualDataPoints.Sum(x => x.Costs);
                    var groupByDateDataPoints = actualDataPoints.GroupBy(x => x.ProgressDate).Select(group => new { ProgressDate = group.Key, DataPoints = group.ToList() });
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
                                {
                                    decimal currentRowValue = (decimal)findExistingOrNewDataRow[alignedDateField];
                                    findExistingOrNewDataRow[alignedDateField] = currentRowValue + currentValue;
                                }
                            }
                        }
                    }
                }

                IEnumerable<SummaryStats> materialStats = summaryStats.Where(x => x.Material != null && x.Material.DataPoints != null);
                if (materialStats != null && materialStats.Count() > 0)
                {
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> materialDataPoints = materialStats.SelectMany(x => x.Material.DataPoints);
                    forecastCalculation.Actuals += materialDataPoints.Sum(x => x.Costs);
                    var groupByDateDataPoints = materialDataPoints.GroupBy(x => x.ProgressDate).Select(group => new { ProgressDate = group.Key, DataPoints = group.ToList() });
                    foreach (var groupByDateDataPoint in groupByDateDataPoints)
                    {
                        DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= groupByDateDataPoint.ProgressDate.Date);
                        if (alignedDataDate != null)
                        {
                            if (alignedDataDate < StartSelectionDate)
                                StartSelectionDate = (DateTime)alignedDataDate;

                            if (alignedDataDate > EndSelectionDate)
                                EndSelectionDate = (DateTime)alignedDataDate;

                            string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                            if (dataPointsTable.Columns.Contains(alignedDateField))
                            {
                                decimal currentValue = groupByDateDataPoint.DataPoints.Sum(x => x.Costs);
                                if (currentValue != 0)
                                {
                                    decimal currentRowValue = (decimal)findExistingOrNewDataRow[alignedDateField];
                                    findExistingOrNewDataRow[alignedDateField] = currentRowValue + currentValue;
                                }
                            }
                        }
                    }
                }

                //newDataRow[breakDownEntity] = exoDataPoints;
            }

            //effectively override remaining
            updateForecast(findExistingOrNewDataRow, entity, false);
            updateUncommitted(findExistingOrNewDataRow);

            return findExistingOrNewDataRow;
        }

        private void setForecastCellNull(DataRow updateRow, ExoSubJobProjection entity, string fieldName)
        {
            DateTime dateTime;
            if(DateTime.TryParse(fieldName, out dateTime))
            {
                IEnumerable<DashboardFlatStructure> relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                IEnumerable<Common.ViewModel.Reporting.DataPoint> dataPoints = relevantDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null).SelectMany(x => x.Stats.Remaining.DataPoints);
                IEnumerable<Common.ViewModel.Reporting.DataPoint> dateSpecificDataPoints = dataPoints.Where(x => x.ProgressDate.Date == dateTime);

                if(DataPointsTable.Columns.Contains(fieldName))
                    updateRow[fieldName] = dateSpecificDataPoints.Sum(x => x.Costs);

                updateUncommitted(updateRow);
            }
        }

        private void updateForecast(DataRow dataRow, ExoSubJobProjection entity, bool isChild)
        {
            IEnumerable<FORECAST> currentRowFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.VARIATION_CODE == entity.Variation_Code);
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
                        if ((alignedDataDate > FixedDataDate && !isChild) || (alignedDataDate <= FixedDataDate && isChild))
                            if (dataPointsTable.Columns.Contains(alignedDateField))
                            {
                                if (currentRowFORECAST.FORECAST_UNITS != null)
                                    dataRow[alignedDateField] = currentRowFORECAST.FORECAST_UNITS;
                            }
                    }
                }
            }

            //only do autoforcast on non variation entity
            if (isChild && entity.Variation_Code == string.Empty)
            {
                IEnumerable<DashboardFlatStructure> relevantDashboards = AllProjectDashboards.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
                IEnumerable<DashboardFlatStructure> remainingDashboard = relevantDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.DataPoints != null);
                if (remainingDashboard != null)
                {
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = remainingDashboard.SelectMany(x => x.Stats.Remaining.DataPoints);
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

        private void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (gridColumn.ReadOnly)
            {
                DateTime parseEndDate;
                if (DateTime.TryParse(gridColumn.ActualColumnChooserHeaderCaption.ToString(), out parseEndDate))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    parseEndDate = parseEndDate.AddDays(1).AddSeconds(-1);
                    EndSelectionDate = parseEndDate;
                    StartSelectionDate = parseEndDate.AddDays(-7);
                    if(parseEndDate == firstAlignedDataDate)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [ActualDate] > #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");

                    IsHidden = false;

                    this.RaisePropertyChanged(x => x.FilterCriteria);
                }
                else if(gridColumn.FieldName.ToUpper().Contains("OUTSTANDING"))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Variation_Code] = '" + entity.Variation_Code + "' And [IsPO] = 'True'");
                    IsHidden = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
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

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
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

                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c2");
                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
                else
                {
                    if (e.Column.FieldType == typeof(decimal))
                        GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, e.Column.FieldName + ": {0:c2}");

                    e.Column.ReadOnly = true;
                    e.Column.Fixed = FixedStyle.Left;
                }
            }
            else
            {
                e.Cancel = true;
            }
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

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();
            string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
            pasteCellData(gridControl, gridTableView, RowData);

            GridControlService.RefreshData();
            e.Handled = true;
        }


        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.Clear();
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = gridTableView.GetSelectedCells();
            if (selected_cells.Count == 0)
                return;

            List<List<string>> row_data = new List<List<string>>();
            foreach (var row in RowData)
            {
                List<string> column_data = row.Split('\t').ToList();
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
                        findExistingOrAddNewForecast(newRow, entity, columnDateTime, decimal_value);
                    }
                }
                else
                {
                    if (!isNewRow)
                        EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, newRow[copyColumn.FieldName], DBNull.Value, EntityMessageType.Changed);

                    setForecastCellNull(newRow, entity, copyColumn.FieldName);
                    //newRow[dataColumn] = DBNull.Value;
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
            EntitiesUndoRedoManager.Clear();
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = tableView.GetSelectedCells();
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
                    findExistingOrAddNewForecast(editing_row, entity, deleteCellDate, null);
                    //editing_row[columnFieldName] = 0.00m;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            GridControlService.RefreshData();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
             if (changedType == typeof(FORECAST))
            {
                FORECAST changedFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
                if (changedFORECAST != null)
                {
                    ExoSubJobProjection findUpdatedEntity = exoSubJobs.FirstOrDefault(x => x.SubJob.Code == changedFORECAST.SUBJOB_CODE && x.Discipline.Code == changedFORECAST.DISCIPLINE_CODE);
                    if(findUpdatedEntity != null)
                    {
                        buildSingleRowStat(findUpdatedEntity, true);
                    }
                }

                mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            EntitiesUndoRedoManager.Clear();
            DataRowView dataRowView = (DataRowView)e.Row;
            ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView.Row[columnEntity];
            EntitiesUndoRedoManager.PauseActionId();
            if (e.Column.FieldName.ToUpper().Contains("ENTITY"))
            {
                //entity.Entity.Entity.PRIMARY_TITLE = e.Value.ToString();
                ///MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, columnPrimaryTitle, e.OldValue, e.Value, EntityMessageType.Changed);
                //MainViewModel.Save(entity);
            }
            else
            {
                string fieldName = e.Column.FieldName;
                DateTime dateTime;
                if(DateTime.TryParse(fieldName, out dateTime))
                {
                    decimal? forecastUnits = null;
                    decimal convertUnits = 0;
                    if (e.Value != null && decimal.TryParse(e.Value.ToString(), out convertUnits))
                        forecastUnits = convertUnits;

                    EntitiesUndoRedoManager.AddUndo(dataRowView.Row, fieldName, e.OldValue, forecastUnits, EntityMessageType.Changed);
                    findExistingOrAddNewForecast(dataRowView.Row, entity, dateTime, forecastUnits);
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            e.Handled = true;
        }

        private void findExistingOrAddNewForecast(DataRow dataRow, ExoSubJobProjection entity, DateTime forecastDate, decimal? forecastUnits)
        {
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.VARIATION_CODE == entity.Variation_Code);
            if(findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = string.Empty;
                newFORECAST.VARIATION_CODE = entity.Variation_Code;
                newFORECAST.FORECAST_DATE = forecastDate.Date;
                newFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(newFORECAST);
                updateUncommitted(dataRow);
            }
            else
            {
                findFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(findFORECAST);
                updateUncommitted(dataRow);
            }
        }

        private void updateUncommitted(DataRow dataRow)
        {
            ForecastCalculation calculation = (ForecastCalculation)dataRow[columnCalculation];

            decimal uncommittedRecalculation = 0;
            for (int i = 0; i < dataRow.ItemArray.Count(); i++)
            {
                DataColumn dataColumn = dataPointsTable.Columns[i];
                string columnName = dataColumn.ColumnName;
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                    if(parseDateTime > FixedDataDate)
                        if(((decimal)dataRow[columnName]) > 0)
                            uncommittedRecalculation += (decimal)dataRow[columnName];
            }

            calculation.Uncommitted = uncommittedRecalculation;
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

        bool isBackgroundEdit = false;
        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);

            //use ignore refresh here because it'll be refreshed in basebulksave

            EntitiesUndoRedoManager.PauseActionId();
            foreach (var bulkDeleteProperty in bulkDeleteProperties)
            {
                if (!entityProperties.Any(x => x.ActionId == bulkDeleteProperty.ActionId && x.MessageType == EntityMessageType.Changed))
                {
                    foreach (DataColumn column in DataPointsTable.Columns)
                    {
                        EntitiesUndoRedoManager.AddRedo(bulkDeleteProperty.ChangedEntity, column.ColumnName, bulkDeleteProperty.ChangedEntity[column], bulkDeleteProperty.ChangedEntity[column], EntityMessageType.Changed);
                    }
                }

                DataPointsTable.Rows.Remove(bulkDeleteProperty.ChangedEntity);
            }
            EntitiesUndoRedoManager.UnpauseActionId();

            foreach (var bulkAddProperty in bulkAddProperties)
            {
                DataPointsTable.Rows.Add(bulkAddProperty.ChangedEntity);
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object oldValue = entityProperty.OldValue;
                ExoSubJobProjection exoSubJob = (ExoSubJobProjection)entityProperty.ChangedEntity[columnEntity];
                if (oldValue == null)
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
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, exoSubJob, parseDateTime, oldValueDecimal);
                }
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
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);

            //use ignore refresh here because it'll be refreshed in basebulksave

            EntitiesUndoRedoManager.PauseActionId();
            foreach (var bulkDeleteProperty in bulkDeleteProperties)
            {
                if (!entityProperties.Any(x => x.ActionId == bulkDeleteProperty.ActionId && x.MessageType == EntityMessageType.Changed))
                {
                    foreach (DataColumn column in DataPointsTable.Columns)
                    {
                        EntitiesUndoRedoManager.AddRedo(bulkDeleteProperty.ChangedEntity, column.ColumnName, bulkDeleteProperty.ChangedEntity[column], bulkDeleteProperty.ChangedEntity[column], EntityMessageType.Changed);
                    }
                }

                DataPointsTable.Rows.Remove(bulkDeleteProperty.ChangedEntity);
                //bulkDeleteProperty.ChangedEntity.Delete();
            }
            EntitiesUndoRedoManager.UnpauseActionId();

            foreach (var bulkAddProperty in bulkAddProperties)
            {
                DataPointsTable.Rows.Add(bulkAddProperty.ChangedEntity);
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object newValue = entityProperty.NewValue;
                ExoSubJobProjection exoSubJob = (ExoSubJobProjection)entityProperty.ChangedEntity[columnEntity];
                if (newValue == null)
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
                    if (entityProperty.NewValue != null)
                        newValueDecimal = (decimal)entityProperty.NewValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, exoSubJob, parseDateTime, newValueDecimal);
                }
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
                return false;

            return true;
        }

        public void DistributeUnits(object parameter)
        {
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();

            foreach(var selectedCell in selected_cells)
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

        protected override string ViewName => "PROJECTForecastView_v1.00";

        public void LoadLayout()
        {
            PersistentLayoutHelper.TryDeserializeLayout(LayoutSerializationService, ViewName);
        }
        #endregion
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
        public decimal Actuals { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Uncommitted { get; set; }
        public decimal EstimateAtCompletion => Actuals + Outstanding + Uncommitted;
        public decimal Variance => Budget - EstimateAtCompletion;
    }
}