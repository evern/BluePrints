using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
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
using System.Data;
using System.Linq;

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
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private PROJECT loadPROJECT { get; set; }
        public DateTime DataDate { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_HOUR_SNAPSHOTS, FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_SUMMARY_SNAPSHOTS, FORECAST_SUMMARY_SNAPSHOTProjectionFunc, x => setDataDate(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINE_DESCS, DISCIPLINE_DESCProjectionFunc);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_SUMMARY_SNAPSHOT>, IQueryable<FORECAST_SUMMARY_SNAPSHOT>> FORECAST_SUMMARY_SNAPSHOTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderByDescending(x => x.DATA_DATE);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB_HOUR_SNAPSHOT>, IQueryable<FORECAST_JOB_HOUR_SNAPSHOT>> FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<DISCIPLINE_DESC>, IQueryable<DISCIPLINE_DESC>> DISCIPLINE_DESCProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private void setDataDate(FORECAST_SUMMARY_SNAPSHOT forecastSummarySnapshot)
        {
            if(forecastSummarySnapshot == null)
            {
                MessageBoxService.ShowMessage("Snapshot isn't saved yet, please open up forecast and save a snapshot");
                return;
            }

            DataDate = forecastSummarySnapshot.DATA_DATE;
            this.RaisePropertiesChanged();
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
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
            IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> forecastJobSnapshotJobCollection = FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.Job && x.DATA_DATE == DataDate);

            //data relevant to hours
            IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> forecastJobSnapshotHoursCollection = FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.SNAPSHOT_TYPE != Common.ForecastSnapshotValueType.Job && x.DATA_DATE == DataDate);

            LoadingScreenManager.ShowLoadingScreen(forecastJobSnapshotJobCollection.Count());
            LoadingScreenManager.SetMessage("Preparing View...");
            //construct data points table
            dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobSnapshot));
            dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));
            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            //child data table is used to record original value of actuals + committed + remaining values before it is overridden by forecasts
            foreach (FORECAST_JOB_HOUR_SNAPSHOT forecastJobSnapshot in forecastJobSnapshotJobCollection)
            {
                ForecastJobSnapshot forecastSnapshotData = new ForecastJobSnapshot();
                forecastSnapshotData.SubJobCode = forecastJobSnapshot.SUBJOB_CODE;
                forecastSnapshotData.DisciplineCode = forecastJobSnapshot.DISCIPLINE_CODE;
                forecastSnapshotData.CommodityCode = forecastJobSnapshot.COMMODITY_CODE;
                forecastSnapshotData.TenderBudget = forecastJobSnapshot.TENDER_BUDGET;
                forecastSnapshotData.Budget = forecastJobSnapshot.PROJECT_BUDGET;

                IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> relevantFORECAST_JOB_HOUR_SNAPSHOTS = forecastJobSnapshotHoursCollection.Where(x => x.SUBJOB_CODE == forecastJobSnapshot.SUBJOB_CODE && x.DISCIPLINE_CODE == forecastJobSnapshot.DISCIPLINE_CODE && x.COMMODITY_CODE == forecastJobSnapshot.COMMODITY_CODE && x.VARIATION_CODE == forecastJobSnapshot.VARIATION_CODE);
                foreach (DateTime alignedDataDate in alignedDataDateCollection)
                {
                    IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> currentMonthFORECAST_JOB_HOUR_SNAPSHOTS = relevantFORECAST_JOB_HOUR_SNAPSHOTS.Where(x => x.FORECAST_DATE != null && ((DateTime)x.FORECAST_DATE).Date == alignedDataDate.Date);
                    ForecastDateSnapshot forecastDateSnapshot = new ForecastDateSnapshot(relevantFORECAST_JOB_HOUR_SNAPSHOTS, alignedDataDate.Date);
                    forecastSnapshotData.DateCosts.Add(forecastDateSnapshot);
                }

                Jobs.Add(forecastSnapshotData);
                DataRow jobRow = updateDataTable(forecastSnapshotData);
                LoadingScreenManager.Progress();
            }

            GridControlService.GridControl.EndDataUpdate();
            LoadingScreenManager.CloseLoadingScreen();

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

            foreach (ForecastDateSnapshot dateCost in job.DateCosts)
            {
                foreach (FORECAST_JOB_HOUR_SNAPSHOT poForecastSnapshot in dateCost.POForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow poForecastRow = poForecastRows.First(x => x.Key == poForecastSnapshot.STOCK_CODE).Value;
                    poForecastRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = poForecastSnapshot.FORECAST_COST;
                }

                foreach (FORECAST_JOB_HOUR_SNAPSHOT indirectForecastSnapshot in dateCost.IndirectForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow indirectForecastRow = indirectForecastRows.First(x => x.Key == indirectForecastSnapshot.STOCK_CODE).Value;
                    indirectForecastRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = indirectForecastSnapshot.FORECAST_COST;
                }

                foreach (FORECAST_JOB_HOUR_SNAPSHOT actualForecastSnapshot in dateCost.ActualForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow actualForecastRow = actualForecastRows.First(x => x.Key == actualForecastSnapshot.STOCK_CODE).Value;
                    actualForecastRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = actualForecastSnapshot.FORECAST_COST;
                }

                foreach (FORECAST_JOB_HOUR_SNAPSHOT materialForecastSnapshot in dateCost.MaterialForecastSnapshots)
                {
                    //finds the unique row based on stock code
                    DataRow materialForecastRow = materialForecastRows.First(x => x.Key == materialForecastSnapshot.STOCK_CODE).Value;
                    materialForecastRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = materialForecastSnapshot.FORECAST_COST;
                }

                //retrieve original p6 values
                compareChildP6CostsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Costs;
                compareChildP6UnitsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.P6Quantities;

                //retrieve p6 values that can be either original or edited
                if(dateCost.P6OverrideCost == null)
                    compareP6CostsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = DBNull.Value;
                else
                    compareP6CostsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = (decimal)dateCost.P6OverrideCost;

                if(dateCost.P6OverrideQuantity == null)
                    compareP6UnitsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = DBNull.Value;
                else
                    compareP6UnitsRemainingRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = (decimal)dateCost.P6OverrideQuantity;


                commodityRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.TotalCosts;
                if (dateCost.ViewOverrideCost == null)
                    compareUncommittedRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = DBNull.Value;
                else
                    compareUncommittedRow[dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat)] = (decimal)dateCost.ViewOverrideCost;
            }

            return commodityRow;
        }

        private List<DateTime> generateDates()
        {
            DateTime endDateToGenerate = FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.FORECAST_DATE != null).Max(x => (DateTime)x.FORECAST_DATE);
            DateTime firstDateToGenerateFrom = new DateTime();
            firstDateToGenerateFrom = DataDate;

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
            }
            else
            {
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DropDownPhase", Header = "", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default, HeaderToolTip = "Source of forecasted costs/hours type" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DropDownIndirectBudget", ReadOnly = true, Header = "Budget (A)", Increment = 1, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Budget, HeaderToolTip = "Indirect budget from Exo" });
            }

            foreach (DateTime alignedDate in alignedDates)
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                if (isChild)
                    columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastChild });
                else
                    columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });

                if (!isChild)
                    summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
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
        #endregion
    }
}