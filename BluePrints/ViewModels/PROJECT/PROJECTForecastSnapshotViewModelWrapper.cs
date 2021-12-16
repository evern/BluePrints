using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.View;
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
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class PROJECTForecastSnapshotViewModelWrapper : BluePrintsEntitiesCollectionWrapper<Data.PROJECT, Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>
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
        protected PROJECTForecastSnapshotViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            ForecastSummary = new ForecastSummary();
            delayedProjectSaveTimer = new DispatcherTimer();
            delayedProjectSaveTimer.Interval = new TimeSpan(0, 0, 0, 1);
            projectSavingBackgroundWorker.DoWork += ProjectSavingBackgroundWorker_DoWork;
            projectSavingBackgroundWorker.WorkerSupportsCancellation = true;

            delayedUpdateFloatingProjectSummaryTimer = new DispatcherTimer();
            delayedUpdateFloatingProjectSummaryTimer.Interval = new TimeSpan(0, 0, 0, 1);

            delayedGridUpdateTimer = new DispatcherTimer();
            delayedGridUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            P6ErrorIconName = "Warning";
            P6ErrorMessage = "P6 Data Date is less than data date, please change data date in P6 and press 'Refresh P6' so that PF is accurate";
            IsHidden = true;
            canEditConstructionUncommitted = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_ConstructionUncommitted)) == LoginCredentials.PermissionStatus.All;

            dataModellingBackgroundWorker = new BackgroundWorker();
            dataModellingBackgroundWorker.DoWork += dataModellingBackgroundWorker_DoWork; ;
            dataModellingBackgroundWorker.RunWorkerCompleted += dataModellingBackgroundWorker_RunWorkerCompleted; ;
            dataModellingBackgroundWorker.WorkerSupportsCancellation = true;
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        InstantFeedbackActualDetailsCollectionViewModelWrapper instantFeedbackActualDetailViewModel = InstantFeedbackActualDetailsCollectionViewModelWrapper.Create();
        IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        BackgroundWorker projectSavingBackgroundWorker = new BackgroundWorker();
        DispatcherTimer delayedProjectSaveTimer;
        DispatcherTimer delayedGridUpdateTimer;
        DispatcherTimer delayedUpdateFloatingProjectSummaryTimer;
        BackgroundWorker dataModellingBackgroundWorker;
        public Data.PROJECT LoadPROJECT { get; set; }
        bool isWeeks;
        public bool IsWeeks
        {
            get => isWeeks;
            set
            {
                if (isWeeks != value)
                {
                    isWeeks = value;
                    ForecastSummary.Reset();
                    EntitiesUndoRedoManager.Clear();
                    mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
                }
            }
        }

        bool isShowActualsHistory;
        public bool IsShowActualsHistory
        {
            get => isShowActualsHistory;
            set
            {
                if (value)
                {
                    if (MessageBoxService.ShowMessage("Please note that productivity isn't shown in 'Show Actuals History' mode, do you wish to continue?", "Warning", MessageButton.YesNo) == MessageResult.No)
                        return;
                }

                isShowActualsHistory = value;
                BluePrintsDataUtils.SaveUserPreference(DataUtils.GetNameOf(() => UserPreferences.Forecast_ShowActuals), value ? UserPreferences.PreferenceTrueValue : UserPreferences.PreferenceFalseValue);
                ForecastSummary.Reset();
                EntitiesUndoRedoManager.Clear();
                mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
            }
        }

        bool isAutoHideSummary;
        public bool IsAutoHideSummary
        {
            get => isAutoHideSummary;
            set
            {
                isAutoHideSummary = value;
                BluePrintsDataUtils.SaveUserPreference(DataUtils.GetNameOf(() => UserPreferences.Forecast_AutoHideSummary), value ? UserPreferences.PreferenceTrueValue : UserPreferences.PreferenceFalseValue);
                this.RaisePropertyChanged(x => x.SummaryVisibility);
            }
        }

        public Visibility SummaryVisibility => IsAutoHideSummary ? Visibility.Collapsed : Visibility.Visible;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            LoadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => setProject(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_HOUR_SNAPSHOTS, FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINE_DESCS, DISCIPLINE_DESCProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_EACS, FORECAST_EACProjectionFunc);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<Data.PHASE, Data.PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_SETTINGS, FORECAST_JOB_SETTINGProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTIONS, VARIATION_CONSTRUCTIONProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == LoadPROJECT.GUID);
        }
        protected virtual Func<IRepositoryQuery<FORECAST_EAC>, IQueryable<FORECAST_EAC>> FORECAST_EACProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.COST_TYPE == Common.CostType.Cost);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB_HOUR_SNAPSHOT>, IQueryable<FORECAST_JOB_HOUR_SNAPSHOT>> FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.DATA_DATE == FixedDataDate);
        }

        protected virtual Func<IRepositoryQuery<DISCIPLINE_DESC>, IQueryable<DISCIPLINE_DESC>> DISCIPLINE_DESCProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<FORECAST_JOB_SETTING>, IQueryable<FORECAST_JOB_SETTING>> FORECAST_JOB_SETTINGProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<VARIATION_CONSTRUCTION>, IQueryable<VARIATION_CONSTRUCTION>> VARIATION_CONSTRUCTIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(LoadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID == LoadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<Data.PROJECT> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);

            IsPasteCellLevel = true;
            this.RaisePropertyChanged(x => x.IsPasteCellLevel);
            this.RaisePropertyChanged(x => x.SelectMode);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Project Details
        public DateTime FixedDataDateMonthEnd => new DateTime(((DateTime)FixedDataDate).Year, ((DateTime)FixedDataDate).Month, 1).AddMonths(1).AddDays(-1);

        public DateTime PreviousDataDate
        {
            get
            {
                DateTime previousEACDataDate = new DateTime(FixedDataDateMonthEnd.Year, FixedDataDateMonthEnd.Month, 1);
                previousEACDataDate = previousEACDataDate.AddSeconds(-1);
                return previousEACDataDate;
            }
        }

        public DateTime LoadDataDate { get; set; }
        DateTime fixedDateTime;
        public DateTime FixedDataDate
        {
            get => fixedDateTime;
            set
            {
                //prevent tab switching from setting this to null because it's binded to view
                if (value != null && value.Year != new DateTime().Year)
                {
                    DateTime rawDataDate = value;
                    rawDataDate = new DateTime(value.Year, value.Month, 1);
                    rawDataDate = rawDataDate.AddMonths(1).AddSeconds(-1);
                    fixedDateTime = rawDataDate;
                }
            }
        }

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
            this.RaisePropertyChanged(x => x.FixedDataDate);

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

        public string P6ForecastProject
        {
            get
            {
                if (LoadPROJECT == null)
                    return string.Empty;

                return LoadPROJECT.P6FORECAST_NAME;
            }
            set
            {
                LoadPROJECT.P6FORECAST_NAME = value;
                savePROJECT();
            }
        }

        public DateTime? P6DataDate
        {
            get
            {
                if (LoadPROJECT == null)
                    return null;

                return LoadPROJECT.P6FORECAST_DATADATE;
            }
        }
        #endregion

        #region DataTable Population
        protected string columnEntity = "Entity";
        protected string columnCompare = "CompareEntities";
        DataTable dataPointsTable = null;
        ConcurrentBag<ForecastJobSnapshot> Jobs = null;
        protected List<DateTime> alignedDataDateCollection;
        public ForecastSummary ForecastSummary { get; set; }
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        List<X_PURCHORD_LINE_DETAIL> X_PURCHORD_LINE_DETAILS;
        List<ExoSubJobProjection> projectLines;
        public virtual DataTable DataPointsTable
        {
            get
            {
                return dataPointsTable;
            }

        }
        protected override bool loadDataPointsTable()
        {
            //Auto refresh forecast data on load
            if (FORECAST_JOB_HOUR_SNAPSHOTCollection.Count() == 0)
            {
                RefreshAllForecastData();
                return false;
            }

            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);

            initializeDataTable();
            dataModellingBackgroundWorker.RunWorkerAsync();

            //so filters will show transactions, as it is not shown during load, RaisePropertyChanged on ActualDetails will allow the grid to start showing data
            instantFeedbackActualDetailViewModel.OnParameterChange(LoadPROJECT);
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());
            return true;
        }

        private void initializeDataTable()
        {
            dataPointsTable = new DataTable();
            //construct data points table
            dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobSnapshot));
            dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));

            DateTime actualsEarliestDate = FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual && x.FORECAST_DATE != null).Count() == 0 ? DateTime.Now : FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual && x.FORECAST_DATE != null).Min(x => (DateTime)x.FORECAST_DATE);
            DateTime firstDataPointDate = isShowActualsHistory ? actualsEarliestDate : FixedDataDate;
            alignedDataDateCollection = generateDates(firstDataPointDate);
            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }
        }

        private void initializeGridColumns(ObservableCollection<ColumnDescriptor> parentViewColumns, ObservableCollection<SummaryDescriptor> parentSummaries, ObservableCollection<ColumnDescriptor> childViewColumns, ObservableCollection<SummaryDescriptor> childSummaries, IEnumerable<DateTime> alignedDataDates)
        {
            InitializeColumnSource(parentViewColumns, parentSummaries, alignedDataDates, false);
            InitializeColumnSource(childViewColumns, childSummaries, alignedDataDates, true);
        }

        private void updateForecastSummary(IEnumerable<ForecastJobSnapshot> jobs)
        {
            ForecastSummary.Reset();

            //calculate project summary, needs to be done after uncommitted is calculated
            ForecastSummary.Budget_Cost = jobs.Sum(x => x.Budget);
            ForecastSummary.Current_Cost = jobs.Sum(x => x.ActualCosts);
            ForecastSummary.Commitments = jobs.Sum(x => x.Outstanding);
            ForecastSummary.Uncommitted_Forecast = jobs.Sum(x => x.Uncommitted);
            ForecastSummary.OriginalEstimateAtCompletion = jobs.Sum(x => x.OriginalEstimateAtCompletion);
            ForecastSummary.EstimateAtCompletion = jobs.Sum(x => x.EstimateAtCompletion);
            ForecastSummary.CurrentEstimateAtCompletion = jobs.Sum(x => x.CurrentEstimateAtCompletion);
            ForecastSummary.Contingency = jobs.Where(x => x.IsContingency).Sum(x => x.EstimateAtCompletion);
            this.RaisePropertyChanged(x => x.ForecastSummary);
        }

        private void dataModellingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (dataModellingBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Loading EXO Data...");
            loadExoMethodsData();

            Jobs = new ConcurrentBag<ForecastJobSnapshot>();
            Common.LoadingScreenManager.SetMessage("Loading Tender Budget...");
            refreshTenderBudgetCollection();
            refreshJOBCOST_LINES_AUDIT();


            Common.LoadingScreenManager.SetMessage("Loading Unique Jobs...");
            //data relevant to job
            HashSet<string> uniqueWBSNames = new HashSet<string>();

            //Earned and P6 planned may generate outdated entries, omit it when generating unique entries
            foreach (string uniqueWBSName in FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.SNAPSHOT_TYPE != ForecastSnapshotValueType.P6Planned && x.SNAPSHOT_TYPE != ForecastSnapshotValueType.Earned).Select(x => x.ForecastViewCode).Distinct())
                uniqueWBSNames.Add(uniqueWBSName);

            foreach (string uniqueWBSName in projectLines.Select(x => x.ForecastViewCode).Distinct())
                uniqueWBSNames.Add(uniqueWBSName);

            Common.LoadingScreenManager.SetMaxProgress(uniqueWBSNames.Count() + 1);
            Common.LoadingScreenManager.ResetCurrentProgress();
            Common.LoadingScreenManager.SetMessage("Loading Data for Jobs...");
            ConcurrentBag<UniqueForecastJob> uniqueForecastJobs = new ConcurrentBag<UniqueForecastJob>();
            Parallel.ForEach(uniqueWBSNames,
            uniqueWBSName =>
            {
                List<string> delimited = uniqueWBSName.Split(';').ToList();
                string subJobCode = delimited[0];
                string disciplineCode = delimited[1];
                string commodityCode = delimited[2];
                string variationCode = delimited[3];
                UniqueForecastJob uniqueForecastJob = new UniqueForecastJob(projectLines, subJobCode, disciplineCode, commodityCode, variationCode, FixedDataDate, PreviousDataDate, FORECAST_JOB_HOUR_SNAPSHOTCollection);
                uniqueForecastJob.UpdateTenderBudget(TenderBudgetCollection.AsQueryable());
                uniqueForecastJob.UpdateErrorMessage(JOBCOST_LINES_AUDITCollection.AsQueryable());
                uniqueForecastJobs.Add(uniqueForecastJob);
                Common.LoadingScreenManager.Progress();

                //For Debugging
                //if (subJobCode == "20638-000-00-I1" && disciplineCode == "GP01" && commodityCode == "G64" && variationCode == "")
                //{

                //}
            });

            Common.LoadingScreenManager.SetMaxProgress(uniqueForecastJobs.Count() + 1);
            Common.LoadingScreenManager.ResetCurrentProgress();
            Common.LoadingScreenManager.SetMessage("Preparing View...");
            DateTime firstViewDate = alignedDataDateCollection.Count == 0 ? DateTime.Now : alignedDataDateCollection.First();
            bool isBudgetReadOnly = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_ChangeBudget)) == LoginCredentials.PermissionStatus.None;
            //child data table is used to record original value of actuals + committed + remaining values before it is overridden by forecasts
            Parallel.ForEach(uniqueForecastJobs,
                uniqueForecastJob =>
                {
                    ForecastJobSnapshot forecastJobSnapshot = new ForecastJobSnapshot(uniqueForecastJob, isBudgetReadOnly, FORECAST_EACCollection, FORECAST_EACPreviousCommitmentCollection, FORECAST_JOB_SETTINGCollection, COMMODITY_CODECollection, projectLines, PreviousDataDate);
                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        ForecastDateSnapshot forecastDateSnapshot = new ForecastDateSnapshot(uniqueForecastJob.AllCollection, firstViewDate, alignedDataDate.Date, FixedDataDate);
                        forecastJobSnapshot.DateCosts.Add(forecastDateSnapshot);
                    }

                    Jobs.Add(forecastJobSnapshot);
                    Common.LoadingScreenManager.Progress();
                });

            ////For Debugging
            //foreach(var uniqueForecastJob in uniqueForecastJobs)
            //{

            //}
        }

        private void dataModellingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Common.LoadingScreenManager.SetMessage("Caching User Forecast Override(s)...");
            List<FORECAST> cachedFORECASTCollection = QueryableFORECASTCollection.ToList();

            Common.LoadingScreenManager.SetMessage("Updating View...");
            Common.LoadingScreenManager.SetMaxProgress(Jobs.Count());
            Common.LoadingScreenManager.ResetCurrentProgress();

            foreach (ForecastJobSnapshot job in Jobs)
            {
                DataRow jobRow = updateDataTable(job, cachedFORECASTCollection);
                Common.LoadingScreenManager.Progress();
            }

            initializeGridColumns(ParentViewColumns, ParentSummaries, ChildViewColumns, ChildSummaries, alignedDataDateCollection);
            Common.LoadingScreenManager.SetMessage("Updating Summary...");
            updateForecastSummary(Jobs);
            loadSummaryStats();
            refreshP6DataDateError();
            Common.LoadingScreenManager.CloseLoadingScreen();

            IsLoading = false;
            this.RaisePropertiesChanged();
        }

        private DataRow updateDataTable(ForecastJobSnapshot job, List<FORECAST> CachedFORECASTCollection = null)
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

            compareP6UnitsRemainingRow[columnEntity] = new ForecastJobSnapshot() { DropDownPhase = "P6 Hours", CompareMask = "n2", ExoJob = job.ExoJob, DateCosts = job.DateCosts, IsP6HoursRow = true, P6RemainingUnits = job.P6RemainingUnits, P6RemainingCosts = job.P6RemainingCosts };
            compareP6CostsRemainingRow[columnEntity] = new ForecastJobSnapshot() { DropDownPhase = "P6 $", CompareMask = "c0" };

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
            compareUncommittedRow[columnEntity] = new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_UncommittedRowPhase + " $", CompareMask = "c0" };
            compareDataTable.Rows.Add(compareUncommittedRow);

            //create rows based on unique codes for each type
            Dictionary<string, DataRow> poForecastRows = new Dictionary<string, DataRow>();
            Dictionary<string, DataRow> indirectForecastRows = new Dictionary<string, DataRow>();

            //might want to use this in show actuals history
            //Dictionary<string, DataRow> materialForecastRows = new Dictionary<string, DataRow>();
            //Dictionary<string, DataRow> actualForecastRows = new Dictionary<string, DataRow>();

            //add PO forecast rows on demand
            foreach (KeyValuePair<string, decimal> uniquePOStockCodeAttrbutes in job.POStockCodeAttributes)
            {
                DataRow comparePOForecastRow = compareDataTable.NewRow();
                comparePOForecastRow[columnEntity] = new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_PORowPhase + " [" + uniquePOStockCodeAttrbutes.Key + "] $", CompareMask = "c0", DropDownIndirectBudget = uniquePOStockCodeAttrbutes.Value };
                poForecastRows.Add(uniquePOStockCodeAttrbutes.Key, comparePOForecastRow);
                compareDataTable.Rows.Add(comparePOForecastRow);
            }

            //add indirect rows on demand
            foreach (KeyValuePair<string, decimal> uniqueIndirectStockCode in job.IndirectStockCodeAttributes)
            {
                DataRow compareIndirectRemainingRow = compareDataTable.NewRow();
                compareIndirectRemainingRow[columnEntity] = new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_IndirectRowPhase + " [" + uniqueIndirectStockCode.Key + "] $", DropDownIndirectBudget = uniqueIndirectStockCode.Value, CompareMask = "c0" };
                indirectForecastRows.Add(uniqueIndirectStockCode.Key, compareIndirectRemainingRow);
                compareDataTable.Rows.Add(compareIndirectRemainingRow);
            }

            ////add actual rows on demand
            //foreach (KeyValuePair<string, decimal> uniqueActualStockCode in job.ActualStockCodeAttributes)
            //{
            //    DataRow compareActualRemainingRow = compareDataTable.NewRow();
            //    compareActualRemainingRow[columnEntity] = ViewModelSource.Create(() => new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_ActualRowPhase + " [" + uniqueActualStockCode.Key + "] $", DropDownIndirectBudget = uniqueActualStockCode.Value, CompareMask = "c0" });
            //    actualForecastRows.Add(uniqueActualStockCode.Key, compareActualRemainingRow);
            //    compareDataTable.Rows.Add(compareActualRemainingRow);
            //}

            //add the compare data table into a single column in parent row
            commodityRow[columnCompare] = compareDataTable;
            dataPointsTable.Rows.Add(commodityRow);
            decimal P6TotalCurrentRemainingUnits = 0;

            //use cached data if present so that it's threadsafe
            IQueryable<FORECAST> loadFORECASTCollection = CachedFORECASTCollection == null ? QueryableFORECASTCollection : CachedFORECASTCollection.AsQueryable();
            List<FORECAST> relevantFORECASTS = loadFORECASTCollection.Where(x => x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode).ToList();
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

                //might want to use this in show actuals history
                //foreach (FORECAST_JOB_HOUR_SNAPSHOT actualForecastSnapshot in dateCost.ActualForecastSnapshots)
                //{
                //    //finds the unique row based on stock code
                //    DataRow actualForecastRow = actualForecastRows.First(x => x.Key == actualForecastSnapshot.STOCK_CODE).Value;
                //    actualForecastRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = actualForecastSnapshot.FORECAST_COST;
                //}

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

                decimal viewCost = 0;
                if (forecastCostsOverrides.Count > 0 && dateCost != job.DateCosts.First())
                {
                    decimal overrideCosts = forecastCostsOverrides.Sum(x => (decimal)x.FORECAST_UNITS);
                    viewCost = overrideCosts;
                }
                else
                {
                    viewCost = dateCost.TotalCosts;
                }

                //only describe actuals when it's less than data date
                if (dateCost.QueryDate <= FixedDataDateMonthEnd)
                {
                    commodityRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = dateCost.ActualCosts;

                    //describe previously forecasted costs
                    compareUncommittedRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = forecastHistory.Sum(x => (decimal)x.FORECAST_UNITS);
                }
                else
                {
                    commodityRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = viewCost;

                    //when there aren't any P6 overrides then parent value will be purely uncommitted value, it's either this or P6 override which isn't categorised as uncommitted
                    if (forecastCostsOverrides.Count > 0 && forecastUnitsOverrides.Count == 0)
                        compareUncommittedRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = viewCost - dateCost.TotalCosts;
                    else
                        compareUncommittedRow[dateCost.QueryDate.ToString(BluePrintsResources.ColumnDateFormat)] = 0;
                }
            }

            job.P6RemainingUnitsOverride = P6TotalCurrentRemainingUnits;
            //updateViewForecastsOnDatesFromDb(commodityRow, false, relevantFORECASTS);
            updateTotalUncommittedOnJob(commodityRow);

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

            List<FORECAST> currentRowFORECASTS = relevantFORECASTS != null ? relevantFORECASTS : QueryableFORECASTCollection.Where(x => x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode).ToList();

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
                                uncommittedCostRow[alignedDateField] = overrideCostOnDataDate - dateCost.P6Costs - dateCost.ActualCosts - dateCost.IndirectForecastCosts - dateCost.POOutstandingCosts;
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

        private List<DateTime> generateDates(DateTime firstDataPointDate)
        {
            List<FORECAST_JOB_HOUR_SNAPSHOT> snapShots = FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.FORECAST_DATE != null).ToList();
            DateTime endDateToGenerate = snapShots.Count == 0 ? DateTime.Now.AddMonths(1) : snapShots.Max(x => (DateTime)x.FORECAST_DATE);
            DateTime firstDateToGenerateFrom = new DateTime();
            firstDateToGenerateFrom = firstDataPointDate;

            if (IsWeeks)
                return ChronologicalHelpers.GenerateEndDatesCollection(firstDateToGenerateFrom, endDateToGenerate, true);
            else
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

        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, IEnumerable<DateTime> alignedDates, bool isChild)
        {
            columns.Clear();
            summaries.Clear();

            if (!isChild)
            {
                bool isPreviousEACReadOnly = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_EditPreviousEAC)) == LoginCredentials.PermissionStatus.None;

                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PhaseCode", ReadOnly = true, Header = "Phase", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.SubJobCode", ReadOnly = true, Header = "Subjob", Fixed = FixedStyle.Left, Width = 110, Settings = SettingsType.JobError });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.AreaCode", ReadOnly = true, Visible = false, Header = "Area", Fixed = FixedStyle.Left, Width = 60, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DisciplineCode", ReadOnly = true, Header = "Discipline", Fixed = FixedStyle.Left, Width = 38, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DisciplineDesc", ReadOnly = true, Header = "Package", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.CommodityCode", ReadOnly = true, Header = "Commodity", Fixed = FixedStyle.Left, Width = 35, Settings = SettingsType.CommodityCode });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.CommodityName", ReadOnly = true, Header = "Commodity Name", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.VariationCode", ReadOnly = true, Header = "Variation", Fixed = FixedStyle.Left, Width = 60, Settings = SettingsType.Default });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.TenderBudget", ReadOnly = false, Header = "Tender Budget (H)", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Budget, Mask = "c0", HeaderToolTip = "Budget saved here during Roll Over" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.TenderBudget", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Budget", ReadOnly = false, Header = "Project Budget (A)", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Budget, HeaderToolTip = "EAC saved here during Roll Over" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Budget", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.BudgetVariance", ReadOnly = true, Header = "Rev 0 Variance (I) (H - A)", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Budget variance from first EAC" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.BudgetVariance", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.P6BudgetedUnits", ReadOnly = true, Visible = false, Header = "P6 Budget Hours", Mask = "###,##0h", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, HeaderToolTip = "Total hours from P6" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.P6BudgetedUnits", DisplayFormat = "###,##0h", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualUnits", ReadOnly = true, Header = "Actual Units", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n0", HeaderToolTip = "Actual units to date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualUnits", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualCosts", ReadOnly = true, Header = "Actual Costs (B)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Costs burned to Date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualCosts", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.P6RemainingUnits", ReadOnly = true, Header = "Remaining Hours", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n0", HeaderToolTip = "Remaining hours from refreshing P6" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.P6RemainingUnits", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ProgressETC", ReadOnly = true, Visible = false, Header = "User Remaining Hours", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "n0", HeaderToolTip = "Discretionary remaining hours keyed in by user, it'll only show value when there are remaining hours" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ProgressETC", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.P6RemainingUnitsOverride", ReadOnly = true, Header = "PF Hours", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n0", HeaderToolTip = "Remaining hours from refreshing P6" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.P6RemainingUnitsOverride", DisplayFormat = "n0", Type = SummaryItemType.Sum });
                if (!IsShowActualsHistory)
                {
                    columns.Add(new ColumnDescriptor() { FieldName = "Entity.Productivity", ReadOnly = false, Visible = false, Header = "PF", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n2", HeaderToolTip = "Productivity Factor, 0 means there aren't any units from P6" });
                    columns.Add(new ColumnDescriptor() { FieldName = "Entity.CurrentProductivity", ReadOnly = true, Visible = false, Header = "Current PF", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "n2", HeaderToolTip = "Current productivity factor, 0 means there aren't any earned or actuals units" });
                }

                columns.Add(new ColumnDescriptor() { FieldName = "Entity.IsProductivityFloating", Visible = false, ReadOnly = true, Header = "Floating PF", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default, HeaderToolTip = "Productivity on job with floating productivity can be updated to match current productivity" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualCostsPostDataDate", ReadOnly = true, Header = "Actual Costs Post DD", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Actual costs post data date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualCostsPostDataDate", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.ActualCostsPreviousDataDate", ReadOnly = true, Header = "Actual Costs Post DD", Visible = false, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Actual costs post data date" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.ActualCostsPreviousDataDate", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PctComplete", ReadOnly = true, Visible = false, Header = "% Complete", Fixed = FixedStyle.Left, Width = 40, Settings = SettingsType.Number, Mask = "p0", HeaderToolTip = "Procurement: Actuals / EAC, Others: (Budgeted Units - Remaining Units)/ Budgeted Units" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Outstanding", ReadOnly = true, Header = "Outstanding (C)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Open Commitment, amount left on purchase order (outstanding PO) or amount left on P6 forecasts" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Outstanding", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.TotalCommitmentPrevious", ReadOnly = true, Header = "Previous Total Commitment", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Previous total actual and PO outstanding up to previous calendar month end" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.TotalCommitmentPrevious", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.TotalCommitment", ReadOnly = true, Header = "Total Commitment (B + C)", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Total actuals and PO outstanding" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.TotalCommitment", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.TotalCommitmentDifference", ReadOnly = true, Header = "Total Commitment Diff", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Difference between current total commitment and previous total commitment" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.TotalCommitmentDifference", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.OriginalUncommitted", Visible = false, ReadOnly = true, Header = "Non-PF Uncommitted", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "(Sum of uncommitted costs - (costs from the forecasting months)) + P6 Costs" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.OriginalUncommitted", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Uncommitted", ReadOnly = true, Header = "Uncommitted (D)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "(Sum of uncommitted costs - (costs from the forecasting months)) + (P6 Costs with or without PF)" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Uncommitted", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.CurrentUncommitted", Visible = false, ReadOnly = true, Header = "PF Uncommitted", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "(Sum of uncommitted costs - (costs from the forecasting months)) + (P6 Costs with PF)" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.CurrentUncommitted", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.EstimateToComplete", ReadOnly = true, Visible = false, Header = "ETC", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Estimate to Complete (or costs to complete) - equal to forecasted costs, plus open commitments (outstanding purchase order)" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.EstimateToComplete", DisplayFormat = "c0", Type = SummaryItemType.Sum });

                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PreviousEAC", ReadOnly = isPreviousEACReadOnly, Header = "Prev. EAC (F)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Previous estimate at completion" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PreviousEAC", DisplayFormat = "c0", Type = SummaryItemType.Sum });

                columns.Add(new ColumnDescriptor() { FieldName = "Entity.EstimateAtCompletion", ReadOnly = true, Header = "EAC (E) (B + C + D)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Estimate at complete, forecasted costs + open commitments + accruals" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.EstimateAtCompletion", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.Variance", ReadOnly = true, Header = "Variance (A - E)", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Variance to budget" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Variance", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PeriodMovement", Header = "Period Move (G)", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0", HeaderToolTip = "Difference from previous EAC" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PeriodMovement", DisplayFormat = "c0", Type = SummaryItemType.Sum });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.PercentagePeriodMovement", Header = "Percentage Period Move (G / F)", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "p0", HeaderToolTip = "Period Movement / Previous EAC" });
                summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PercentagePeriodMovement", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            }
            else
            {
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DropDownPhase", Header = "", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default, HeaderToolTip = "Source of forecasted costs/hours type" });
                columns.Add(new ColumnDescriptor() { FieldName = "Entity.DropDownIndirectBudget", ReadOnly = true, Header = "Project Budget (A)", Increment = 1, Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Budget, HeaderToolTip = "Indirect budget from Exo" });
            }

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x.Date))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                if (alignedDate <= FixedDataDateMonthEnd)
                {
                    //do not show actuals
                    if (isShowActualsHistory)
                        columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = true, ColumnDate = alignedDate, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastPast });
                }
                if (alignedDate > FixedDataDateMonthEnd)
                {
                    if (isChild)
                        columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, ColumnDate = alignedDate, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastChild });
                    else
                        columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, ColumnDate = alignedDate, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                }

                if (!isChild)
                    summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }
        #endregion

        #region View Events
        public bool CanReloadP6Forecast()
        {
            return !IsLoading;
        }

        public async void ReloadP6Forecast()
        {
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);
            await BluePrintsContextHelper.RefreshDeliverablesRemainingDataPointsByProject(LoadPROJECT.NUMBER, true);
            await BluePrintsContextHelper.RefreshDeliverablesPlannedDataPointsByProject(LoadPROJECT.NUMBER, true);
            BluePrintsContextHelper.RefreshForecastP6ByProject(LoadPROJECT.NUMBER, FixedDataDate, true);
            BluePrintsContextHelper.RefreshForecastP6ByProject(LoadPROJECT.NUMBER, FixedDataDate, false);
            FullRefresh();
        }

        public void ResetCellContent()
        {
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = getSelectedCells();

            foreach (var selected_cell in selected_cells)
            {
                int row_handle = selected_cell.RowHandle;
                DataRowView editing_row_view = (DataRowView)GridControlService.GridControl.GetRow(row_handle);
                if (editing_row_view == null)
                    continue;

                DataRow editing_row = editing_row_view.Row;
                DataColumn editing_column = editing_row.Table.Columns[selected_cell.Column.FieldName];

                string columnFieldName = selected_cell.Column.FieldName;
                DateTime deleteCellDate;
                if (DateTime.TryParse(columnFieldName, out deleteCellDate))
                {
                    if (deleteCellDate <= FixedDataDate)
                        continue;

                    resetViewRemainingOnJob(editing_row, columnFieldName, true);
                    findExistingOrAddNewForecast(editing_row, deleteCellDate, null);
                    //editing_row[columnFieldName] = 0.00m;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            refreshGridData();
        }

        public bool CanApplyCurrentPF()
        {
            return !IsLoading;
        }

        private void refreshP6DataDateError()
        {
            IsP6DataDateError = P6DataDate < FixedDataDate;
            Animate = true;
            this.RaisePropertyChanged(x => x.IsP6DataDateError);
            this.RaisePropertyChanged(x => x.P6ErrorMessage);
            this.RaisePropertyChanged(x => x.P6ErrorIconName);
            this.RaisePropertyChanged(x => x.Animate);
        }

        public void ShowP6ErrorMessage()
        {
            MessageBoxService.ShowMessage(P6ErrorMessage, "PF Error", MessageButton.OK, MessageIcon.Error);
        }

        public string P6ErrorMessage { get; set; }
        public string P6ErrorIconName { get; set; }
        public bool IsP6DataDateError { get; set; }
        public bool Animate { get; set; }
        public void ApplyCurrentPF()
        {
            if (P6DataDate < FixedDataDate)
            {
                ShowP6ErrorMessage();
                return;
            }

            EntitiesUndoRedoManager.PauseActionId();
            GridControl gridControl = GridControlService.GridControl;
            TableView tableView = gridControl.View as TableView;
            var selectedRows = tableView.GetSelectedRows();

            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            foreach (var selectedRow in selectedRows)
            {
                int row_handle = selectedRow.RowHandle;
                DataRowView editing_row_view = (DataRowView)GridControlService.GridControl.GetRow(row_handle);
                DataRow editing_row = editing_row_view.Row;
                ForecastJobSnapshot job = (ForecastJobSnapshot)editing_row[columnEntity];

                if (job.CurrentProductivity > 0)
                {
                    List<ErrorMessage> currentJobErrorMessage;
                    commitCellValue(BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Productivity), editing_row, job.Productivity, job.CurrentProductivity, out currentJobErrorMessage);
                    findExistingOrAddNewForecastJobSetting(editing_row, true);
                    errorMessages.AddRange(currentJobErrorMessage);
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            refreshGridData();

            ShowErrorMessage("Errors", errorMessages);
        }

        public bool CanUpdateCurrentPF()
        {
            return !IsLoading;
        }

        public void UpdateCurrentPF()
        {
            if (MessageBoxService.ShowMessage("Are you use you want to apply current PF to all jobs that had current PF applied?", "Confirmation", MessageButton.OKCancel, MessageIcon.Question) == MessageResult.Cancel)
                return;

            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            IEnumerable<DataRow> enumerableRows = from DataRow dr in dataPointsTable.Rows select dr;
            foreach (var row in enumerableRows)
            {
                ForecastJobSnapshot job = (ForecastJobSnapshot)row[columnEntity];
                List<ErrorMessage> currentJobErrorMessage;
                if (job.IsProductivityFloating && job.CurrentProductivity > 0)
                {
                    commitCellValue(BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Productivity), row, job.Productivity, job.CurrentProductivity, out currentJobErrorMessage);
                    errorMessages.AddRange(currentJobErrorMessage);
                }
            }

            refreshGridData();
            ShowErrorMessage("Errors", errorMessages);
        }


        string weekViewPreventEditingMessage = "Sorry, cells in weeks view isn't editable, please switch to month view to edit cell";
        bool canEditConstructionUncommitted = false;
        public override void ValidateCell(GridCellValidationEventArgs e)
        {
            if (e.Value != null)
            {
                DateTime dateTime;
                if (IsWeeks)
                {
                    e.ErrorContent = weekViewPreventEditingMessage;
                    e.IsValid = false;
                }
                else if (DateTime.TryParse(e.Column.FieldName, out dateTime))
                {
                    ForecastJobSnapshot job = (ForecastJobSnapshot)((DataRowView)e.Row)[columnEntity];
                    if (job.IsConstruction)
                    {
                        if (!canEditConstructionUncommitted && P6ForecastProject != null)
                        {
                            e.ErrorContent = "Cannot set uncommitted cost for non procurement/construction job";
                            e.IsValid = false;
                        }
                    }
                    else if (job.IsDesign || job.IsIndirect)
                    {
                        e.ErrorContent = "Cannot set uncommitted cost for non procurement/construction job";
                        e.IsValid = false;
                    }
                    else
                    {
                        decimal defaultCosts = getMasterRowResetValue((DataTable)(((DataRowView)e.Row)[columnCompare]), e.Column.FieldName);
                        if ((decimal)e.Value < defaultCosts)
                        {
                            e.ErrorContent = "Cannot set costs below forecasted costs";
                            e.IsValid = false;
                        }
                    }
                }
                else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Budget)))
                {
                    if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_ChangeBudget)) == LoginCredentials.PermissionStatus.None)
                    {
                        e.ErrorContent = "You do not have permission to change the budget";
                        e.IsValid = false;
                    }
                }
                else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobSnapshot().TenderBudget)))
                {
                    if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_ChangeBudget)) == LoginCredentials.PermissionStatus.None)
                    {
                        e.ErrorContent = "You do not have permission to change the tender budget";
                        e.IsValid = false;
                    }
                }
                else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Productivity)))
                {
                    DataRowView row = (DataRowView)e.Row;

                    if (sumP6Units(row) == 0)
                    {
                        e.ErrorContent = "There are no units from P6 to override productivity, please edit P6 hours manually";
                        e.IsValid = false;
                    }
                    else
                    {
                        ForecastJobSnapshot job = (ForecastJobSnapshot)row[columnEntity];
                        if (QueryableFORECASTCollection.Any(x => x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode))
                        {
                            if (MessageBoxService.ShowMessage("Any forecast done on this job will be removed and automatically generated based on PF = " + e.Value.ToString() + ", do you wish to continue?", "Warning", MessageButton.OKCancel) == MessageResult.Cancel)
                            {
                                e.ErrorContent = "Action cancelled";
                                e.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        public void ChildGridValidateCell(GridCellValidationEventArgs e)
        {
            if (e.Value != null)
            {
                if (IsWeeks)
                {
                    e.ErrorContent = weekViewPreventEditingMessage;
                    e.IsValid = false;
                }
            }
        }

        private decimal sumP6Units(DataRowView parentRow)
        {
            decimal p6Units = 0;
            DataTable compareDataTable = (DataTable)parentRow[columnCompare];
            if (compareDataTable != null && compareDataTable.Rows.Count > 0)
            {
                if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                {
                    DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRowIndex)];
                    DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                    DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRowIndex)];

                    foreach (DataColumn column in compareDataTable.Columns)
                    {
                        DateTime parseDateTime;
                        if (DateTime.TryParse(column.ColumnName, out parseDateTime))
                        {
                            decimal childP6UnitsValue = compareChildP6UnitsRemainingRow[column.ColumnName] == DBNull.Value ? 0 : (decimal)compareChildP6UnitsRemainingRow[column.ColumnName];

                            p6Units += childP6UnitsValue;
                        }
                    }
                }
            }

            return p6Units;
        }

        public void EditValueValidate(DevExpress.Xpf.Editors.ValidationEventArgs e)
        {
            BaseEdit baseEdit = e.Source as BaseEdit;
            if (baseEdit == null)
                return;

            string fieldName = baseEdit.Tag.ToString();
        }

        public void GotFocus(RoutedEventArgs e)
        {
            //only allow editing when user focused on control instead of being changed from EditValueChanged
            allowValueEditing = true;
        }

        //prevent value from being saved if layout is loading
        bool allowValueEditing = false;
        public void EditValueChanged(EditValueChangedEventArgs e)
        {
            if (IsLoading)
                return;

            if (!allowValueEditing)
                return;

            if (MainViewModel == null || LoadPROJECT == null || ForecastSummary == null)
                return;

            decimal? newValueDecimal = null;
            decimal notNullDecimalValue = 0;
            if (e.NewValue != null)
            {
                decimal.TryParse(e.NewValue.ToString(), out notNullDecimalValue);
                newValueDecimal = notNullDecimalValue;
            }

            string fieldName = ((BaseEdit)e.OriginalSource).Tag.ToString();
            DataUtils.TrySetNestedValue(fieldName, LoadPROJECT, newValueDecimal);
            savePROJECT();

            if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().ORI_REVENUE))
                ForecastSummary.Original_Revenue = notNullDecimalValue;
            else if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().VAR_REVENUE))
            {
                if (newValueDecimal == null)
                    ForecastSummary.Approved_Var_Revenue = VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Approved).Sum(x => x.ManualApprovedEstimatedValue);
                else
                    ForecastSummary.Approved_Var_Revenue = notNullDecimalValue;

                this.RaisePropertyChanged(x => x.IsManualApprovedVariationRevenue);
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().UNAPPROVED_VAR_REVENUE))
                ForecastSummary.Unapproved_Var_Revenue = notNullDecimalValue;
            else if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().TOTAL_UNAPPROVED_VAR_REVENUE))
            {
                if (newValueDecimal == null)
                    ForecastSummary.Total_Unapproved_Var_Revenue = VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Submitted).Sum(x => x.ManualApprovedEstimatedValue);
                else
                    ForecastSummary.Total_Unapproved_Var_Revenue = notNullDecimalValue;

                this.RaisePropertyChanged(x => x.IsManualRevisedVariationRevenue);
            }
            //else if (fieldName == BindableBase.GetPropertyName(() => new Data.PROJECT().EAC_REVENUE))
            //    ForecastSummary.EAC_Revenue = newValueDecimal;

            this.RaisePropertyChanged(x => x.ForecastSummary);
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

            bool removeFloatingProductivity = false;
            ForecastJobSnapshot job = (ForecastJobSnapshot)dataRowView.Row[columnEntity];
            if (job.IsProductivityFloating)
            {
                DateTime dateTime;
                if (DateTime.TryParse(e.Column.FieldName, out dateTime))
                    removeFloatingProductivity = true;
                else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Productivity)))
                    removeFloatingProductivity = true;

                if (removeFloatingProductivity)
                {
                    findExistingOrAddNewForecastJobSetting(dataRowView.Row, false);
                }
            }

            List<ErrorMessage> errorMessages;
            commitCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value, out errorMessages);
            EntitiesUndoRedoManager.UnpauseActionId();

            if (e.Column.FieldName == columnEntity + "." + BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Productivity))
            {
                updateTotalUncommittedOnJob(dataRowView.Row, true);
                updateFloatingSummaryMembers();
            }

            this.RaisePropertyChanged(x => x.ForecastSummary);
            refreshGridData();
            e.Handled = true;
            ShowErrorMessage("Errors", errorMessages);
        }


        public bool CanSaveProjectBudget()
        {
            return !IsLoading;
        }

        public void SaveProjectBudget()
        {
            if (!checkSaveEACPermission(true))
                return;

            saveProjectBudgetToTenderBudget();
            saveEACToProjectBudget();
            MessageBoxService.ShowMessage("Project budget is saved to tender budget and EAC is saved as project budget", "Roll Over Data Saved", MessageButton.OK, MessageIcon.Information);
        }

        public bool CanSaveCurrentMonthEAC()
        {
            return !IsLoading;
        }

        public void SaveCurrentMonthEAC()
        {
            if (!checkSaveEACPermission(false))
                return;

            Common.LoadingScreenManager.ShowLoadingScreen(DataPointsTable.Rows.Count);

            DateTime firstForecastDate = new DateTime(FixedDataDateMonthEnd.Year, FixedDataDateMonthEnd.Month, 1).AddMonths(2).AddDays(-1);
            foreach (DataRow masterRow in DataPointsTable.Rows)
            {
                ForecastJobSnapshot entity = (ForecastJobSnapshot)masterRow[columnEntity];
                findExistingOrAddNewEAC(FixedDataDateMonthEnd, entity, bluePrintsUnitOfWork, entity.EstimateAtCompletion, false, ForecastEACType.EAC);
                findExistingOrAddNewEAC(FixedDataDateMonthEnd, entity, bluePrintsUnitOfWork, entity.TotalCommitment, false, ForecastEACType.PreviousCommitment);
                decimal firstForecastDateValue = (decimal)masterRow[firstForecastDate.ToString(BluePrintsResources.ColumnDateFormat)];

                FORECAST findFORECASTS = bluePrintsUnitOfWork.FORECASTS.FirstOrDefault(x => x.FORECAST_TYPE == ForecastDataType.DataDateForecast && x.FORECAST_DATE == firstForecastDate && x.SUBJOB_CODE == entity.SubJobCode && x.DISCIPLINE_CODE == entity.DisciplineCode && x.COMMODITY_CODE == entity.CommodityCode && x.VARIATION_CODE == entity.VariationCode);
                if (findFORECASTS != null)
                    findFORECASTS.FORECAST_UNITS = firstForecastDateValue;
                else
                {
                    findFORECASTS = new FORECAST();
                    findFORECASTS.GUID = Guid.Empty;
                    findFORECASTS.GUID_PROJECT = LoadPROJECT.GUID;
                    findFORECASTS.SUBJOB_CODE = entity.SubJobCode;
                    findFORECASTS.DISCIPLINE_CODE = DataUtils.NormalizeString(entity.DisciplineCode);
                    findFORECASTS.COMMODITY_CODE = DataUtils.NormalizeString(entity.CommodityCode);
                    findFORECASTS.VARIATION_CODE = DataUtils.NormalizeString(entity.VariationCode);
                    findFORECASTS.FORECAST_DATE = firstForecastDate;
                    findFORECASTS.FORECAST_UNITS = firstForecastDateValue;
                    findFORECASTS.FORECAST_TYPE = ForecastDataType.DataDateForecast;
                    bluePrintsUnitOfWork.FORECASTS.Add(findFORECASTS);
                }

                Common.LoadingScreenManager.Progress();
            }

            findExistingOrAddNewEACHistory(FixedDataDateMonthEnd, ForecastSummary, bluePrintsUnitOfWork);

            Common.LoadingScreenManager.CloseLoadingScreen();
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Saving changes...");
            bluePrintsUnitOfWork.SaveChanges();
            Common.LoadingScreenManager.CloseLoadingScreen();
            FixedDataDate = FixedDataDateMonthEnd.AddMonths(1);
            LoadDataDate = FixedDataDate;
            SaveDateAndRefresh();
        }
        #endregion

        #region Helpers
        private bool checkSaveEACPermission(bool isCheckProjectBudget)
        {
            if (isCheckProjectBudget)
            {
                if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_SaveProjectBudget)) == LoginCredentials.PermissionStatus.None)
                {
                    MessageBoxService.ShowMessage("You are not authorised to use this function", "Not Authorised", MessageButton.OK, MessageIcon.Exclamation);
                    return false;
                }
            }
            else
            {
                if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_SaveEAC)) == LoginCredentials.PermissionStatus.None)
                {
                    MessageBoxService.ShowMessage("You are not authorised to use this function", "Not Authorised", MessageButton.OK, MessageIcon.Exclamation);
                    return false;
                }
            }

            List<ForecastJobSnapshot> jobs = getJobDataFromDatatable();
            if (jobs.Any(x => x.IsPOError))
            {
                MessageBoxService.ShowMessage("Some PO forecast aren't completed yet or misaligned\nPlease go to PO forecast and click Align Actuals\nThen refresh this screen", "Error", MessageButton.OK, MessageIcon.Exclamation);
                return false;
            }

            return true;
        }

        private IEnumerable<GridCell> getSelectedCells()
        {
            GridControl gridControl = GridControlService.GridControl;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return selected_cells;
                else
                {
                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            return selected_cells;
        }


        private void refreshGridData()
        {
            refreshGridDataDelayed();
        }

        private void refreshGridDataDelayed()
        {
            delayedGridUpdateTimer.Tick -= DelayedGridUpdateTimer_Tick;
            delayedGridUpdateTimer.Tick += DelayedGridUpdateTimer_Tick;
            delayedGridUpdateTimer.Start();
        }

        private void DelayedGridUpdateTimer_Tick(object sender, EventArgs e)
        {
            delayedGridUpdateTimer.Stop();
            GridControlEx gridControlEx = (GridControlEx)GridControlService.GridControl;
            ObservableCollection<BaseModel.Misc.GroupInfo> saveStates = gridControlEx.States;
            GridControlService.GridControl.RefreshData();
            DataControlDetailDescriptor gridDetail = (DataControlDetailDescriptor)GridControlService.GridControl.DetailDescriptor;
            GridControl childGrid = (GridControl)gridDetail.DataControl;

            childGrid.RefreshRow(0);
            childGrid.RefreshRow(1);
            childGrid.RefreshRow(2);
            childGrid.RefreshRow(3);
            childGrid.RefreshRow(4);

            gridControlEx.States = saveStates;
        }

        protected virtual void commitCellValue(string fieldName, DataRow row, object oldValue, object newValue, out List<ErrorMessage> errorMessages, bool skipSaveChangesAndRowUpdate = false)
        {
            ForecastJobSnapshot ForecastJobSnapshot = (ForecastJobSnapshot)row[columnEntity];
            errorMessages = new List<ErrorMessage>();
            fieldName = fieldName.Replace("Entity.", "");
            if (fieldName == BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Budget))
            {
                commitBudget(primeroUnitOfWork, bluePrintsUnitOfWork, row, newValue, out errorMessages);
                ForecastJobSnapshot.RefreshErrorMessage(ExoQueries.GetExoSubJobProjection(primeroUnitOfWork, LoadPROJECT.NUMBER, ForecastJobSnapshot.SubJobCode, ForecastJobSnapshot.DisciplineCode, ForecastJobSnapshot.CommodityCode, ForecastJobSnapshot.VariationCode), queryableJOBCOST_LINES_AUDITCollection);
                ForecastJobSnapshot.Update();
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new ForecastJobSnapshot().TenderBudget))
            {
                decimal newTenderBudget;
                if (decimal.TryParse(newValue.ToString(), out newTenderBudget))
                {
                    findExistingOrAddNewEAC(FixedDataDateMonthEnd, ForecastJobSnapshot, bluePrintsUnitOfWork, newTenderBudget, true, ForecastEACType.TenderBudget);
                    ForecastJobSnapshot.TenderBudget = newTenderBudget;
                    ForecastJobSnapshot.Update();
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new ForecastJobSnapshot().DisciplineDesc))
            {
                ForecastJobSnapshot.FindExistingOrAddDisciplineDesc(DISCIPLINE_DESCCollectionViewModel, LoadPROJECT.GUID);
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new ForecastJobSnapshot().PreviousEAC))
            {
                decimal newPreviousEAC;
                if (decimal.TryParse(newValue.ToString(), out newPreviousEAC))
                {
                    DateTime previousEACDataDate = new DateTime(FixedDataDateMonthEnd.Year, FixedDataDateMonthEnd.Month, 1);
                    previousEACDataDate = previousEACDataDate.AddDays(-1);

                    findExistingOrAddNewEAC(previousEACDataDate, ForecastJobSnapshot, bluePrintsUnitOfWork, newPreviousEAC, true, ForecastEACType.EAC);
                    ForecastJobSnapshot.PreviousEAC = newPreviousEAC;
                    ForecastJobSnapshot.Update();
                }
            }
            else if (fieldName.Contains(BindableBase.GetPropertyName(() => new ForecastJobSnapshot().Productivity)))
            {
                if (newValue != null)
                {
                    decimal newProductivity = (decimal)newValue;
                    ForecastJobSnapshot job = ((ForecastJobSnapshot)row[columnEntity]);

                    DataTable compareDataTable = (DataTable)row[columnCompare];
                    DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRowIndex)];

                    DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                    DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRowIndex)];

                    List<FORECAST> resetFORECASTS = QueryableFORECASTCollection.Where(x => x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode).ToList();
                    foreach (FORECAST resetFORECAST in resetFORECASTS)
                    {
                        resetFORECAST.FORECAST_UNITS = null;
                    }

                    //FORECASTCollectionViewModel.BaseBulkSave(resetFORECASTS);
                    foreach (ForecastDateSnapshot dateCost in job.DateCosts)
                    {
                        string alignedDateField = (dateCost.QueryDate).ToString(BluePrintsResources.ColumnDateFormat);
                        decimal originalP6Units = (decimal)compareChildP6UnitsRemainingRow[alignedDateField];
                        decimal oldP6Units = (decimal)compareP6UnitsRemainingRow[alignedDateField];
                        if (originalP6Units > 0)
                        {
                            decimal newP6Units = 0;
                            if (newProductivity > 0)
                                newP6Units = originalP6Units / newProductivity;
                            findExistingOrAddNewForecast(compareP6UnitsRemainingRow, dateCost.QueryDate, newP6Units, oldP6Units, true);
                        }
                        else
                        {
                            resetChildRow(compareDataTable, alignedDateField, false);
                        }
                    }

                    if (!skipSaveChangesAndRowUpdate)
                    {
                        bluePrintsUnitOfWork.SaveChanges();
                        updateViewForecastsOnDatesFromDb(compareP6UnitsRemainingRow, true);
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

                    findExistingOrAddNewForecast(row, dateTime, forecastUnits, oldValue, skipSaveChangesAndRowUpdate);
                }
            }
        }

        private FORECAST_EAC createNewEAC(DateTime forecastDate, ForecastJobSnapshot projection, decimal newPreviousEAC, ForecastEACType forecastEACType)
        {
            FORECAST_EAC newFORECAST_EAC = new FORECAST_EAC();
            newFORECAST_EAC.GUID = Guid.Empty;
            newFORECAST_EAC.GUID_PROJECT = LoadPROJECT.GUID;
            newFORECAST_EAC.SUBJOB_CODE = projection.SubJobCode;
            newFORECAST_EAC.DISCIPLINE_CODE = DataUtils.NormalizeString(projection.DisciplineCode);
            newFORECAST_EAC.COMMODITY_CODE = DataUtils.NormalizeString(projection.CommodityCode);
            newFORECAST_EAC.VARIATION_CODE = DataUtils.NormalizeString(projection.VariationCode);
            newFORECAST_EAC.FORECAST_DATE = forecastDate.Date;
            newFORECAST_EAC.FORECAST_COSTS = newPreviousEAC;
            newFORECAST_EAC.TYPE = forecastEACType;
            newFORECAST_EAC.CREATED = DateTime.Now;
            newFORECAST_EAC.CREATEDBY = LoginCredentials.CurrentUserGuid;

            return newFORECAST_EAC;
        }
        #endregion

        #region Database Helpers
        public bool CanSaveDateAndRefresh()
        {
            return !IsLoading;
        }

        public void SaveDateAndRefresh()
        {
            if (FixedDataDate != null)
            {
                ForecastSummary.Reset();
                if (FixedDataDate != LoadDataDate)
                {
                    if (FixedDataDate < LoadDataDate)
                    {
                        if (FORECAST_EACCollection.Count() > 0)
                        {
                            DateTime lastEACDataDate = FORECAST_EACCollection.Max(x => x.FORECAST_DATE);
                            if (FixedDataDate < lastEACDataDate)
                            {
                                if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_MoveDataDate)) == LoginCredentials.PermissionStatus.None)
                                {
                                    MessageBoxService.ShowMessage("Cannot move data date backwards because EAC is finalised for " + ((DateTime)lastEACDataDate).ToShortDateString(), "Error", MessageButton.OK, MessageIcon.Exclamation);
                                    FixedDataDate = LoadDataDate;
                                    this.RaisePropertyChanged(x => x.FixedDataDate);
                                    return;
                                }
                            }

                        }
                    }
                    //restrict user from moving data date forward if there are forecast but EAC isn't saved
                    else if (FixedDataDate > LoadDataDate)
                    {
                        bool hasEACOnCurrentDataDate = FORECAST_EACCollection.Where(x => x.FORECAST_DATE == LoadDataDate).Count() > 0;
                        if (LoadDataDate != null && !hasEACOnCurrentDataDate)
                        {
                            if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_MoveDataDate)) == LoginCredentials.PermissionStatus.None)
                            {
                                MessageBoxService.ShowMessage("Cannot move data date forward because EAC isn't saved for " + ((DateTime)LoadDataDate).ToShortDateString(), "Error", MessageButton.OK, MessageIcon.Exclamation);
                                FixedDataDate = LoadDataDate;
                                this.RaisePropertyChanged(x => x.FixedDataDate);
                                return;
                            }
                        }
                    }
                }

                LoadPROJECT.FORECAST_DATA_DATE = FixedDataDate;
                LoadPROJECT.FORECAST_END_DATE = FixedEndDate;
                PROJECTCollectionViewModel.Save(LoadPROJECT);
                LoadDataDate = FixedDataDate;
                FullRefresh();
            }
        }

        /// <summary>
        /// Saves project budget to tender budget
        /// </summary>
        private void saveProjectBudgetToTenderBudget()
        {
            Common.LoadingScreenManager.ShowLoadingScreen(DataPointsTable.Rows.Count);
            Common.LoadingScreenManager.SetMessage("Copying project budget to tender budget...");
            foreach (DataRow masterRow in DataPointsTable.Rows)
            {
                ForecastJobSnapshot entity = (ForecastJobSnapshot)masterRow[columnEntity];
                findExistingOrAddNewEAC(FixedDataDateMonthEnd, entity, bluePrintsUnitOfWork, entity.Budget, false, ForecastEACType.TenderBudget);
                entity.TenderBudget = entity.Budget;
                Common.LoadingScreenManager.Progress();
            }

            Common.LoadingScreenManager.CloseLoadingScreen();
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Saving changes...");
            bluePrintsUnitOfWork.SaveChanges();
            Common.LoadingScreenManager.CloseLoadingScreen();
        }

        /// <summary>
        /// Saves project budget to tender budget
        /// </summary>
        private void saveEACToProjectBudget()
        {
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            Common.LoadingScreenManager.ShowLoadingScreen(DataPointsTable.Rows.Count);
            Common.LoadingScreenManager.SetMessage("Copying EAC to project budget...");
            foreach (DataRow forecastRow in DataPointsTable.Rows)
            {
                List<ErrorMessage> currentErrorMessages = new List<ErrorMessage>();
                ForecastJobSnapshot entity = (ForecastJobSnapshot)forecastRow[columnEntity];
                if (commitBudget(primeroUnitOfWork, bluePrintsUnitOfWork, forecastRow, entity.EstimateAtCompletion, out currentErrorMessages, false))
                    entity.Budget = entity.EstimateAtCompletion;

                errorMessages.AddRange(currentErrorMessages);
                Common.LoadingScreenManager.Progress();
            }

            Common.LoadingScreenManager.CloseLoadingScreen();
            ShowErrorMessage("Save EAC to Project Budget Issues", errorMessages);
            refreshGridData();
            updateFloatingSummaryMembers();
        }

        private void findExistingOrAddNewEAC(DateTime forecastDate, ForecastJobSnapshot entity, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork, decimal newPreviousEAC, bool save, ForecastEACType forecastEACType)
        {
            if (entity.SubJobCode == null)
                return;

            string normalizedVariationCode = DataUtils.NormalizeString(entity.VariationCode);
            IQueryable<FORECAST_EAC> jobFORECAST_EACs = bluePrintsEntitiesUnitOfWork.FORECAST_EACS.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.SUBJOB_CODE == entity.SubJobCode && x.DISCIPLINE_CODE == entity.DisciplineCode && x.COMMODITY_CODE == entity.CommodityCode && x.VARIATION_CODE == normalizedVariationCode && x.TYPE == forecastEACType);
            FORECAST_EAC forecast_EAC;
            if (forecastEACType == ForecastEACType.TenderBudget)
                forecast_EAC = jobFORECAST_EACs.FirstOrDefault(x => x.TYPE == forecastEACType);
            else
                forecast_EAC = jobFORECAST_EACs.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.TYPE == forecastEACType);

            if (forecast_EAC != null)
            {
                forecast_EAC.FORECAST_COSTS = newPreviousEAC;
                forecast_EAC.TYPE = forecastEACType;
                if (save)
                    bluePrintsEntitiesUnitOfWork.SaveChanges();
            }
            else
            {
                FORECAST_EAC newForecast_EAC = createNewEAC(forecastDate, entity, newPreviousEAC, forecastEACType);
                bluePrintsEntitiesUnitOfWork.FORECAST_EACS.Add(newForecast_EAC);
                if (save)
                    bluePrintsEntitiesUnitOfWork.SaveChanges();
            }
        }

        private void findExistingOrAddNewEACHistory(DateTime forecastDate, ForecastSummary entity, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork)
        {
            FORECAST_HISTORY forecast_history = bluePrintsEntitiesUnitOfWork.FORECAST_HISTORIES.FirstOrDefault(x => x.EAC_DATE == forecastDate && x.GUID_PROJECT == LoadPROJECT.GUID);

            if (forecast_history == null)
            {
                forecast_history = new FORECAST_HISTORY();
                bluePrintsEntitiesUnitOfWork.FORECAST_HISTORIES.Add(forecast_history);
            }

            forecast_history.ORIGINAL_REVENUE = entity.Original_Revenue;
            forecast_history.APPROVED_VARIATION = entity.Approved_Var_Revenue;
            forecast_history.ORIGINAL_COSTS = entity.Budget_Cost;
            forecast_history.UNAPPROVED_VARIATION = entity.Unapproved_Var_Revenue;
            forecast_history.TOTAL_UNAPPROVED_VARIATION = entity.Total_Unapproved_Var_Revenue;
            forecast_history.TOTAL_EAC = entity.EstimateAtCompletion;
            forecast_history.EAC_DATE = forecastDate;
            forecast_history.GUID_PROJECT = LoadPROJECT.GUID;
            forecast_history.CONTINGENCY = entity.Contingency;
            forecast_history.CASHFLOW = entity.UnderOverClaim;
            forecast_history.CREATED = DateTime.Now;
            forecast_history.CREATEDBY = LoginCredentials.CurrentUserGuid;

            bluePrintsEntitiesUnitOfWork.SaveChanges();
        }

        private void findExistingOrAddNewForecastJobSetting(DataRow updateRow, bool isFloatingProductivity)
        {
            ForecastJobSnapshot job = ((ForecastJobSnapshot)updateRow[columnEntity]);
            FORECAST_JOB_SETTING relevantFORECAST_JOB_SETTING = FORECAST_JOB_SETTINGCollection.FirstOrDefault(x => x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode);
            if (relevantFORECAST_JOB_SETTING == null)
            {
                FORECAST_JOB_SETTING newFORECAST_JOB_SETTING = new FORECAST_JOB_SETTING();
                newFORECAST_JOB_SETTING.GUID_PROJECT = LoadPROJECT.GUID;
                newFORECAST_JOB_SETTING.SUBJOB_CODE = job.SubJobCode;
                newFORECAST_JOB_SETTING.DISCIPLINE_CODE = DataUtils.NormalizeString(job.DisciplineCode);
                newFORECAST_JOB_SETTING.COMMODITY_CODE = DataUtils.NormalizeString(job.CommodityCode);

                if (job.VariationCode != null && job.VariationCode != string.Empty)
                    newFORECAST_JOB_SETTING.VARIATION_CODE = job.VariationCode;
                else
                    newFORECAST_JOB_SETTING.VARIATION_CODE = string.Empty;

                relevantFORECAST_JOB_SETTING = newFORECAST_JOB_SETTING;
            }

            relevantFORECAST_JOB_SETTING.IS_FLOATING_PRODUCTIVITY = isFloatingProductivity;
            job.IsProductivityFloating = isFloatingProductivity;
            FORECAST_JOB_SETTINGCollectionViewModel.Save(relevantFORECAST_JOB_SETTING);
        }

        private bool commitBudget(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork, DataRow dataRow, object newValue, out List<ErrorMessage> errorMessages, bool updateView = true)
        {
            errorMessages = new List<ErrorMessage>();
            if (IsLoading)
                return false;

            decimal newDecimalValue = 0;
            if (newValue != null && decimal.TryParse(newValue.ToString(), out newDecimalValue))
            {
                ForecastJobSnapshot job = (ForecastJobSnapshot)dataRow[columnEntity];
                List<ExoSubJobProjection> projections = new List<ExoSubJobProjection>();
                projections.Add(job.ExoJob);

                List<JOBCOST_LINES> findExistingOrAddLines = ExoQueries.GetProjectLines(primeroUnitOfWork, LoadPROJECT.NUMBER, job, ExoJobLinesQueryCompliance.IgnoreStockCode);
                if (findExistingOrAddLines.Count == 0)
                {
                    IEnumerable<ExoSubJobProjection> addedProjections = ExoMethods.CommitToExo(projections, MessageBoxService, masterJob, copyLine, LoadPROJECT, USERCollection, primeroUnitOfWork, bluePrintsUnitOfWork, BulkColumnEditDialogService, out errorMessages);
                    if (errorMessages.Count > 0)
                    {
                        job.Budget = 0;
                        job.Update();
                        return false;
                    }

                    if (addedProjections.Count() > 0)
                    {
                        job.ExoJob = addedProjections.First();
                        job.Update();
                    }
                }

                bool isBudgetSet = false;
                //put the budget on the line which stock code matches commodity code and set the rest to zero budget
                foreach (JOBCOST_LINES findExistingOrAddLine in findExistingOrAddLines)
                {
                    double exoBudget = 0;
                    if (findExistingOrAddLine.STOCKCODE == job.CommodityCode)
                    {
                        //prevent duplicate entries in budget input to multiple the budget
                        if (isBudgetSet)
                            exoBudget = 0;
                        else
                        {
                            exoBudget = Convert.ToDouble(newDecimalValue);
                            isBudgetSet = true;
                        }
                    }
                    else if (findExistingOrAddLine == findExistingOrAddLines.Last() && !isBudgetSet)
                    {
                        exoBudget = Convert.ToDouble(newDecimalValue);
                        isBudgetSet = true;
                    }
                    else
                        exoBudget = Convert.ToDouble(newDecimalValue);

                    ExoMethods.UpdateJOBCOST_LINES_AUDIT(bluePrintsUnitOfWork, job.SubJobCode, job.DisciplineCode, job.CommodityCode, findExistingOrAddLine.STOCKCODE, job.VariationCode, Convert.ToDecimal(exoBudget), findExistingOrAddLine);
                    findExistingOrAddLine.QUOTE_QTY = 1;
                    findExistingOrAddLine.ACTUAL_UNITCOST = exoBudget;
                }

                primeroUnitOfWork.SaveChanges();
                job.Budget = newDecimalValue;

                job.Update();
                if (updateView)
                {
                    refreshGridData();
                    updateFloatingSummaryMembers();
                }

                return true;
            }

            return false;
        }

        public bool CanSaveSummary()
        {
            return !IsLoading;
        }

        public void SaveSummary()
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            findExistingOrAddNewEACHistory(FixedDataDateMonthEnd, ForecastSummary, bluePrintsEntitiesUnitOfWork);
            MessageBoxService.ShowMessage("Summary saved for excel for data date " + FixedDataDateMonthEnd.ToShortDateString(), "Excel Data Saved", MessageButton.OK);
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(Data.PROJECT projection, Data.PROJECT entity, bool isNewEntity)
        {
        }

        public override string UnifiedRowValidation(Data.PROJECT projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(Data.PROJECT projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Updates
        private void loadExoMethodsData()
        {
            masterJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, LoadPROJECT.NUMBER, LoadPROJECT.NUMBER);
            copyLine = ExoQueries.GetAnyProjectLineByJobNumber(primeroUnitOfWork, LoadPROJECT.NUMBER);
            projectLines = ExoQueries.GetExoSubJobProjection(primeroUnitOfWork, LoadPROJECT);
            X_PURCHORD_LINE_DETAILS = PrimeroEntities.GetPurchaseOrdersDetail(primeroUnitOfWork, LoadPROJECT.NUMBER, FixedDataDateMonthEnd);
        }

        private void loadSummaryStats()
        {
            IPrimeroEntitiesUnitOfWork threadSafePrimeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(LoadPROJECT.OfficeNameForExo).CreateUnitOfWork();
            List<ExoTimeAuthorisation> jobLines = new List<ExoTimeAuthorisation>();

            dynamic revenueLine = ExoQueries.GetProjectRevenue(threadSafePrimeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
            if (revenueLine != null)
            {
                if (LoadPROJECT.ORI_REVENUE == null)
                    LoadPROJECT.ORI_REVENUE = Convert.ToDecimal(revenueLine.BUDGETED_REV);

                savePROJECT();
            }

            FORECAST_HISTORY forecastHistory = QueryableFORECAST_HISTORYCollection.OrderByDescending(x => x.EAC_DATE).FirstOrDefault(x => x.EAC_DATE < FixedDataDateMonthEnd);
            IEnumerable<FORECAST_EAC> forecastEACs = FORECAST_EACCollection.Where(x => x.FORECAST_DATE.Date == PreviousDataDate.Date);

            if (forecastHistory != null)
            {
                ForecastSummary.Prev_Original_Revenue = forecastHistory.ORIGINAL_REVENUE;
                ForecastSummary.Prev_Approved_Variation = forecastHistory.APPROVED_VARIATION;
                ForecastSummary.Prev_Unapproved_Variation = forecastHistory.UNAPPROVED_VARIATION;
                ForecastSummary.Prev_Total_Unapproved_Variation = forecastHistory.TOTAL_UNAPPROVED_VARIATION;
                ForecastSummary.Prev_Total_EAC = forecastHistory.TOTAL_EAC == null ? forecastEACs.Sum(x => x.FORECAST_COSTS) : forecastHistory.TOTAL_EAC;
            }
            else
                ForecastSummary.Prev_Total_EAC = forecastEACs.Sum(x => x.FORECAST_COSTS);

            //dynamic revenueLine = ExoQueries.GetProjectRevenue(primeroEntitiesUnitOfWork, loadPROJECT.NUMBER);
            //if (revenueLine != null)
            ForecastSummary.Original_Revenue = LoadPROJECT.ORI_REVENUE == null ? 0 : (decimal)LoadPROJECT.ORI_REVENUE;
            ForecastSummary.Approved_Var_Revenue = LoadPROJECT.VAR_REVENUE == null || LoadPROJECT.VAR_REVENUE == 0 ? VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Approved).Sum(x => x.ManualApprovedEstimatedValue) : (decimal)LoadPROJECT.VAR_REVENUE;
            ForecastSummary.Unapproved_Var_Revenue = LoadPROJECT.UNAPPROVED_VAR_REVENUE == null ? 0 : (decimal)LoadPROJECT.UNAPPROVED_VAR_REVENUE;
            ForecastSummary.Total_Unapproved_Var_Revenue = LoadPROJECT.TOTAL_UNAPPROVED_VAR_REVENUE == null || LoadPROJECT.TOTAL_UNAPPROVED_VAR_REVENUE == 0 ? VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Submitted).Sum(x => x.ManualApprovedEstimatedValue) : (decimal)LoadPROJECT.TOTAL_UNAPPROVED_VAR_REVENUE;
            ForecastSummary.TotalClaims = ExoQueries.GetProjectClaims(threadSafePrimeroEntitiesUnitOfWork, LoadPROJECT.NUMBER);
        }

        private void updateFloatingSummaryMembers()
        {
            delayedUpdateFloatingProjectSummaryTimer.Tick -= DelayedUpdateFloatingProjectSummaryTimer_Tick;
            delayedUpdateFloatingProjectSummaryTimer.Tick += DelayedUpdateFloatingProjectSummaryTimer_Tick;
            delayedUpdateFloatingProjectSummaryTimer.Start();
        }

        private void DelayedUpdateFloatingProjectSummaryTimer_Tick(object sender, EventArgs e)
        {
            delayedUpdateFloatingProjectSummaryTimer.Stop();

            List<ForecastJobSnapshot> jobs = getJobDataFromDatatable();
            ForecastSummary.Reset();
            //cannot use parallel foreach because of inaccuracy
            foreach (ForecastJobSnapshot job in jobs)
            {
                ForecastSummary.Budget_Cost += job.Budget;
                ForecastSummary.OriginalEstimateAtCompletion += job.OriginalEstimateAtCompletion;
                ForecastSummary.Current_Cost += job.ActualCosts;
                ForecastSummary.EstimateAtCompletion += job.EstimateAtCompletion;
                ForecastSummary.CurrentEstimateAtCompletion += job.CurrentEstimateAtCompletion;
                ForecastSummary.Uncommitted_Forecast += job.Uncommitted;

                if (job.IsContingency)
                    ForecastSummary.Contingency += job.EstimateAtCompletion;
            }

            this.RaisePropertyChanged(x => x.ForecastSummary);
        }

        private List<ForecastJobSnapshot> getJobDataFromDatatable()
        {
            List<ForecastJobSnapshot> forecastJobs = new List<ForecastJobSnapshot>();
            if (dataPointsTable == null)
                return forecastJobs;
            else
            {
                IEnumerable<ForecastJobSnapshot> enumerableJobs = from DataRow dr in dataPointsTable.Rows select (ForecastJobSnapshot)dr[columnEntity];
                forecastJobs = enumerableJobs.ToList();
            }

            return forecastJobs;
        }
        #endregion

        #region Undo Redo
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

        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object oldValue = entityProperty.OldValue;
                if (oldValue == null || oldValue == DBNull.Value)
                {
                    resetViewRemainingOnJob(entityProperty.ChangedEntity, entityProperty.PropertyName, false);
                }
                else
                {
                    //do this twice so that detailed grid value can be updated (hack)
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = oldValue;
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = oldValue;
                }

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? oldValueDecimal = null;
                    if (entityProperty.OldValue != null)
                        oldValueDecimal = (decimal)entityProperty.OldValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, parseDateTime, oldValueDecimal);
                }
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                updateTotalUncommittedOnJob(entityProperty.ChangedEntity, true);
            }

            //refreshGridData();
        }

        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                object newValue = entityProperty.NewValue;
                if (newValue == null || newValue == DBNull.Value)
                {
                    resetViewRemainingOnJob(entityProperty.ChangedEntity, entityProperty.PropertyName, false);
                    //newValue = 0.00m;
                }
                else
                {
                    //do this twice so that detailed grid value can be updated (hack)
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;
                    entityProperty.ChangedEntity[entityProperty.PropertyName] = newValue;
                }

                DateTime parseDateTime;
                if (DateTime.TryParse(entityProperty.PropertyName, out parseDateTime))
                {
                    decimal? newValueDecimal = null;
                    if (entityProperty.NewValue != DBNull.Value && entityProperty.NewValue != null)
                        newValueDecimal = (decimal)entityProperty.NewValue;
                    findExistingOrAddNewForecast(entityProperty.ChangedEntity, parseDateTime, newValueDecimal);
                }
            }

            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                updateTotalUncommittedOnJob(entityProperty.ChangedEntity, true);
            }

            //refreshGridData();
        }

        private void resetViewRemainingOnJob(DataRow updateRow, string fieldName, bool addUndo)
        {
            if (updateRow[columnCompare] == DBNull.Value)
                return;

            DataTable compareDataTable = (DataTable)updateRow[columnCompare];

            decimal oldValue = 0.00m;
            decimal newValue = 0.00m;
            if (compareDataTable.Columns.Contains(fieldName))
            {
                decimal resetValue = getMasterRowResetValue(compareDataTable, fieldName);
                resetChildRow(compareDataTable, fieldName, addUndo);
                oldValue = (decimal)updateRow[fieldName];
                newValue = resetValue;

                //do it twice so that child row value can be updated (hack)
                updateRow[fieldName] = newValue;
                updateRow[fieldName] = newValue;
                EntitiesUndoRedoManager.AddUndo(updateRow, fieldName, oldValue, newValue, EntityMessageType.Changed);
                updateTotalUncommittedOnJob(updateRow, true);
            }
        }

        private void resetChildRow(DataTable compareDataTable, string dateFieldName, bool addUndo)
        {
            if (compareDataTable != null && compareDataTable.Rows.Count > 0)
            {
                if (compareDataTable.Columns.Contains(dateFieldName))
                {
                    if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                    {
                        DataRow compareP6CostsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6CostRowIndex)];
                        DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRowIndex)];

                        DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
                        DataRow compareChildP6CostsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6CostRowIndex)];
                        DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRowIndex)];

                        decimal childP6CostsValue = compareChildP6CostsRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareChildP6CostsRemainingRow[dateFieldName];
                        decimal childP6UnitsValue = compareChildP6UnitsRemainingRow[dateFieldName] == DBNull.Value ? 0 : (decimal)compareChildP6UnitsRemainingRow[dateFieldName];
                        if (addUndo)
                        {
                            //EntitiesUndoRedoManager.AddUndo(compareP6UnitsRemainingRow, dateFieldName, compareP6UnitsRemainingRow[dateFieldName], childP6CostsValue, EntityMessageType.Changed);
                            EntitiesUndoRedoManager.AddUndo(compareP6UnitsRemainingRow, dateFieldName, compareP6UnitsRemainingRow[dateFieldName], childP6UnitsValue, EntityMessageType.Changed);
                        }

                        compareP6CostsRemainingRow[dateFieldName] = childP6CostsValue;
                        compareP6UnitsRemainingRow[dateFieldName] = childP6UnitsValue;
                    }
                }
            }
        }

        /// <summary>
        /// update view and database at the same time
        /// </summary>
        /// <param name="dataRow">data row containing the job and compare info</param>
        /// <param name="forecastDate">date of the forecast to update</param>
        /// <param name="viewNewValue">determine what will be updated in db but will be replaced by null if it's same as compare info, 
        /// however if it is passed in as null it signifies that the view is already updated and won't update it</param>
        /// <param name="addUndo">whether to add undo information</param>
        private void findExistingOrAddNewForecast(DataRow dataRow, DateTime forecastDate, decimal? viewNewValue, object oldValue = null, bool skipRowSavingAndRefresh = false)
        {
            ForecastJobSnapshot job = (ForecastJobSnapshot)dataRow[columnEntity];
            string dateFieldName = forecastDate.ToString(BluePrintsResources.ColumnDateFormat);
            decimal? compareValue = null;
            decimal? saveNewValue = viewNewValue;

            //when p6 % is going through undo/redo this will happen
            if (dataRow[columnCompare].GetType() != typeof(DataTable))
                return;

            DataTable compareDataTable = (DataTable)dataRow[columnCompare];
            decimal resetValue = getMasterRowResetValue(compareDataTable, dateFieldName);
            if (resetValue > 0)
            {
                compareValue = resetValue;
            }

            if (viewNewValue != null && compareValue != null && viewNewValue == compareValue)
            {
                saveNewValue = null;
            }

            ForecastDataType editForecastDataType = job.IsP6HoursRow ? ForecastDataType.P6 : ForecastDataType.Cost;
            if (oldValue != null)
            {
                //when cost has been edited on master row, reset p6 override row
                if (editForecastDataType == ForecastDataType.Cost)
                    resetChildRow(compareDataTable, dateFieldName, true);

                EntitiesUndoRedoManager.AddUndo(dataRow, dateFieldName, oldValue, saveNewValue, EntityMessageType.Changed);
            }

            //this is definitely present because the view is generated from datecost model
            ForecastDateSnapshot dateCost = job.DateCosts.First(x => x.QueryDate == forecastDate.Date);

            IQueryable<FORECAST> findFORECASTS = QueryableFORECASTCollection.Where(x => x.FORECAST_DATE >= dateCost.MonthStartDate && x.FORECAST_DATE <= dateCost.MonthEndDate && x.SUBJOB_CODE == job.SubJobCode && x.DISCIPLINE_CODE == job.DisciplineCode && x.COMMODITY_CODE == job.CommodityCode && x.VARIATION_CODE == job.VariationCode);
            IQueryable<FORECAST> findCostFORECASTS = findFORECASTS.Where(x => x.FORECAST_TYPE == ForecastDataType.Cost);
            IQueryable<FORECAST> findP6FORECASTS = findFORECASTS.Where(x => x.FORECAST_TYPE == ForecastDataType.P6);

            List<FORECAST> costFORECASTS = findCostFORECASTS.Where(x => x.FORECAST_DATE == forecastDate.Date).ToList();
            List<FORECAST> p6FORECASTS = findP6FORECASTS.Where(x => x.FORECAST_DATE == forecastDate.Date).ToList();

            FORECAST costFORECAST = costFORECASTS.FirstOrDefault();
            FORECAST p6FORECAST = p6FORECASTS.FirstOrDefault();

            //fix duplicate entries due to concurrency issues
            foreach (FORECAST duplicateFORECAST in costFORECASTS)
            {
                if (duplicateFORECAST != costFORECAST)
                    bluePrintsUnitOfWork.FORECASTS.Remove(duplicateFORECAST);
            }

            //fix duplicate entries due to concurrency issues
            foreach (FORECAST duplicateFORECAST in p6FORECASTS)
            {
                if (duplicateFORECAST != p6FORECAST)
                    bluePrintsUnitOfWork.FORECASTS.Remove(duplicateFORECAST);
            }

            FORECAST editFORECAST = editForecastDataType == ForecastDataType.Cost ? costFORECAST : p6FORECAST;
            FORECAST resetFORECAST = editForecastDataType == ForecastDataType.Cost ? p6FORECAST : costFORECAST;

            List<FORECAST> deleteFORECASTS = new List<FORECAST>();
            deleteFORECASTS.AddRange(findCostFORECASTS.Where(x => x.FORECAST_DATE != forecastDate.Date));
            deleteFORECASTS.AddRange(findP6FORECASTS.Where(x => x.FORECAST_DATE != forecastDate.Date));

            foreach (FORECAST deleteFORECAST in deleteFORECASTS)
            {
                deleteFORECAST.FORECAST_UNITS = null;
            }

            //FORECASTCollectionViewModel.BaseBulkSave(deleteFORECASTS);

            if (editFORECAST == null)
            {
                editFORECAST = new FORECAST();
                editFORECAST.GUID = Guid.Empty;
                editFORECAST.GUID_PROJECT = LoadPROJECT.GUID;
                editFORECAST.SUBJOB_CODE = job.SubJobCode;
                editFORECAST.DISCIPLINE_CODE = DataUtils.NormalizeString(job.DisciplineCode);
                editFORECAST.COMMODITY_CODE = DataUtils.NormalizeString(job.CommodityCode);
                editFORECAST.VARIATION_CODE = DataUtils.NormalizeString(job.VariationCode);
                editFORECAST.FORECAST_DATE = forecastDate.Date;
                editFORECAST.FORECAST_UNITS = saveNewValue;
                editFORECAST.FORECAST_TYPE = editForecastDataType;
                bluePrintsUnitOfWork.FORECASTS.Add(editFORECAST);
            }
            else
            {
                editFORECAST.FORECAST_UNITS = saveNewValue;
            }

            if (resetFORECAST == null)
            {
                resetFORECAST = new FORECAST();
                DataUtils.ShallowCopy(resetFORECAST, editFORECAST);
                resetFORECAST.GUID = Guid.Empty;
                resetFORECAST.FORECAST_TYPE = editForecastDataType == ForecastDataType.Cost ? ForecastDataType.P6 : ForecastDataType.Cost;

                bluePrintsUnitOfWork.FORECASTS.Add(resetFORECAST);
            }

            //either reset p6 or cost info to null
            if (editForecastDataType == ForecastDataType.P6)
            {
                resetFORECAST.FORECAST_UNITS = saveNewValue * job.P6NominalRate;
            }
            else
                resetFORECAST.FORECAST_UNITS = null;

            //used to ensure child row is set
            if (viewNewValue != null)
            {
                dataRow[forecastDate.ToString(BluePrintsResources.ColumnDateFormat)] = viewNewValue;
            }

            if (!skipRowSavingAndRefresh)
            {
                bluePrintsUnitOfWork.SaveChanges();
                updateViewForecastsOnDatesFromDb(dataRow, true);
                updateTotalUncommittedOnJob(dataRow, true);
                updateFloatingSummaryMembers();
            }
        }

        /// <summary>
        /// Sum uncommitted values, need to be run after any updates to dates value
        /// </summary>
        private void updateTotalUncommittedOnJob(DataRow dataRow, bool searchParentRow = false)
        {
            ForecastJobSnapshot job = (ForecastJobSnapshot)dataRow[columnEntity];
            //need to map back into main row because datarow could be coming from p6 hours edit
            DataRow parentRow = searchParentRow ? findRow(job, true) : dataRow;
            dataRow = parentRow;
            job = (ForecastJobSnapshot)parentRow[columnEntity];

            DataTable dataTable = dataRow.Table;
            DataTable compareDataTable = (DataTable)dataRow[columnCompare];
            DataRow compareP6UnitsRemainingRow = compareDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompare_P6HourRowIndex)];
            DataTable compareChildDataTable = (DataTable)compareP6UnitsRemainingRow[columnCompare];
            DataRow compareChildP6UnitsRemainingRow = compareChildDataTable.Rows[Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRowIndex)];

            decimal uncommittedPOValues = 0;
            decimal uncommitedP6Values = 0;
            for (int i = 0; i < dataRow.ItemArray.Count(); i++)
            {
                DataColumn dataColumn = dataTable.Columns[i];
                string columnName = dataColumn.ColumnName;
                DateTime parseDateTime;
                if (DateTime.TryParse(columnName, out parseDateTime))
                    if (parseDateTime > FixedDataDateMonthEnd)
                        if (dataRow[columnName] != DBNull.Value && dataRow[columnName] != null)
                            if (((decimal)dataRow[columnName]) > 0)
                            {
                                decimal currentDateCellValue = (decimal)dataRow[columnName];
                                ForecastDateSnapshot dateCost = job.DateCosts.FirstOrDefault(x => x.QueryDate.Date == parseDateTime.Date);
                                if (dateCost != null)
                                {
                                    if (compareP6UnitsRemainingRow[columnName] != DBNull.Value && compareChildP6UnitsRemainingRow[columnName] != DBNull.Value)
                                    {
                                        //when current p6 units doesn't match original it means that the cost is directly contributed by P6
                                        decimal currentP6Units = (decimal)compareP6UnitsRemainingRow[columnName];
                                        decimal originalP6Units = (decimal)compareChildP6UnitsRemainingRow[columnName];

                                        if (currentP6Units == originalP6Units)
                                            uncommittedPOValues += currentDateCellValue - dateCost.CommittedCosts - dateCost.P6Costs;
                                        else
                                            uncommitedP6Values += currentDateCellValue - dateCost.CommittedCosts - dateCost.P6Costs;
                                    }
                                    else
                                        uncommittedPOValues += currentDateCellValue - dateCost.CommittedCosts - dateCost.P6Costs;
                                }
                            }
            }

            //flag procurement jobs as error when uncommitted values on dates doesn't add up to outstanding POs
            if (job.IsProcurement)
            {
                decimal differences = Math.Round(job.Outstanding) - Math.Round(job.PORemainingCosts);
                differences = Math.Abs(differences);

                decimal differencePercentage = 0;
                if (job.Outstanding > 0)
                    differencePercentage = differences / job.Outstanding;
                if (differencePercentage <= 0.01m)
                {
                    job.IsPOError = false;
                    job.JobErrorMessage = string.Empty;
                }
                else
                {
                    string strDifferencePercentage = Math.Round(differencePercentage * 100, 0).ToString();
                    job.IsPOError = true;
                    job.JobErrorMessage = "PO forecasted amount differs with outstanding amount by " + strDifferencePercentage + "%, please fix it in PO forecast";
                }
            }

            job.Uncommitted = uncommittedPOValues + uncommitedP6Values + job.P6RemainingCosts;
            if (job.CurrentProductivity > 0)
                job.CurrentUncommitted = uncommittedPOValues + uncommitedP6Values + (job.P6RemainingCosts / job.CurrentProductivity);
            else
                job.CurrentUncommitted = job.Uncommitted;

            job.OriginalUncommitted = uncommittedPOValues + uncommitedP6Values + job.P6RemainingCosts;
        }

        public bool IsManualApprovedVariationRevenue
        {
            get
            {
                if (IsLoading)
                    return false;

                return ForecastSummary.Approved_Var_Revenue != VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Approved).Sum(x => x.ManualApprovedEstimatedValue);
            }
        }

        public bool IsManualRevisedVariationRevenue
        {
            get
            {
                if (IsLoading)
                    return false;

                return ForecastSummary.Total_Unapproved_Var_Revenue != VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Submitted).Sum(x => x.ManualApprovedEstimatedValue);
            }
        }
        #endregion

        #region Filtering
        public bool IsHidden { get; set; }
        public CriteriaOperator ActualFilterCriteria { get; set; }
        public CriteriaOperator POFilterCriteria { get; set; }
        public virtual DateTime EndSelectionDate { get; set; }
        public virtual DateTime StartSelectionDate { get; set; }

        /// <summary>
        /// Because grid alternate between showing editor and focused row, use mousedown to invoke set filter
        /// </summary>
        public void MouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                TableView tableView = e.Source as TableView;
                if (tableView == null)
                    return;

                TableViewHitInfo hi = ((TableView)e.Source).CalcHitInfo(e.OriginalSource as DependencyObject);
                RowData clickRowData = tableView.FocusedRowData;
                //if(clickRowData.Row == null)
                //{
                //    GridControl masterGrid = tableView.Grid;
                //    var selected_cells = Enumerable.Range(0, masterGrid.VisibleRowCount)
                //    .Select(x => (GridControl)masterGrid.GetDetail(x))
                //    .Where(x => x != null).
                //    Select(x => ((TableView)(x).View).FocusedRowData).ToList();

                //    clickRowData = selected_cells.FirstOrDefault();
                //}

                if (clickRowData != null)
                    setFilter((DataRowView)clickRowData.Row, hi.Column);
            }
            catch (Exception ex)
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

        public IListSource ActualsDetail => instantFeedbackActualDetailViewModel.InstantFeedbackEntities;
        public List<X_PURCHORD_LINE_DETAIL> PODetail => X_PURCHORD_LINE_DETAILS;
        public Visibility ActualDetailsVisibility => !IsPoDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PODetailsVisibility => IsPoDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public bool IsPoDetailsVisible { get; set; }
        private bool isDetailBestFitApplied { get; set; }
        public int DateSortIndex => 1;
        private void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (gridColumn == null || dataRowView == null)
                return;

            DateTime parseEndDate;
            if (DateTime.TryParse(gridColumn.ActualColumnChooserHeaderCaption.ToString(), out parseEndDate))
            {
                ForecastJobSnapshot entity = (ForecastJobSnapshot)dataRowView[columnEntity];
                parseEndDate = parseEndDate.AddDays(1).AddSeconds(-1);
                EndSelectionDate = parseEndDate;

                if (IsWeeks)
                    StartSelectionDate = EndSelectionDate.AddDays(-6);
                else
                    StartSelectionDate = new DateTime(EndSelectionDate.Year, EndSelectionDate.Month, 1);

                if (parseEndDate.Date == alignedDataDateCollection.First().Date)
                {
                    if (entity.CommodityCode != string.Empty)
                        ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [COMMODITY_CODE] = '" + entity.CommodityCode + "' And [TRANSDATE] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    else
                        ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [TRANSDATE] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                }
                else
                {
                    if (entity.CommodityCode != string.Empty)
                        ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [COMMODITY_CODE] = '" + entity.CommodityCode + "' And [TRANSDATE] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [TRANSDATE] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                    else
                        ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [TRANSDATE] >= #" + StartSelectionDate.Year + "-" + StartSelectionDate.Month + "-" + StartSelectionDate.Day + "# And [TRANSDATE] <= #" + EndSelectionDate.Year + "-" + EndSelectionDate.Month + "-" + EndSelectionDate.Day + "#");
                }

                IsHidden = false;
                IsPoDetailsVisible = false;

                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("POSTDATADATE"))
            {
                DateTime dataDate = (DateTime)FixedDataDate;
                ForecastJobSnapshot entity = (ForecastJobSnapshot)dataRowView[columnEntity];
                if (entity.CommodityCode != string.Empty)
                    ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [COMMODITY_CODE] = '" + entity.CommodityCode + "' And [TRANSDATE] > #" + dataDate.Year + "-" + dataDate.Month + "-" + dataDate.Day + "#");
                else
                    ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [TRANSDATE] > #" + dataDate.Year + "-" + dataDate.Month + "-" + dataDate.Day + "#");

                IsHidden = false;
                IsPoDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("ACTUAL"))
            {
                DateTime dataDate = (DateTime)FixedDataDate;
                ForecastJobSnapshot entity = (ForecastJobSnapshot)dataRowView[columnEntity];
                if (entity.CommodityCode != string.Empty)
                    ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [COMMODITY_CODE] = '" + entity.CommodityCode + "' And [TRANSDATE] <= #" + dataDate.Year + "-" + dataDate.Month + "-" + dataDate.Day + "#");
                else
                    ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [TRANSDATE] <= #" + dataDate.Year + "-" + dataDate.Month + "-" + dataDate.Day + "#");

                IsHidden = false;
                IsPoDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("INVOICED"))
            {
                ForecastJobSnapshot entity = (ForecastJobSnapshot)dataRowView[columnEntity];
                if (entity.CommodityCode != string.Empty)
                    ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [COMMODITY_CODE] = '" + entity.CommodityCode + "' AND [InvoiceAmount] > 0.0m");
                else
                    ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' AND [InvoiceAmount] > 0.0m");

                IsHidden = false;
                IsPoDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("OUTSTANDING"))
            {
                ForecastJobSnapshot entity = (ForecastJobSnapshot)dataRowView[columnEntity];
                if (entity.CommodityCode != string.Empty)
                    POFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [COMMODITY_CODE] = '" + entity.CommodityCode + "'");
                else
                    POFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "'");
                IsHidden = false;

                IsPoDetailsVisible = true;
                this.RaisePropertyChanged(x => x.PODetail);
                this.RaisePropertyChanged(x => x.POFilterCriteria);
            }
            else
            {
                IsHidden = true;
            }

            this.RaisePropertyChanged(x => x.ActualDetailsVisibility);
            this.RaisePropertyChanged(x => x.PODetailsVisibility);
            this.RaisePropertyChanged(x => x.DateSortIndex);
        }

        public void DetailGridKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.F)
                {
                    clearFilter();
                }
            }
        }

        private void clearFilter()
        {
            IsHidden = false;

            //workaround for when detail grid doesn't show anything when it's first loaded, bug on devexpress
            ActualFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = 'x'");
            POFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = 'x'");
            this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            this.RaisePropertyChanged(x => x.POFilterCriteria);

            ActualFilterCriteria = CriteriaOperator.Parse("");
            POFilterCriteria = CriteriaOperator.Parse("");
            this.RaisePropertyChanged(x => x.IsHidden);
            this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            this.RaisePropertyChanged(x => x.POFilterCriteria);
            this.RaisePropertyChanged(x => x.ActualsDetail);
            this.RaisePropertyChanged(x => x.PODetail);
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

        public void RefreshAllForecastData()
        {
            IsLoading = true;
            Common.LoadingScreenManager.ShowLoadingScreen(1);
            //Common.LoadingScreenManager.SetMessage("Fetching P6 remaining data...");
            //await BluePrintsContextHelper.RefreshDeliverablesRemainingDataPointsByProject(LoadPROJECT.NUMBER, true);

            //Common.LoadingScreenManager.SetMessage("Fetching P6 planned data...");
            //await BluePrintsContextHelper.RefreshDeliverablesPlannedDataPointsByProject(LoadPROJECT.NUMBER, true);

            Common.LoadingScreenManager.SetMessage("Updating actuals, indirect, P6 and PO data...");
            BluePrintsContextHelper.RefreshAllForecastData(LoadPROJECT.NUMBER, FixedDataDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            resetIsLoading();
            FullRefresh();
        }

        public override void FullRefresh()
        {
            if(alignedDataDateCollection != null)
                alignedDataDateCollection.Clear();

            loadExoMethodsData();
            loadSummaryStats();

            Common.LoadingScreenManager.ShowLoadingScreen(1);
            Common.LoadingScreenManager.SetMessage("Preparing to Refresh...");
            pause();
            Common.LoadingScreenManager.CloseLoadingScreen();

            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);
            //ForecastSummary.Reset();
            base.FullRefresh();
        }

        protected override void OnClose(CancelEventArgs e)
        {
            dataModellingBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }

        /// <summary>
        /// wait for database to be written before refreshing
        /// </summary>
        private async void pause()
        {
            await Task.Delay(5000);
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

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<VARIATION_CONSTRUCTION> VARIATION_CONSTRUCTIONCollection
        {
            get
            {
                return GetEntities<VARIATION_CONSTRUCTION>();
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public IEnumerable<FORECAST_EAC> FORECAST_EACCollection
        {
            get
            {
                return GetEntities<FORECAST_EAC>().Where(x => x.TYPE == ForecastEACType.EAC);
            }
        }

        public IEnumerable<FORECAST_EAC> FORECAST_EACTenderBudgetCollection
        {
            get
            {
                return GetEntities<FORECAST_EAC>().Where(x => x.TYPE == ForecastEACType.TenderBudget);
            }
        }

        public IEnumerable<FORECAST_EAC> FORECAST_EACPreviousCommitmentCollection
        {
            get
            {
                return GetEntities<FORECAST_EAC>().Where(x => x.TYPE == ForecastEACType.PreviousCommitment);
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

        public IQueryable<FORECAST> QueryableFORECASTCollection
        {
            get
            {
                return bluePrintsUnitOfWork.FORECASTS.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
            }
        }

        public IQueryable<FORECAST_HISTORY> QueryableFORECAST_HISTORYCollection
        {
            get
            {
                return bluePrintsUnitOfWork.FORECAST_HISTORIES.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
            }
        }

        private List<X_JOBCOST_LINES_AUDIT> jobcostLinesAuditCollection;
        private List<X_JOBCOST_LINES_AUDIT> JOBCOST_LINES_AUDITCollection
        {
            get
            {
                if (jobcostLinesAuditCollection == null)
                    jobcostLinesAuditCollection = queryableJOBCOST_LINES_AUDITCollection.ToList();

                return jobcostLinesAuditCollection;
            }
        }

        private void refreshJOBCOST_LINES_AUDIT()
        {
            jobcostLinesAuditCollection = null;
            JOBCOST_LINES_AUDITCollection.ToList();
        }

        private IQueryable<X_JOBCOST_LINES_AUDIT> queryableJOBCOST_LINES_AUDITCollection
        {
            get
            {
                return bluePrintsUnitOfWork.X_JOBCOST_LINES_AUDIT.Where(x => x.JOBCODE.Contains(LoadPROJECT.NUMBER));
            }
        }

        private List<FORECAST_EAC> tenderBudgetCollection;
        private List<FORECAST_EAC> TenderBudgetCollection
        {
            get
            {
                if (tenderBudgetCollection == null)
                    tenderBudgetCollection = queryableTenderBudgetCollection.ToList();

                return tenderBudgetCollection;
            }
        }

        private void refreshTenderBudgetCollection()
        {
            tenderBudgetCollection = null;
            TenderBudgetCollection.ToList();
        }

        private IQueryable<FORECAST_EAC> queryableTenderBudgetCollection
        {
            get
            {
                return bluePrintsUnitOfWork.FORECAST_EACS.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.TYPE == ForecastEACType.TenderBudget);
            }
        }

        public IEnumerable<PROJWBS> P6PROJECTSCollection
        {
            get
            {
                var collection = GetEntities<PROJWBS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.wbs_short_name);
                return collection;
            }
        }

        public CollectionViewModel<Data.PROJECT, Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<Data.PROJECT, Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<Data.PROJECT>();
            }
        }

        public IEnumerable<FORECAST_JOB_SETTING> FORECAST_JOB_SETTINGCollection
        {
            get
            {
                return GetEntities<FORECAST_JOB_SETTING>();
            }
        }

        public CollectionViewModel<FORECAST_JOB_SETTING, FORECAST_JOB_SETTING, Guid, IBluePrintsEntitiesUnitOfWork> FORECAST_JOB_SETTINGCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<FORECAST_JOB_SETTING, FORECAST_JOB_SETTING, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<FORECAST_JOB_SETTING>();
            }
        }

        public CollectionViewModel<DISCIPLINE_DESC, DISCIPLINE_DESC, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINE_DESCCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DISCIPLINE_DESC, DISCIPLINE_DESC, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DISCIPLINE_DESC>();
            }
        }
        #endregion
    }

    public class UniqueForecastJob
    {
        public UniqueForecastJob(IEnumerable<ExoSubJobProjection> projectLines, string subJobCode, string disciplineCode, string commodityCode, string variationCode, DateTime dataDate, DateTime previousDataDate, IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> FORECAST_JOB_HOURByDataDateCollection)
        {
            SUBJOB_CODE = subJobCode;
            DISCIPLINE_CODE = disciplineCode;
            COMMODITY_CODE = commodityCode;
            VARIATION_CODE = variationCode;
            ProjectLine = projectLines.FirstOrDefault(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && x.VariationCode == variationCode);

            IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> filteredForecastJobHourSnapshot = FORECAST_JOB_HOURByDataDateCollection.Where(x => x.SUBJOB_CODE == subJobCode && x.DISCIPLINE_CODE == disciplineCode && x.COMMODITY_CODE == commodityCode && x.VARIATION_CODE == variationCode);
            AllCollection = filteredForecastJobHourSnapshot.ToList();
            BudgetCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Budget).ToList();
            PreviousActualCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual && x.FORECAST_DATE <= previousDataDate).ToList();
            ActualCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual && x.FORECAST_DATE <= dataDate).ToList();
            FutureActualCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Actual && x.FORECAST_DATE > dataDate).ToList();
            P6RemainingCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.P6Remaining).ToList();
            P6PlannedCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.P6Planned).ToList();
            PreviousPOCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.PreviousOutstandingPO).ToList();
            POCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.CurrentOutstandingPO).ToList();
            //POForecastCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.ForecastPO).ToList();
            ProgressETCCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.ProgressETC).ToList();
            EarnedCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.Earned).ToList();
            IndirectCollection = filteredForecastJobHourSnapshot.Where(x => x.SNAPSHOT_TYPE == ForecastSnapshotValueType.ForecastIndirect).ToList();
        }

        public ExoSubJobProjection ProjectLine { get; set; }
        public string SUBJOB_CODE { get; set; }
        public string DISCIPLINE_CODE { get; set; }
        public string COMMODITY_CODE { get; set; }
        public string VARIATION_CODE { get; set; }
        public decimal BudgetCosts
        {
            get
            {
                if (BudgetCollection == null || BudgetCollection.Count == 0)
                    return 0;

                return BudgetCollection.Sum(x => x.PROJECT_BUDGET);
            }
        }

        public decimal P6BudgetHours
        {
            get
            {
                if (P6PlannedCollection == null || P6PlannedCollection.Count == 0)
                    return 0;

                return P6PlannedCollection.Sum(x => x.FORECAST_QTY);
            }
        }

        public decimal PreviousPOOutstandingCosts
        {
            get
            {
                if (PreviousPOCollection == null || PreviousPOCollection.Count == 0)
                    return 0;

                return PreviousPOCollection.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal POOutstandingCosts
        {
            get
            {
                if (POCollection == null || POCollection.Count == 0)
                    return 0;

                return POCollection.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal POForecastCosts
        {
            get
            {
                if (POForecastCollection == null || POForecastCollection.Count == 0)
                    return 0;

                return POForecastCollection.Sum(x => x.FORECAST_COST);
            }
        }

        public decimal P6RemainingHours
        {
            get
            {
                if (P6RemainingCollection == null || P6RemainingCollection.Count == 0)
                    return 0;

                return P6RemainingCollection.Sum(x => x.FORECAST_QTY);
            }
        }

        public decimal P6RemainingCosts
        {
            get
            {
                if (P6RemainingCollection == null || P6RemainingCollection.Count == 0)
                    return 0;

                return P6RemainingCollection.Sum(x => x.FORECAST_COST);
            }
        }

        public void UpdateTenderBudget(IQueryable<FORECAST_EAC> tenderBudgetCollection)
        {
            if (tenderBudgetCollection == null || tenderBudgetCollection.Count() == 0)
                tenderBudget = 0;

            FORECAST_EAC findTenderBudget = tenderBudgetCollection.FirstOrDefault(x => x.SUBJOB_CODE == SUBJOB_CODE && x.DISCIPLINE_CODE == DISCIPLINE_CODE && x.COMMODITY_CODE == COMMODITY_CODE && x.VARIATION_CODE == VARIATION_CODE);
            if (findTenderBudget != null && findTenderBudget.FORECAST_COSTS != null)
                tenderBudget = (decimal)findTenderBudget.FORECAST_COSTS;
            else
                tenderBudget = 0;
        }

        private decimal tenderBudget;
        public decimal TenderBudget => tenderBudget;
        public List<FORECAST_JOB_HOUR_SNAPSHOT> AllCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> BudgetCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> PreviousActualCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> ActualCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> FutureActualCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> P6RemainingCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> P6PlannedCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> PreviousPOCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> POCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> EarnedCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> POForecastCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> IndirectCollection { get; set; }
        public List<FORECAST_JOB_HOUR_SNAPSHOT> ProgressETCCollection { get; set; }

        public FORECAST_JOB_HOUR_SNAPSHOT ForecastJob
        {
            get
            {
                if (BudgetCollection.Count > 0)
                    return BudgetCollection.First();
                else if (ActualCollection.Count > 0)
                    return ActualCollection.First();
                else //this property can only be constructed by either of these 3 category so it must exist in P6 collection
                    return P6RemainingCollection.First();
            }
        }

        public void UpdateErrorMessage(IQueryable<X_JOBCOST_LINES_AUDIT> JOBCOST_LINES_AUDITS)
        {
            bool isExistInBudget = ProjectLine != null;
            bool isExistInCacheBudget = BudgetCollection.Count > 0;
            bool isExistInActuals = ActualCollection.Count > 0;
            bool isExistInRemaining = P6RemainingCollection.Count > 0;
            string possibleErrorMessage = string.Empty;

            if (isExistInBudget)
            {
                IEnumerable<X_JOBCOST_LINES_AUDIT> findJOBCOST_LINES_AUDIT = JOBCOST_LINES_AUDITS.Where(x => x.JOBCODE == SUBJOB_CODE && x.DISCIPLINE_CODE == DISCIPLINE_CODE && x.COMMODITY_CODE == COMMODITY_CODE && x.VARIATION_CODE == VARIATION_CODE).OrderByDescending(x => x.UPDATED).ThenBy(x => x.CREATED);
                X_JOBCOST_LINES_AUDIT createdJOBCOST_LINES_AUDIT = findJOBCOST_LINES_AUDIT.FirstOrDefault(x => x.BUDGET_UPDATED == null);
                X_JOBCOST_LINES_AUDIT updatedJOBCOST_LINES_AUDIT = findJOBCOST_LINES_AUDIT.FirstOrDefault(x => x.BUDGET_UPDATED != null);

                if (!isExistInCacheBudget)
                {
                    if (createdJOBCOST_LINES_AUDIT != null && updatedJOBCOST_LINES_AUDIT != null)
                        possibleErrorMessage = "Job is added since last data refresh\nJob was created by " + createdJOBCOST_LINES_AUDIT.CREATED_BY_USER + "\nBudget was updated by: " + updatedJOBCOST_LINES_AUDIT.BUDGET_UPDATED_BY_USER;
                    else if (createdJOBCOST_LINES_AUDIT != null)
                        possibleErrorMessage = "Job is added since last data refresh\nJob was created by " + createdJOBCOST_LINES_AUDIT.CREATED_BY_USER;
                    else if (updatedJOBCOST_LINES_AUDIT != null)
                        possibleErrorMessage = "Job is added since last data refresh\nBudget was updated by " + updatedJOBCOST_LINES_AUDIT.BUDGET_UPDATED_BY_USER;
                    else
                        possibleErrorMessage = "Job is added since last data refresh\nBudget was created in EXO and not tracked by BluePrints";
                }
                else if (Math.Round(ProjectLine.ExoBudget, 0) != Math.Round(BudgetCosts, 0))
                {
                    possibleErrorMessage = "Exo budget doesn't match previously saved budget of " + BudgetCosts;
                    if (updatedJOBCOST_LINES_AUDIT != null)
                        possibleErrorMessage += "\nBudget was updated by " + updatedJOBCOST_LINES_AUDIT.BUDGET_UPDATED_BY_USER;
                }
            }
            else
            {
                if (isExistInActuals && isExistInRemaining)
                    possibleErrorMessage = "Job have actuals and remaining costs";
                else if (isExistInActuals)
                    possibleErrorMessage = "Job have actuals";
                else if (isExistInRemaining)
                    possibleErrorMessage = "Job have remaining costs";

                if (possibleErrorMessage != string.Empty)
                    possibleErrorMessage += " but its not budgeted, please add the budget in Budget Input";
            }

            errorMessage = possibleErrorMessage;
        }

        string errorMessage;
        public string ErrorMessage => errorMessage;
    }
}