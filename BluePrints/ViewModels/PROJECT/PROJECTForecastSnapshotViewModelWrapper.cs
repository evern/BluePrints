using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class PROJECTForecastSnapshotViewModelWrapper : BluePrintsEntitiesCollectionWrapper<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTForecastSnapshotViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTForecastSnapshotViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTForecastSnapshotViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the PROJECTForecastSnapshotViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTForecastSnapshotViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTForecastSnapshotViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            delayedProjectSaveTimer = new DispatcherTimer();
            delayedProjectSaveTimer.Interval = new TimeSpan(0, 0, 0, 1);
            projectSavingBackgroundWorker.DoWork += ProjectSavingBackgroundWorker_DoWork;
            projectSavingBackgroundWorker.WorkerSupportsCancellation = true;
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        BackgroundWorker projectSavingBackgroundWorker = new BackgroundWorker();
        DispatcherTimer delayedProjectSaveTimer;
        public PROJECT LoadPROJECT { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            LoadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => setProject(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_HOUR_SNAPSHOTS, FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_SUMMARY_SNAPSHOTS, FORECAST_SUMMARY_SNAPSHOTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINE_DESCS, DISCIPLINE_DESCProjectionFunc);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<Data.PHASE, Data.PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == LoadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.COST_TYPE == Common.CostType.Cost);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_SUMMARY_SNAPSHOT>, IQueryable<FORECAST_SUMMARY_SNAPSHOT>> FORECAST_SUMMARY_SNAPSHOTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID).OrderByDescending(x => x.DATA_DATE);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB_HOUR_SNAPSHOT>, IQueryable<FORECAST_JOB_HOUR_SNAPSHOT>> FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.DATA_DATE == FixedDataDate);
        }

        protected virtual Func<IRepositoryQuery<DISCIPLINE_DESC>, IQueryable<DISCIPLINE_DESC>> DISCIPLINE_DESCProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID == LoadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Project Details
        public DateTime FixedDataDateMonthEnd => new DateTime(((DateTime)FixedDataDate).Year, ((DateTime)FixedDataDate).Month, 1).AddMonths(1).AddDays(-1);
        public DateTime? LoadDataDate { get; set; }
        public DateTime FixedDataDate { get; set; }
        public DateTime FixedEndDate { get; set; }
        private void setProject(Data.PROJECT project)
        {
            LoadPROJECT = project;

            DateTime dataDate;
            if (LoadPROJECT.FORECAST_DATA_DATE == null)
            {
                DateTime endOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1);

                LoadPROJECT.FORECAST_DATA_DATE = endOfCurrentMonth;
                dataDate = endOfCurrentMonth;
                LoadDataDate = dataDate;
                savePROJECT();
            }
            else
            {
                dataDate = (DateTime)LoadPROJECT.FORECAST_DATA_DATE;
                LoadDataDate = dataDate;
            }

            FixedDataDate = dataDate;

            DateTime endDate;
            if (LoadPROJECT.FORECAST_END_DATE == null)
                endDate = DateTime.Now.AddMonths(1);
            else
                endDate = (DateTime)LoadPROJECT.FORECAST_END_DATE;

            FixedEndDate = endDate;

            this.RaisePropertiesChanged();
        }

        private void savePROJECT()
        {
            delayedProjectSaveTimer.Tick -= DelayedProjectSaveTimer_Tick;
            delayedProjectSaveTimer.Tick += DelayedProjectSaveTimer_Tick;

            delayedProjectSaveTimer.Start();
        }

        private void DelayedProjectSaveTimer_Tick(object sender, EventArgs e)
        {
            delayedProjectSaveTimer.Stop();
            projectSavingBackgroundWorker.RunWorkerAsync();
        }

        private void ProjectSavingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //when view is closed halfway
            if (PROJECTCollectionViewModel != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => PROJECTCollectionViewModel.Save(LoadPROJECT)));
        }
        #endregion

        #region DataTable Population
        protected string columnEntity = "Entity";
        protected string columnCompare = "CompareEntities";
        DataTable dataPointsTable = null;
        List<ForecastJobSnapshot> Jobs = null;
        protected List<DateTime> alignedDataDateCollection;
        public ForecastSummary ForecastSummary { get; set; }
        public virtual DataTable DataPointsTable
        {
            get
            {
                return dataPointsTable;
            }

        }
        protected override bool loadDataPointsTable()
        {
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);

            dataPointsTable = null;

            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);

            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());
            return true;
        }


        private void updateDataPointsTable()
        {
            dataPointsTable = new DataTable();
            Jobs = new List<ForecastJobSnapshot>();
            GridControlService.GridControl.BeginDataUpdate();


            alignedDataDateCollection = generateDates();
            InitializeColumnSource(ParentViewColumns, ParentSummaries, alignedDataDateCollection, false);
            InitializeColumnSource(ChildViewColumns, ChildSummaries, alignedDataDateCollection, true);

            //data relevant to job
            IEnumerable<string> uniqueWBSNames = FORECAST_JOB_HOUR_SNAPSHOTCollection.Select(x => x.ForecastViewCode).Distinct();
            List<UniqueForecastJob> uniqueForecastJobs = new List<UniqueForecastJob>();
            foreach(string uniqueWBSName in uniqueWBSNames)
            {
                List<string> delimited = uniqueWBSName.Split(';').ToList();
                string subJobCode = delimited[0];
                string disciplineCode = delimited[1];
                string commodityCode = delimited[2];
                string variationCode = delimited[3];

                uniqueForecastJobs.Add(new UniqueForecastJob(subJobCode, disciplineCode, commodityCode, variationCode, FORECAST_JOB_HOUR_SNAPSHOTCollection));
            }

            Common.LoadingScreenManager.ShowLoadingScreen(uniqueForecastJobs.Count());
            Common.LoadingScreenManager.SetMessage("Preparing View...");
            //construct data points table
            dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobSnapshot));
            dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));

            DateTime firstViewDate = alignedDataDateCollection.Count == 0 ? DateTime.Now : alignedDataDateCollection.First();
            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            //child data table is used to record original value of actuals + committed + remaining values before it is overridden by forecasts
            foreach (UniqueForecastJob uniqueForecastJob in uniqueForecastJobs)
            {
                ForecastJobSnapshot forecastSnapshotData = new ForecastJobSnapshot();
                forecastSnapshotData.SubJobCode = uniqueForecastJob.SUBJOB_CODE;
                forecastSnapshotData.DisciplineCode = uniqueForecastJob.DISCIPLINE_CODE;
                forecastSnapshotData.CommodityCode = uniqueForecastJob.COMMODITY_CODE;
                forecastSnapshotData.VariationCode = uniqueForecastJob.VARIATION_CODE;
                forecastSnapshotData.Budget = uniqueForecastJob.Budget;
                forecastSnapshotData.TenderBudget = uniqueForecastJob.TenderBudget;

                populateFallBackRate(forecastSnapshotData, uniqueForecastJob.P6Collection);
                forecastSnapshotData.ActualUnits = uniqueForecastJob.ActualCollection.Sum(x => x.FORECAST_QTY);
                forecastSnapshotData.ActualCosts = uniqueForecastJob.ActualCollection.Sum(x => x.FORECAST_COST);

                foreach (DateTime alignedDataDate in alignedDataDateCollection)
                {
                    ForecastDateSnapshot forecastDateSnapshot = new ForecastDateSnapshot(uniqueForecastJob.AllCollection, firstViewDate, alignedDataDate.Date, FixedDataDate);
                    forecastSnapshotData.DateCosts.Add(forecastDateSnapshot);
                }

                Jobs.Add(forecastSnapshotData);
                DataRow jobRow = updateDataTable(forecastSnapshotData);
                Common.LoadingScreenManager.Progress();
            }

            GridControlService.GridControl.EndDataUpdate();
            Common.LoadingScreenManager.CloseLoadingScreen();

            //ForecastSummary.Reset();

            //calculate project summary, needs to be done after uncommitted is calculated
            //ForecastSummary.Budget_Cost = commodityJobs.Sum(x => x.Budget);
            //ForecastSummary.Current_Cost = commodityJobs.Sum(x => x.ActualCosts);
            //ForecastSummary.Commitments = commodityJobs.Sum(x => x.Outstanding);
            //ForecastSummary.Uncommitted_Forecast = commodityJobs.Sum(x => x.Uncommitted);
            //ForecastSummary.OriginalEstimateAtCompletion = commodityJobs.Sum(x => x.OriginalEstimateAtCompletion);
            //ForecastSummary.EstimateAtCompletion = commodityJobs.Sum(x => x.EstimateAtCompletion);
            //ForecastSummary.CurrentEstimateAtCompletion = commodityJobs.Sum(x => x.CurrentEstimateAtCompletion);
            //ForecastSummary.Contingency = commodityJobs.Where(x => x.IsContingency).Sum(x => x.EstimateAtCompletion);

            this.RaisePropertyChanged(x => x.ForecastSummary);
        }

        private void populateFallBackRate(ForecastJobSnapshot forecastSnapshotData, IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> P6FORECAST_JOB_HOUR_SNAPSHOTS)
        {
            forecastSnapshotData.P6RemainingUnits = P6FORECAST_JOB_HOUR_SNAPSHOTS.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.P6Original).Sum(x => x.FORECAST_QTY);
            forecastSnapshotData.P6RemainingCosts = P6FORECAST_JOB_HOUR_SNAPSHOTS.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.P6Original).Sum(x => x.FORECAST_COST);

            Data.PHASE ratePHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM == forecastSnapshotData.PhaseCode);
            if (ratePHASE != null && forecastSnapshotData.DisciplineCode != null && forecastSnapshotData.DisciplineCode != string.Empty)
            {
                forecastSnapshotData.FallBackRate = BluePrintsDataUtils.CascadeRateSearchByCode(forecastSnapshotData.AreaCode, forecastSnapshotData.SubAreaCode, forecastSnapshotData.DisciplineCode, null, forecastSnapshotData.CommodityCode, forecastSnapshotData.VariationCode, RATECollection, Common.CostType.Cost, (PhaseType)ratePHASE.PHASE_TYPE);
            }
        }

        private DataRow updateDataTable(ForecastJobSnapshot job)
        {
            DataRow commodityRow = dataPointsTable.NewRow();
            commodityRow[columnEntity] = job;

            DataTable compareDataTable;
            //DataRow compareActualsRow;
            //DataRow compareMaterialRow;
            DataRow compareP6CostsRemainingRow;
            DataRow compareP6UnitsRemainingRow;
            DataTable compareChildDataTable;
            DataRow compareChildP6CostsRemainingRow;
            DataRow compareChildP6UnitsRemainingRow;

            compareDataTable = dataPointsTable.Clone();
            compareDataTable.TableName = BluePrintsResources.ForecastCompareTableName;
            compareP6CostsRemainingRow = compareDataTable.NewRow();
            compareP6UnitsRemainingRow = compareDataTable.NewRow();

            compareP6UnitsRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = "P6 Hours", DateCosts = job.DateCosts, IsP6HoursRow = true });
            compareP6CostsRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = "P6 $", CompareMask = "c0" });

            //update discipline desc
            job.PopulateDisciplineDesc(DISCIPLINE_DESCCollection, JOB_COSTGROUPCollection);

            compareChildDataTable = dataPointsTable.Clone();
            compareChildDataTable.TableName = BluePrintsResources.ForecastCompareChildTableName;
            compareChildP6CostsRemainingRow = compareChildDataTable.NewRow();
            compareChildP6UnitsRemainingRow = compareChildDataTable.NewRow();

            compareChildDataTable.Rows.Add(compareChildP6UnitsRemainingRow);
            compareChildDataTable.Rows.Add(compareChildP6CostsRemainingRow);

            compareP6UnitsRemainingRow[columnCompare] = compareChildDataTable;
            //compareDataTable.Rows.Add(compareActualsRow);
            //compareDataTable.Rows.Add(compareMaterialRow);
            compareDataTable.Rows.Add(compareP6UnitsRemainingRow);
            compareDataTable.Rows.Add(compareP6CostsRemainingRow);

            //add uncommitted row irregardless, needs to be added here because it's always the third row
            DataRow compareUncommittedRow = compareDataTable.NewRow();
            compareUncommittedRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_UncommittedRowPhase + " $", CompareMask = "c0" });
            compareDataTable.Rows.Add(compareUncommittedRow);

            //create rows based on unique codes for each type
            Dictionary<string, DataRow> poForecastRows = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> indirectForecastRows = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> materialForecastRows = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> actualForecastRows = new Dictionary<string, DataRow>();

            //add PO forecast rows on demand
            foreach (KeyValuePair<string, decimal> uniquePOStockCodeAttrbutes in job.POStockCodeAttributes)
            {
                DataRow comparePOForecastRow = compareDataTable.NewRow();
                comparePOForecastRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_PORowPhase + " [" + uniquePOStockCodeAttrbutes + "] $", CompareMask = "c0", DropDownIndirectBudget = uniquePOStockCodeAttrbutes.Value });
                poForecastRows.Add(uniquePOStockCodeAttrbutes.Key, comparePOForecastRow);
                compareDataTable.Rows.Add(comparePOForecastRow);
            }

            //add indirect rows on demand
            foreach (KeyValuePair<string, decimal> uniqueIndirectStockCode in job.IndirectStockCodeAttributes)
            {
                DataRow compareIndirectRemainingRow = compareDataTable.NewRow();
                compareIndirectRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_IndirectRowPhase + " [" + uniqueIndirectStockCode + "] $", DropDownIndirectBudget = uniqueIndirectStockCode.Value, CompareMask = "c0" });
                indirectForecastRows.Add(uniqueIndirectStockCode.Key, compareIndirectRemainingRow);
                compareDataTable.Rows.Add(compareIndirectRemainingRow);
            }

            //add material rows on demand
            foreach (KeyValuePair<string, decimal> uniqueMaterialStockCode in job.MaterialStockCodeAttributes)
            {
                DataRow compareMaterialRemainingRow = compareDataTable.NewRow();
                compareMaterialRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_MaterialRowPhase + " [" + uniqueMaterialStockCode + "] $", DropDownIndirectBudget = uniqueMaterialStockCode.Value, CompareMask = "c0" });
                materialForecastRows.Add(uniqueMaterialStockCode.Key, compareMaterialRemainingRow);
                compareDataTable.Rows.Add(compareMaterialRemainingRow);
            }

            //add actual rows on demand
            foreach (KeyValuePair<string, decimal> uniqueActualStockCode in job.ActualStockCodeAttributes)
            {
                DataRow compareActualRemainingRow = compareDataTable.NewRow();
                compareActualRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_ActualRowPhase + " [" + uniqueActualStockCode + "] $", DropDownIndirectBudget = uniqueActualStockCode.Value, CompareMask = "c0" });
                actualForecastRows.Add(uniqueActualStockCode.Key, compareActualRemainingRow);
                compareDataTable.Rows.Add(compareActualRemainingRow);
            }

            //add the compare data table into a single column in parent row
            commodityRow[columnCompare] = compareDataTable;
            dataPointsTable.Rows.Add(commodityRow);
            decimal P6TotalCurrentRemainingUnits = 0;

            List<FORECAST> relevantFORECASTS = FORECASTCollection.Where(x => x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode).ToList();
            foreach (ForecastDateSnapshot dateCost in job.DateCosts)
            {
                foreach (FORECAST_JOB_HOUR_SNAPSHOT poForecastSnapshot in dateCost.POForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow poForecastRow = poForecastRows.First(x => x.Key == poForecastSnapshot.STOCK_CODE).Value;
                    poForecastRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = poForecastSnapshot.FORECAST_COST;
                }

                foreach (FORECAST_JOB_HOUR_SNAPSHOT indirectForecastSnapshot in dateCost.IndirectForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow indirectForecastRow = indirectForecastRows.First(x => x.Key == indirectForecastSnapshot.STOCK_CODE).Value;
                    indirectForecastRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = indirectForecastSnapshot.FORECAST_COST;
                }

                foreach (FORECAST_JOB_HOUR_SNAPSHOT actualForecastSnapshot in dateCost.ActualForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow actualForecastRow = actualForecastRows.First(x => x.Key == actualForecastSnapshot.STOCK_CODE).Value;
                    actualForecastRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = actualForecastSnapshot.FORECAST_COST;
                }

                foreach (FORECAST_JOB_HOUR_SNAPSHOT materialForecastSnapshot in dateCost.MaterialForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow materialForecastRow = materialForecastRows.First(x => x.Key == materialForecastSnapshot.STOCK_CODE).Value;
                    materialForecastRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = materialForecastSnapshot.FORECAST_COST;
                }

                //retrieve original p6 values
                compareChildP6CostsRemainingRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Costs;
                compareChildP6UnitsRemainingRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Hours;

                List<FORECAST> forecastOverrides = relevantFORECASTS.Where(x => x.FORECAST_UNITS != null && x.FORECAST_DATE >= dateCost.MonthStartDate && x.FORECAST_DATE <= dateCost.MonthEndDate).ToList();
                List<FORECAST> forecastCostsOverrides = forecastOverrides.Where(x => x.FORECAST_TYPE == ForecastDataType.Cost).ToList();
                List<FORECAST> forecastUnitsOverrides = forecastOverrides.Where(x => x.FORECAST_TYPE == ForecastDataType.P6).ToList();
                List<FORECAST> forecastJobHourOverrides = forecastOverrides.Where(x => x.FORECAST_TYPE == ForecastDataType.Hour).ToList();
                List<FORECAST> forecastHistory = forecastOverrides.Where(x => x.FORECAST_TYPE == ForecastDataType.DataDateForecast).ToList();

                //skip when date is actual date
                if (forecastUnitsOverrides.Count > 0 && dateCost != job.DateCosts.First())
                {
                    decimal p6OverrideUnits = forecastUnitsOverrides.Sum(x => (decimal)x.FORECAST_UNITS);

                    compareP6UnitsRemainingRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = p6OverrideUnits;
                    compareP6CostsRemainingRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = p6OverrideUnits * job.P6NominalRate;
                    P6TotalCurrentRemainingUnits += p6OverrideUnits;
                }
                else
                {
                    compareP6UnitsRemainingRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Hours;
                    compareP6CostsRemainingRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Costs;
                    P6TotalCurrentRemainingUnits += dateCost.P6Hours;
                }

                commodityRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.TotalCosts;
            }

            job.P6RemainingUnitsOverride = P6TotalCurrentRemainingUnits;
            //updateViewForecastsOnDatesFromDb(commodityRow, false, relevantFORECASTS);
            return commodityRow;
        }

        /// <summary>
        /// Updates the view with forecast values from db for a single row
        /// </summary>
        private void updateViewForecastsOnDatesFromDb(DataRow dataRow, bool searchParentRow = false, List<FORECAST> relevantFORECASTS = null)
        {
            ForecastJobSnapshot job = (ForecastJobSnapshot)dataRow[columnEntity];
            //need to map back into main row because datarow could be coming from p6 hours edit
            DataRow parentRow = searchParentRow ? findRow(job, true) : dataRow;
            job = (ForecastJobSnapshot)parentRow[columnEntity];
            DataTable compareDataTable = (DataTable)parentRow[columnCompare];
            DataRow p6CostRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6CostRowIndex)];
            DataRow p6HoursRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRowIndex)];
            DataRow uncommittedCostRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_UncommittedRowIndex)];

            DataTable childCompareDataTable = (DataTable)p6HoursRow[columnCompare];
            DataRow childCompareP6CostsRow = childCompareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6CostRowIndex)];

            List<FORECAST> currentRowFORECASTS = relevantFORECASTS != null ? relevantFORECASTS : FORECASTCollection.Where(x => x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode).ToList();

            decimal P6CurrentRemainingUnits = 0;
            foreach (ForecastDateSnapshot dateCost in job.DateCosts)
            {
                DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= dateCost.QueryDate);
                if (alignedDataDate != null)
                {
                    string alignedDateField = ((DateTime)alignedDataDate).ToString(BluePrintsResources.ColumnDateFormat);
                    //put forecast history only on compare datatable
                    if (alignedDataDate > FixedDataDateMonthEnd)
                    {
                        if (dataPointsTable.Columns.Contains(alignedDateField))
                        {
                            IEnumerable<FORECAST> currentRowDateFORECAST = currentRowFORECASTS.Where(x => x.FORECAST_UNITS != null && x.FORECAST_TYPE == ForecastDataType.Cost && x.FORECAST_DATE >= dateCost.MonthStartDate && x.FORECAST_DATE <= dateCost.MonthEndDate);
                            IEnumerable<FORECAST> currentRowP6OverrideFORECAST = currentRowFORECASTS.Where(x => x.FORECAST_UNITS != null && x.FORECAST_TYPE == ForecastDataType.P6 && x.FORECAST_DATE >= dateCost.MonthStartDate && x.FORECAST_DATE <= dateCost.MonthEndDate);

                            decimal currentP6Units = (decimal)p6HoursRow[alignedDateField];
                            P6CurrentRemainingUnits += currentP6Units;
                            decimal overrideCostOnDataDate = 0;
                            if (currentRowDateFORECAST.Count() > 0)
                            {
                                overrideCostOnDataDate = currentRowDateFORECAST.Sum(x => (decimal)x.FORECAST_UNITS);
                            }
                            else
                            {
                                overrideCostOnDataDate = getMasterRowResetValue(compareDataTable, alignedDateField);
                            }

                            parentRow[alignedDateField] = overrideCostOnDataDate;

                            if (currentRowP6OverrideFORECAST.Count() > 0)
                            {
                                p6CostRow[alignedDateField] = overrideCostOnDataDate;
                                uncommittedCostRow[alignedDateField] = 0;
                            }
                            else
                                uncommittedCostRow[alignedDateField] = overrideCostOnDataDate - dateCost.P6Costs - dateCost.MaterialCosts - dateCost.ActualCosts - dateCost.IndirectForecastCosts - dateCost.POForecastCosts;
                        }
                    }
                }
            }

            job.P6RemainingUnitsOverride = P6CurrentRemainingUnits;

            if (job.P6RemainingUnitsOverride != null && job.P6RemainingUnitsOverride != 0 && job.P6RemainingUnits != 0)
                job.Productivity = job.P6RemainingUnits / (decimal)job.P6RemainingUnitsOverride;
            else
                job.Productivity = 0.00m;
        }

        private DataRow findRow(ForecastJobSnapshot entity, bool searchCommodityLevel)
        {
            IEnumerable<DataRow> subjobDisciplineRows = (from DataRow dr in dataPointsTable.Rows
                                                         where ((ForecastJobSnapshot)dr[columnEntity]).SubJobCode == entity.SubJobCode && (((ForecastJobSnapshot)dr[columnEntity])).DisciplineCode == entity.DisciplineCode
                                                         select dr);

            IEnumerable<DataRow> variationRows;
            if (entity.VariationCode == string.Empty || entity.VariationCode == null)
                variationRows = subjobDisciplineRows.Where(x => ((ForecastJobSnapshot)x[columnEntity]).VariationCode == string.Empty || (((ForecastJobSnapshot)x[columnEntity])).VariationCode == null);
            else
                variationRows = subjobDisciplineRows.Where(x => ((ForecastJobSnapshot)x[columnEntity]).VariationCode == entity.VariationCode);

            if (searchCommodityLevel)
                return variationRows.FirstOrDefault(x => ((ForecastJobSnapshot)x[columnEntity]).CommodityCode == entity.CommodityCode);
            else
                return variationRows.FirstOrDefault();
        }

        private decimal getMasterRowResetValue(DataTable compareDataTable, string dateFieldName)
        {
            if (compareDataTable != null && compareDataTable.Rows.Count > 0)
            {
                if (compareDataTable.Columns.Contains(dateFieldName))
                {
                    decimal totalValue = 0;
                    if (compareDataTable.TableName == BluePrintsResources.ForecastCompareChildTableName)
                    {
                        //when delete button is pressed on the P6 units cell
                        DataRow compareP6HoursRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRowIndex)];
                        totalValue = compareP6HoursRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareP6HoursRemainingRow[dateFieldName];
                    }
                    else
                    {
                        List<DataRow> costRows = (from DataRow costRow in compareDataTable.Rows
                                                  let job = (ForecastJobSnapshot)costRow[columnEntity]
                                                  let dropDownPhase = job.DropDownPhase
                                                  where dropDownPhase.Contains(BluePrintsResources.ForecastCompare_PORowPhase) || dropDownPhase.Contains(BluePrintsResources.ForecastCompare_IndirectRowPhase)
                                                  || dropDownPhase.Contains(BluePrintsResources.ForecastCompare_MaterialRowPhase) || dropDownPhase.Contains(BluePrintsResources.ForecastCompare_IndirectRowPhase)
                                                  select costRow).ToList();

                        DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRowIndex)];

                        DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                        DataRow compareChildP6CostsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6CostRowIndex)];
                        DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRowIndex)];

                        decimal p6CostValue = compareChildP6CostsRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareChildP6CostsRemainingRow[dateFieldName];
                        decimal dynamicCostsFromCostRows = 0;
                        foreach (DataRow costRow in costRows)
                        {
                            string parseDecimalStr = costRow[dateFieldName].ToString();
                            if(parseDecimalStr != string.Empty)
                            {
                                decimal parseDecimal = 0;
                                if (decimal.TryParse(parseDecimalStr, out parseDecimal))
                                    dynamicCostsFromCostRows += parseDecimal;
                            }
                        }

                        totalValue = p6CostValue + dynamicCostsFromCostRows;
                    }

                    return totalValue;
                }
            }

            return 0.00m;
        }

        private List<DateTime> generateDates()
        {
            List<FORECAST_JOB_HOUR_SNAPSHOT> snapShots = FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.FORECAST_DATE != null).ToList();
            DateTime endDateToGenerate = snapShots.Count == 0 ? DateTime.Now.AddMonths(1) : snapShots.Max(x => (DateTime)x.FORECAST_DATE);
            DateTime firstDateToGenerateFrom = new DateTime();
            firstDateToGenerateFrom = FixedDataDate;

            return ChronologicalHelpers.GenerateEndDatesCollection(firstDateToGenerateFrom, endDateToGenerate);
        }
        #endregion

        #region View Definition
        protected ObservableCollection<ColumnDescriptor> parentViewColumns;
        public ObservableCollection<ColumnDescriptor> ParentViewColumns
        {
            get
            {
                if (parentViewColumns == null)
                {
                    parentViewColumns = new ObservableCollection<ColumnDescriptor>();
                }
                return parentViewColumns;
            }
        }

        protected ObservableCollection<ColumnDescriptor> childViewColumns;
        public ObservableCollection<ColumnDescriptor> ChildViewColumns
        {
            get
            {
                if (childViewColumns == null)
                {
                    childViewColumns = new ObservableCollection<ColumnDescriptor>();
                }
                return childViewColumns;
            }
        }

        protected ObservableCollection<SummaryDescriptor> parentSummaries;
        public ObservableCollection<SummaryDescriptor> ParentSummaries
        {
            get
            {
                if (parentSummaries == null)
                {
                    parentSummaries = new ObservableCollection<SummaryDescriptor>();
                }
                return parentSummaries;
            }
        }

        protected ObservableCollection<SummaryDescriptor> childSummaries;
        public ObservableCollection<SummaryDescriptor> ChildSummaries
        {
            get
            {
                if (childSummaries == null)
                {
                    childSummaries = new ObservableCollection<SummaryDescriptor>();
                }
                return childSummaries;
            }
        }

        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates, bool isChild)
        {
            columns.Clear();
            summaries.Clear();

            if (!isChild)
            {
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PhaseCode", ReadOnly = true, Header = "Phase", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.SubJobCode", ReadOnly = true, Header = "Subjob", Fixed = FixedStyle.Left, Width = 110, Settings = SettingsType.JobError });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.AreaCode", ReadOnly = true, Visible = false, Header = "Area", Fixed = FixedStyle.Left, Width = 60, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DisciplineCode", ReadOnly = true, Header = "Discipline", Fixed = FixedStyle.Left, Width = 38, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DisciplineDesc", ReadOnly = true, Header = "Package", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.CommodityCode", ReadOnly = true, Header = "Commodity", Fixed = FixedStyle.Left, Width = 35, Settings = SettingsType.CommodityCode });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.VariationCode", ReadOnly = true, Header = "Variation", Fixed = FixedStyle.Left, Width = 60, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.TenderBudget", ReadOnly = false, Header = "Tender Budget (H)", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Budget, Mask = "c0", HeaderToolTip = "Budget saved here during Roll Over" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.TenderBudget", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Budget", ReadOnly = false, Header = "Project Budget (A)", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Budget, HeaderToolTip = "EAC saved here during Roll Over" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Budget", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualUnits", ReadOnly = true, Header = "Actual Units", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n0", HeaderToolTip = "Actual units to date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualUnits", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualCosts", ReadOnly = true, Header = "Actual Costs (B)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Costs burned to Date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualCosts", DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
            else
            {
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DropDownPhase", Header = "", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default, HeaderToolTip = "Source of forecasted costs/hours type" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DropDownIndirectBudget", ReadOnly = true, Header = "Budget (A)", Increment = 1, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Budget, HeaderToolTip = "Indirect budget from Exo" });
            }

            foreach (DateTime alignedDate in alignedDates)
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                if (alignedDate > FixedDataDateMonthEnd)
                {
                    if (isChild)
                        columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastChild });
                    else
                        columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });

                    if (!isChild)
                        summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
                }
            }
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(PROJECT projection, PROJECT entity, bool isNewEntity)
        {
        }

        public override string UnifiedRowValidation(PROJECT projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PROJECT projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTForecastSnapshotViewModelWrapper_v2"; }
        }

        public async void RefreshAllForecastData()
        {
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            //Common.LoadingScreenManager.SetMessage("Fetching P6 remaining data...");
            //await BluePrintsContextHelper.RefreshDeliverablesRemainingDataPointsByProject(LoadPROJECT.NUMBER, true);

            //Common.LoadingScreenManager.SetMessage("Fetching P6 planned data...");
            //await BluePrintsContextHelper.RefreshDeliverablesPlannedDataPointsByProject(LoadPROJECT.NUMBER, true);

            Common.LoadingScreenManager.SetMessage("Updating actuals, indirect, P6 and PO data...");
            BluePrintsContextHelper.RefreshAllForecastData(LoadPROJECT.NUMBER, FixedDataDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            FullRefresh();
        }

        public override void FullRefresh()
        {
            alignedDataDateCollection.Clear();
            //ForecastSummary.Reset();
            base.FullRefresh();
        }

        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> FORECAST_JOB_HOUR_SNAPSHOTCollection
        {
            get
            {
                return GetEntities<FORECAST_JOB_HOUR_SNAPSHOT>();
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                return GetEntities<Data.PHASE>();
            }
        }

        public IQueryable<FORECAST> FORECASTCollection
        {
            get
            {
                return bluePrintsUnitOfWork.FORECASTS.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<DISCIPLINE_DESC> DISCIPLINE_DESCCollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE_DESC>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROJECT>();
            }
        }
        #endregion
    }

    public class UniqueForecastJob
    {
        public UniqueForecastJob(string subJobCode, string disciplineCode, string commodityCode, string variationCode, IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> FORECAST_JOB_HOURByDataDateCollection)
        {
            SUBJOB_CODE = subJobCode;
            DISCIPLINE_CODE = disciplineCode;
            COMMODITY_CODE = commodityCode;
            VARIATION_CODE = variationCode;

            IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> filteredForecastJobHourSnapshot = FORECAST_JOB_HOURByDataDateCollection.Where(x => x.SUBJOB_CODE == subJobCode && x.DISCIPLINE_CODE == disciplineCode && x.COMMODITY_CODE == commodityCode && x.VARIATION_CODE == variationCode);
            AllCollection = filteredForecastJobHourSnapshot.ToList();
            BudgetCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Budget).ToList();
            ActualCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual).ToList();
            P6Collection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.P6Original).ToList();
        }

        public string SUBJOB_CODE { get; set; }
        public string DISCIPLINE_CODE { get; set; }
        public string COMMODITY_CODE { get; set; }
        public string VARIATION_CODE { get; set; }
        public decimal Budget
        {
            get
            {
                if (BudgetCollection.Count == 0)
                    return 0;

                return BudgetCollection.First().PROJECT_BUDGET;
            }
        }

        public decimal TenderBudget
        {
            get
            {
                if (BudgetCollection.Count == 0)
                    return 0;

                return BudgetCollection.First().TENDER_BUDGET;
            }
        }

        public List<FORECAST_JOB_HOUR_SNAPSHOT> AllCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> BudgetCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> ActualCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> P6Collection { get; set; }

        public FORECAST_JOB_HOUR_SNAPSHOT ForecastJob
        {
            get
            {
                if (BudgetCollection.Count > 0)
                    return BudgetCollection.First();
                else if (ActualCollection.Count > 0)
                    return ActualCollection.First();
                else //this property can only be constructed by either of these 3 category so it must exist in P6 collection
                    return P6Collection.First();
            }
        }

        public string ErrorMessage
        {
            get
            {
                bool isExistInBudget = BudgetCollection.Count > 0;
                bool isExistInActuals = ActualCollection.Count > 0;
                bool isExistInRemaining = P6Collection.Count > 0;
                string possibleErrorMessage = string.Empty;
                if(!isExistInBudget)
                {
                    if (isExistInActuals && isExistInRemaining)
                        possibleErrorMessage = "Job have actuals and remaining costs";
                    else if (isExistInActuals)
                        possibleErrorMessage = "Job have actuals";
                    else if (isExistInRemaining)
                        possibleErrorMessage = "Job have remaining costs";
                }

                return possibleErrorMessage;
            }
        }
    }
}