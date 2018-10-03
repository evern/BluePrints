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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_REGISTERS, VARIATION_REGISTERProjectionFunc);
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
            hiddenColumnFieldNames.Add(columnCompare);
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
        string columnCompare = "CompareEntities";
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
                    firstAlignedDataDate = ChronologicalHelpers.RewindDataDate((DateTime)FixedStartDate, (DateTime)FixedDataDate, new TimeSpan(7, 0, 0, 0));

                    IEnumerable<Stats> actualStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
                    IEnumerable<Stats> materialStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
                    IEnumerable<Stats> poStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);

                    List<ExoSubJobProjection> combinedSubJobs = new List<ExoSubJobProjection>();
                    foreach(ExoSubJobProjection exoSubJob in exoSubJobs)
                    {
                        if (!combinedSubJobs.Any(x => x.SubJob.Code == exoSubJob.SubJob.Code && x.Discipline.Code == exoSubJob.Discipline.Code && x.Commodity.Code == exoSubJob.Commodity.Code))
                        {
                            combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = exoSubJob.SubJob.Code }, Discipline = new PrimeroDiscipline() { Code = exoSubJob.Discipline.Code }, Commodity = new PrimeroCommodity() { Code = exoSubJob.Commodity.Code } });
                        }
                    }

                    List<ExoDataPoint> allData = new List<ExoDataPoint>();
                    DetailedData.AddRange(actualStats.SelectMany(x => x.ExoDataPoints));
                    DetailedData.AddRange(materialStats.SelectMany(x => x.ExoDataPoints));
                    DetailedData.AddRange(poStats.SelectMany(x => x.ExoDataPoints));
                    allData.AddRange(DetailedData);
                    //allData.AddRange(poStats.SelectMany(x => x.ExoDataPoints));

                    List<string> uniqueSubjobs = allData.Select(x => x.Subjob_Name + ";" + x.Discipline_Code + ";" + x.Commodity_Code).Distinct().ToList();
                    foreach(string uniqueSubjob in uniqueSubjobs)
                    {
                        List<string> delimited = uniqueSubjob.Split(';').ToList();
                        string subjobName = delimited[0];
                        string disciplineName = delimited[1];
                        string commodityName = delimited[2];

                        if(!combinedSubJobs.Any(x => x.SubJob.Code == subjobName && x.Discipline.Code == disciplineName && x.Commodity.Code == commodityName))
                        {
                            combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subjobName }, Discipline = new PrimeroDiscipline() { Code = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityName } });
                        }
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

                    var groupedSubjobs = combinedSubJobs.GroupBy(x => x.SubJob.Code + x.Discipline.Code).Select(group => new { ProgressDate = group.Key, Projection = group.ToList() });
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
        private void updateDataRowForecast(ExoSubJobProjection disciplineEntity)
        {
            DataRow dataRow;
            if(disciplineEntity.Commodity.Code == string.Empty)
            {
                dataRow = (from DataRow dr in dataPointsTable.Rows
                           where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == disciplineEntity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == disciplineEntity.Discipline.Code
                           select dr).FirstOrDefault();
            }
            else
            {
                dataRow = (from DataRow dr in dataPointsTable.Rows
                           where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == disciplineEntity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == disciplineEntity.Discipline.Code && ((ExoSubJobProjection)dr[columnEntity]).Commodity.Code == disciplineEntity.Commodity.Code
                           select dr).FirstOrDefault();
            }

            if (dataRow == null)
                return;

            updateForecast(dataRow, disciplineEntity, false);
        }

        private DataRow buildDisciplineRowStats(IEnumerable<ExoSubJobProjection> groupedSubJobs)
        {
            if (dataPointsTable == null)
                return null;

            if (groupedSubJobs == null || groupedSubJobs.Count() == 0)
                return null;

            ExoSubJobProjection firstSubJob = groupedSubJobs.First();
            DataRow disciplineDataRow = dataPointsTable.NewRow();
            initializeDataRow(disciplineDataRow, firstSubJob.SubJob.Code, firstSubJob.Discipline.Code, string.Empty);

            DataTable childDataTable = new DataTable();
            childDataTable = dataPointsTable.Clone();
            foreach (var groupedSubjob in groupedSubJobs)
            {
                DataRow cloneRow = childDataTable.NewRow();
                initializeDataRow(cloneRow, groupedSubjob.SubJob.Code, groupedSubjob.Discipline.Code, groupedSubjob.Commodity.Code);
                IEnumerable<DashboardFlatStructure> commodityDashboards = AllProjectDashboards.Where(x => x.SubjobCode == groupedSubjob.SubJob.Code && x.DisciplineCode == groupedSubjob.Discipline.Code && x.CommodityCode == groupedSubjob.Commodity.Code);
                populateDataRow(cloneRow, commodityDashboards);
                updateForecast(cloneRow, groupedSubjob, false);
                calculateUncommitted(cloneRow);
                childDataTable.Rows.Add(cloneRow);
            }

            disciplineDataRow[columnChild] = childDataTable;
            IEnumerable<DashboardFlatStructure> disciplineDashboards = AllProjectDashboards.Where(x => x.SubjobCode == firstSubJob.SubJob.Code && x.DisciplineCode == firstSubJob.Discipline.Code);
            populateDataRow(disciplineDataRow, disciplineDashboards);
            //effectively override remaining
            updateForecast(disciplineDataRow, (ExoSubJobProjection)disciplineDataRow[columnEntity], false);
            calculateUncommitted(disciplineDataRow);
            dataPointsTable.Rows.Add(disciplineDataRow);

            return disciplineDataRow;
        }

        private void initializeDataRow(DataRow dataRow, string subJobCode, string disciplineCode, string commodityCode)
        {
            ExoSubJobProjection entity = new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subJobCode }, Discipline = new PrimeroDiscipline() { Code = disciplineCode }, Commodity = new PrimeroCommodity() { Code = commodityCode } };
            ForecastCalculation calculation = new ForecastCalculation();
            dataRow[columnEntity] = entity;
            dataRow[columnCalculation] = calculation;

            IEnumerable<ExoTimeAuthorisation> relevantJobLines;
            if (entity.Commodity.Code == string.Empty)
                relevantJobLines = jobLines.Where(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code);
            else
                relevantJobLines = jobLines.Where(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code);

            entity.ExoBudgetQty = relevantJobLines.Sum(x => x.BudgetQty);
            entity.ExoBudgetCosts = relevantJobLines.Sum(x => x.BudgetCosts);

            calculation.Budget = entity.ExoBudgetCosts;
            calculation.IsBudgetReadOnly = true;

            //variation is only calculated on discipline code lines
            if (commodityCode == string.Empty)
                calculation.Variation = VARIATION_REGISTERCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.STATUS == VariationRegisterStatus.Approved).Sum(x => x.COST);
            else
                calculation.Variation = 0.00m;

            //populate previous estimate to completion
            IEnumerable<FORECAST> previousEAC = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.IS_EAC && x.FORECAST_DATE < FixedDataDate).OrderBy(x => x.FORECAST_DATE);
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
                            decimal currentValue = remainingDataPoints.Where(x => x.ActualDate.Month == alignedDate.Month && x.ActualDate.Year == alignedDate.Year).Sum(x => x.Costs);
                            if (currentValue != 0)
                            {
                                //decimal currentRowValue = (decimal)dataRow[alignedDateField];
                                dataRow[alignedDateField] = currentValue;
                            }
                        }
                    }

                    //var groupByDateDataPoints = remainingDataPoints.GroupBy(x => new DateTime(x.ProgressDate.Year, x.ProgressDate.Month, 1)).Select(group => new { ProgressDate = group.Key, DataPoints = group.ToList() });
                    //foreach (var groupByDateDataPoint in groupByDateDataPoints)
                    //{
                    //    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date.Month == groupByDateDataPoint.ProgressDate.Date.Month && x.Date.Year == groupByDateDataPoint.ProgressDate.Date.Year);
                    //    if (alignedDataDate != null)
                    //    {
                    //        string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                    //        if (dataPointsTable.Columns.Contains(alignedDateField))
                    //        {
                    //            decimal currentValue = groupByDateDataPoint.DataPoints.Sum(x => x.Costs);
                    //            if (currentValue != 0)
                    //            {
                    //                decimal currentRowValue = (decimal)dataRow[alignedDateField];
                    //                dataRow[alignedDateField] = currentRowValue + currentValue;
                    //            }
                    //        }
                    //    }
                    //}
                }

                IEnumerable<SummaryStats> actualStats = summaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null);
                if (actualStats != null && actualStats.Count() > 0)
                {
                    IEnumerable<ExoDataPoint> actualDataPoints = actualStats.SelectMany(x => x.Actual.ExoDataPoints);
                    forecastCalculation.Actuals += actualDataPoints.Sum(x => x.Costs);
                    forecastCalculation.Invoiced += actualDataPoints.Sum(x => x.InvoiceAmount);
                    foreach(DateTime alignedDate in alignedDataDateCollection)
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

                    //var groupByDateDataPoints = actualDataPoints.GroupBy(x => new DateTime(x.ProgressDate.Year, x.ProgressDate.Month, 1)).Select(group => new { ProgressDate = group.Key, DataPoints = group.ToList() });
                    //foreach (var groupByDateDataPoint in groupByDateDataPoints)
                    //{
                    //    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date.Month == groupByDateDataPoint.ProgressDate.Date.Month && x.Date.Year == groupByDateDataPoint.ProgressDate.Date.Year);
                    //    if (alignedDataDate != null)
                    //    {
                    //        string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                    //        if (dataPointsTable.Columns.Contains(alignedDateField))
                    //        {
                    //            decimal currentValue = groupByDateDataPoint.DataPoints.Sum(x => x.Costs);
                    //            if (currentValue != 0)
                    //            {
                    //                //decimal currentRowValue = (decimal)dataRow[alignedDateField];
                    //                dataRow[alignedDateField] = currentValue;
                    //            }
                    //        }
                    //    }
                    //}
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

                    //var groupByDateDataPoints = materialDataPoints.GroupBy(x => new DateTime(x.ProgressDate.Year, x.ProgressDate.Month, 1)).Select(group => new { ProgressDate = group.Key, DataPoints = group.ToList() });
                    //foreach (var groupByDateDataPoint in groupByDateDataPoints)
                    //{
                    //    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date.Month == groupByDateDataPoint.ProgressDate.Date.Month && x.Date.Year == groupByDateDataPoint.ProgressDate.Date.Year);
                    //    if (alignedDataDate != null)
                    //    {
                    //        if (alignedDataDate < StartSelectionDate)
                    //            StartSelectionDate = (DateTime)alignedDataDate;

                    //        if (alignedDataDate > EndSelectionDate)
                    //            EndSelectionDate = (DateTime)alignedDataDate;

                    //        string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                    //        if (dataPointsTable.Columns.Contains(alignedDateField))
                    //        {
                    //            decimal currentValue = groupByDateDataPoint.DataPoints.Sum(x => x.Costs);
                    //            if (currentValue != 0)
                    //            {
                    //                decimal currentRowValue = (decimal)dataRow[alignedDateField];
                    //                dataRow[alignedDateField] = currentRowValue + currentValue;
                    //            }
                    //        }
                    //    }
                    //}
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
            IEnumerable<FORECAST> currentRowFORECASTS = FORECASTCollectionViewModel.Entities.Where(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && !x.IS_EAC);

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
                        if ((alignedDataDate > FixedDataDate && !isCompare) || (alignedDataDate <= FixedDataDate && isCompare))
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
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'" + " And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                        else
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [IsPO] = 'False'" + " And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    }
                    else
                    {
                        if(entity.Commodity.Code != string.Empty)
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'" + " And [ActualDate] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                        else
                            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [IsPO] = 'False'" + " And [ActualDate] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [ActualDate] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    }

                    IsHidden = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                }
                else if (gridColumn.FieldName.ToUpper().Contains("ACTUALS"))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    if(entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False'");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [IsPO] = 'False'");

                    IsHidden = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                }
                else if (gridColumn.FieldName.ToUpper().Contains("INVOICED"))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    if (entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'False' AND [InvoiceAmount] > 0.0m");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [IsPO] = 'False' AND [InvoiceAmount] > 0.0m");

                    IsHidden = false;
                    this.RaisePropertyChanged(x => x.FilterCriteria);
                }
                else if(gridColumn.FieldName.ToUpper().Contains("OUTSTANDING"))
                {
                    ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView[columnEntity];
                    if (entity.Commodity.Code != string.Empty)
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [Commodity_Code] = '" + entity.Commodity.Code + "' And [IsPO] = 'True'");
                    else
                        FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '" + entity.SubJob.Code + "' And [Discipline_Code] = '" + entity.Discipline.Code + "' And [IsPO] = 'True'");
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
            if(newValueString.Substring(0, 1) == "\t")
            {
                newValueString = newValueString.Substring(1, newValueString.Length - 1);
            }

            string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
            pasteCellData(gridControl, gridTableView, RowData);

            GridControlService.RefreshData();
            e.Handled = true;
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
             if (changedType == typeof(FORECAST))
            {
                FORECAST changedFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
                if (changedFORECAST != null)
                {
                    ExoSubJobProjection findUpdatedEntity = exoSubJobs.FirstOrDefault(x => x.SubJob.Code == changedFORECAST.SUBJOB_CODE && x.Discipline.Code == changedFORECAST.DISCIPLINE_CODE);
                    if(findUpdatedEntity != null)
                    {
                        updateDataRowForecast(findUpdatedEntity);
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

            DataRowView dataRowView = (DataRowView)e.Row;
            ExoSubJobProjection entity = (ExoSubJobProjection)dataRowView.Row[columnEntity];
            EntitiesUndoRedoManager.PauseActionId();
            if (e.Column.FieldName.ToUpper().Contains("ENTITY"))
            {
                //entity.Entity.Entity.PRIMARY_TITLE = e.Value.ToString();
                ///MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, columnPrimaryTitle, e.OldValue, e.Value, EntityMessageType.Changed);
                //MainViewModel.Save(entity);
            }
            else if (e.Column.FieldName.ToUpper() == "CALCULATION.VARIATION")
            {
                decimal newValue = 0;
                if (e.Value != null && decimal.TryParse(e.Value.ToString(), out newValue))
                {
                    VARIATION_REGISTER relevantVariationRegister = VARIATION_REGISTERCollectionViewModel.Entities.FirstOrDefault(x => x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.STATUS == VariationRegisterStatus.Approved);
                    if (relevantVariationRegister == null)
                    {
                        VARIATION_REGISTER newVariationRegister = new VARIATION_REGISTER();
                        newVariationRegister.SUBJOB_CODE = entity.SubJob.Code;
                        newVariationRegister.DISCIPLINE_CODE = entity.Discipline.Code;
                        newVariationRegister.VARIATION_CODE = entity.Variation_Code;
                        newVariationRegister.DESCRIPTION = string.Empty;
                        newVariationRegister.COSTCODE = string.Empty;
                        newVariationRegister.STATUS = VariationRegisterStatus.Approved;
                        newVariationRegister.ORIGINAL_VALUE = 0;
                        newVariationRegister.CURRENT_VALUE = 0;
                        newVariationRegister.COST = newValue;
                        newVariationRegister.GUID_PROJECT = loadPROJECT.GUID;
                        VARIATION_REGISTERCollectionViewModel.Save(newVariationRegister);
                    }
                    else
                    {
                        relevantVariationRegister.COST = newValue;
                    }
                }
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
                    findExistingOrAddNewForecast(dataRowView.Row, entity, dateTime, forecastUnits, false);
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();

            GridControlService.RefreshData();
            e.Handled = true;
        }

        private void findExistingOrAddNewForecast(DataRow dataRow, ExoSubJobProjection entity, DateTime forecastDate, decimal? forecastUnits, bool isRecursive = false)
        {
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && !x.IS_EAC);
            if (findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
                newFORECAST.FORECAST_DATE = forecastDate.Date;
                newFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(newFORECAST);
            }
            else
            {
                findFORECAST.FORECAST_UNITS = forecastUnits;
                FORECASTCollectionViewModel.Save(findFORECAST);
            }

            //used to ensure child row is set
            if(forecastUnits == null)
                dataRow[forecastDate.ToShortDateString()] = 0.00m;
            else
                dataRow[forecastDate.ToShortDateString()] = forecastUnits;

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
                    DataRow disciplineRow = (from DataRow dr in dataPointsTable.Rows
                               where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == entity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == entity.Discipline.Code
                               select dr).FirstOrDefault();

                    if(disciplineRow != null)
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

        private void findExistingOrAddNewEAC(ExoSubJobProjection entity, DateTime forecastDate, decimal eacAmount)
        {
            FORECAST findFORECAST = FORECASTCollectionViewModel.Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.SUBJOB_CODE == entity.SubJob.Code && x.DISCIPLINE_CODE == entity.Discipline.Code && x.COMMODITY_CODE == entity.Commodity.Code && x.IS_EAC);
            if (findFORECAST == null)
            {
                FORECAST newFORECAST = new FORECAST();
                newFORECAST.GUID = Guid.Empty;
                newFORECAST.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST.SUBJOB_CODE = entity.SubJob.Code;
                newFORECAST.DISCIPLINE_CODE = entity.Discipline.Code;
                newFORECAST.COMMODITY_CODE = entity.Commodity.Code;
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
                    if(parseDateTime > FixedDataDate)
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
}