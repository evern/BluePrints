using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Helpers;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.Common.Base
{
    public abstract class BluePrintsEntitiesProgressCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : BluePrintsEntitiesCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>
        where TMainEntity : class, IGuidEntityKey, IDeliverable, new()
        where TMainProjectionEntity : class, IGuidEntityKey, ICanAssignP6, IReportable, IBookable, new()
        where TMainEntityUnitOfWork : IUnitOfWork
    {
        #region Initialization
        protected Data.PROJECT loadPROJECT;
        public P6Data.PROJECT p6PROJECT;
        protected PROGRESS loadPROGRESS;
        protected BASELINE liveBASELINE;
        protected bool isQueryForLiveStatus;
        protected abstract CostGroup cost_group { get; }
        protected abstract PhaseType progress_type { get; }
        public bool IsExoDataLoaded => exoAuthorisations != null;
        protected List<ExoTimeAuthorisation> exoAuthorisations = null;
        //ensure mainviewmodel is loaded before calling background worker
        protected DispatcherTimer onMainViewModelFirstLoadedTimer;
        //calculates the planned values only for each deliverables
        protected BackgroundWorker calculatePlannedBackgroundWorker;
        protected BackgroundWorker loadExoBackgroundWorker;
        protected BackgroundWorker updateP6DatesBackgroundWorker;
        //set current data date timer
        protected DispatcherTimer delayedPROGRESSSavingDispatcher;
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        IBluePrintsEntitiesUnitOfWork bluePrintsUOW;
        IP6EntitiesUnitOfWork p6UOW;
        protected IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected bool is_single_project_mode = true;
        protected bool is_load_p6_task = false;
        protected bool isUseReportDate = false;
        protected bool canDateBackwardForward = false;
        BackgroundWorker progressSaveBackgroundWorker;
        public BluePrintsEntitiesProgressCollectionWrapper()
        {
            onMainViewModelFirstLoadedTimer = new DispatcherTimer();
            onMainViewModelFirstLoadedTimer.Interval = new TimeSpan(0, 0, 0, 1);
            onMainViewModelFirstLoadedTimer.Tick += onMainViewModelFirstLoaded;
            calculatePlannedBackgroundWorker = new BackgroundWorker();
            calculatePlannedBackgroundWorker.DoWork += calculatePlannedBackgroundWorker_DoWork;
            calculatePlannedBackgroundWorker.RunWorkerCompleted += calculatePlannedBackgroundWorker_RunWorkerCompleted;
            calculatePlannedBackgroundWorker.WorkerSupportsCancellation = true;

            loadExoBackgroundWorker = new BackgroundWorker();
            loadExoBackgroundWorker.DoWork += loadExoBackgroundWorker_DoWork;
            loadExoBackgroundWorker.RunWorkerCompleted += loadExoBackgroundWorker_RunWorkerCompleted;
            loadExoBackgroundWorker.WorkerSupportsCancellation = true;

            progressSaveBackgroundWorker = new BackgroundWorker();
            progressSaveBackgroundWorker.DoWork += ProgressSaveBackgroundWorker_DoWork;
            progressSaveBackgroundWorker.WorkerSupportsCancellation = true;

            updateP6DatesBackgroundWorker = new BackgroundWorker();
            updateP6DatesBackgroundWorker.DoWork += updateP6DatesBackgroundWorker_DoWork;
            updateP6DatesBackgroundWorker.RunWorkerCompleted += updateP6DatesBackgroundWorker_RunWorkerCompleted;
            updateP6DatesBackgroundWorker.WorkerSupportsCancellation = true;
        }

        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            var receiveParameter = (DualEntitiesParameter<Data.PROJECT, PROGRESS>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadPROGRESS = receiveParameter.GetSecondEntity();

            bool? allowSummaryCalculationOnUpdatePreference = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.DesignProgress_AllowSummaryUpdate));
            allowSummaryCalculationOnUpdate = allowSummaryCalculationOnUpdatePreference == null ? true : (bool)allowSummaryCalculationOnUpdatePreference;

            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo).CreateUnitOfWork();
            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        protected override void addEntitiesLoader()
        {
            bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            p6UOW = p6UnitOfWorkFactory.CreateUnitOfWork();

            if (is_single_project_mode)
            {
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, SetPROGRESStoCurrentDateOnLoaded);
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, false);
            }

            if (is_load_p6_task)
            {
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.P6_ASSIGNMENTS, P6_ASSIGNMENTProjectionFunc);
                loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.TASK, P6TASKProjectionFunc);
            }

            loaderCollection.AddLoaderDescription<Data.PHASE, Data.PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ETCS, PROGRESS_ETCProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        protected Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.APPROVED != null && x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>> P6_ASSIGNMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<P6Data.TASK>, IQueryable<P6Data.TASK>> P6TASKProjectionFunc()
        {
            if (loadPROGRESS.P6PROGRESS_NAME == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return query => query.Where(x => x.proj_id == 0);
            else
                return query => query.Where(x => x.PROJECT.proj_short_name == loadPROGRESS.P6PROGRESS_NAME && x.delete_date == null).Where(x => x.TASKACTV.Count > 0).Where(x => x.delete_date == null).Where(x => x.TASKACTV.Any(taskact => taskact.ACTVCODE != null && taskact.ACTVCODE.actv_code_name.ToUpper() == ProgressType.Design.ToString().ToUpper()));
        }

        private Func<IRepositoryQuery<P6Data.PROJECT>, IQueryable<P6Data.PROJECT>> P6PROJECTProjectionFunc()
        {
            string projectName;
            if (loadPROGRESS.P6PROGRESS_NAME == null)
                projectName = string.Empty;
            else
                projectName = loadPROGRESS.P6PROGRESS_NAME;

            return query => query.Where(x => x.proj_short_name == projectName);
        }

        protected Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == progress_type && x.STATUS == ProgressStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID && x.TYPE == progress_type);
        }

        protected virtual Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected virtual Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.SUBJOB.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected virtual Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
            else
                return query => query.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected Func<IRepositoryQuery<PROGRESS_ETC>, IQueryable<PROGRESS_ETC>> PROGRESS_ETCProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
            else
                return query => query.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Progress_Report.ToString());
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainProjectionEntity> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = OnAfterEntitySavedCallBack;
            MainViewModel.OnMappingAdditionalChangedEntitiesProperties = OnMappingAdditionalChangedEntitiesProperties;
            MainViewModel.OnBeforeAssignRepositoryToExistingProjection = OnBeforeAssignRepositoryToExistingProjection;
            MainViewModel.DisablePasteRowLevel = true;
            MainViewModel.AlwaysSkipMessage = true;
            PROGRESS_ITEMSCollectionViewModel.SetParentViewModel(this);

            MainViewModel.SetParentViewModel(this);
            //mainThreadDispatcher.BeginInvoke(new Action(() => InitializeSummarizer(entities)));
            onMainViewModelFirstLoadedTimer.Start();
            isFirstLoaded = true;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void loadExoBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (loadExoBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            if (MainViewModel == null)
                return;

            BluePrintsUtils.LoadExoAuthorisation<TMainProjectionEntity>(Entities, ref exoAuthorisations, getProjectContexts(), getContextUserIds());
        }

        private void loadExoBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.RaisePropertyChanged(x => x.IsExoDataLoaded);
        }

        List<ProjectUnitOfWorkContext> projectContexts;
        private List<ProjectUnitOfWorkContext> getProjectContexts()
        {
            if (projectContexts == null)
            {
                IPrimeroEntitiesUnitOfWork perthUOW = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
                IPrimeroEntitiesUnitOfWork montrealUOW = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
                projectContexts = new List<ProjectUnitOfWorkContext>();
                foreach (var entity in MainViewModel.Entities)
                {
                    if (!projectContexts.Any(x => x.ProjectNumber == entity.Project_Number))
                    {
                        Data.PROJECT project = bluePrintsUOW.PROJECTS.FirstOrDefault(x => x.NUMBER == entity.Project_Number);
                        IPrimeroEntitiesUnitOfWork uow;
                        if (project != null)
                        {
                            if (project.OfficeNameForExo == BluePrintsResources.OfficeMontreal)
                                uow = montrealUOW;
                            else
                                uow = perthUOW;

                            projectContexts.Add(new ProjectUnitOfWorkContext(project, uow));
                        }
                    }
                }
            }

            return projectContexts;
        }

        List<UserIdsAuthorisationContext> userIdContexts;
        private List<UserIdsAuthorisationContext> getContextUserIds()
        {
            if (userIdContexts == null)
            {
                userIdContexts = new List<UserIdsAuthorisationContext>();
                foreach (var entity in MainViewModel.Entities)
                {
                    Data.PROJECT project = bluePrintsUOW.PROJECTS.FirstOrDefault(x => x.NUMBER == entity.Project_Number);
                    if (project != null)
                    {
                        int? userIdForAuthorisation = null;
                        if (project.OfficeNameForExo == BluePrintsResources.OfficeMontreal)
                            userIdForAuthorisation = LoginCredentials.CurrentUser.EXO_STAFF_ID_REMOTE;
                        else
                            userIdForAuthorisation = LoginCredentials.CurrentUser.EXO_STAFF_ID;

                        if (!userIdContexts.Any(x => x.Id == userIdForAuthorisation))
                            userIdContexts.Add(new UserIdsAuthorisationContext(project.OfficeNameForExo, userIdForAuthorisation));
                    }
                }
            }

            return userIdContexts;
        }

        //when the inherited view model have group entity, OnBeforeEntitySavedCallBack will be used instead of OnAfterEntitySavedCallBack to identify whether the edited entity is group
        protected abstract bool manuallySaveProgressOnAfterBaselineItemSaved { get; }
        /// <summary>
        /// Save progress item during BASELINE_ITEM Undo/Redo operation
        /// </summary>
        /// <param name="projectionEntity"></param>
        /// <param name="isNewEntity"></param>
        protected virtual void OnAfterEntitySavedCallBack(TMainProjectionEntity projectionEntity, TMainEntity entity, bool isNewEntity)
        {
            //because undo/redo operation still relies on mainviewmodel progress needs to be re-checked even though we had onExistingRowAddUndoAndSaveIsContinue
            if (!manuallySaveProgressOnAfterBaselineItemSaved && projectionEntity.ShouldSaveProgress)
            {
                saveProgressItem(projectionEntity);
            }

            if (!manuallySaveProgressOnAfterBaselineItemSaved && projectionEntity.ShouldSaveProgressETC)
            {
                saveProgressETC(projectionEntity);
            }
        }

        private void saveProgressItem(TMainProjectionEntity projection)
        {
            IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = projection.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
            PROGRESS_ITEMSCollectionViewModel.Save(newPRORESS_ITEMS.First());
        }

        private void saveProgressETC(TMainProjectionEntity projection)
        {
            IEnumerable<PROGRESS_ETC> newPROGRESS_ETCs = projection.GetExistingOrNewEditedProgressETCs(PROGRESS_ETCSCollectionViewModel.FindActualProjectionByExpression);
            PROGRESS_ETCSCollectionViewModel.Save(newPROGRESS_ETCs.First());
        }

        private void ProgressSaveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (progressSaveBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            TMainProjectionEntity projectionEntity = (TMainProjectionEntity)e.Argument;
            IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = projectionEntity.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
            PROGRESS_ITEMSCollectionViewModel.Save(newPRORESS_ITEMS.First());
        }

        protected virtual void OnMappingAdditionalChangedEntitiesProperties(TMainProjectionEntity existingProjectionEntity, TMainProjectionEntity projectionEntity)
        {
            projectionEntity.Stats = existingProjectionEntity.Stats;
        }

        protected void OnBeforeAssignRepositoryToExistingProjection(TMainProjectionEntity existingProjection, TMainProjectionEntity repositoryProjection)
        {
            repositoryProjection.Stats = existingProjection.Stats;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, TMainProjectionEntity projection, bool isNew)
        {
            //set this to let the gridcontrol knows that the next custom summary calculation is due to value changes
            isSummaryRecalculation = true;
            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedValueValidation(TMainProjectionEntity projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage))
            {
                var newPercentage = (decimal)new_value;
                if (newPercentage > projection.MaxPercentage)
                    return "Percentage on " + projection.Deliverable_Name + " cannot exceed " + projection.MaxPercentage.ToString();
                else if (newPercentage < projection.MinPercentage)
                    return "Percentage on " + projection.Deliverable_Name + " cannot be less than " + projection.MinPercentage.ToString();
            }

            return string.Empty;
        }

        public override string UnifiedRowValidation(TMainProjectionEntity projection)
        {
            return string.Empty;
        }
        #endregion

        #region Stats Calculation
        protected FullSummarizer fullSummarizer;
        protected ProjectSummaryStats projectSummary;
        protected bool statsCalculatedOnProjection = false;
        protected bool skipExoDataLoading = false;
        protected virtual void onMainViewModelFirstLoaded(object sender, EventArgs e)
        {
            onMainViewModelFirstLoadedTimer.Stop();
            if (!statsCalculatedOnProjection)
            {
                InitializeSummarizer();
                //IsLoading = true;
                //this.RaisePropertyChanged(x => x.IsLoading);
                IsCalculationCompleted = false;
                this.RaisePropertyChanged(x => x.IsCalculationCompleted);
                if (!skipExoDataLoading && !loadExoBackgroundWorker.IsBusy)
                    loadExoBackgroundWorker.RunWorkerAsync();

                liveBASELINE = bluePrintsUOW.BASELINES.FirstOrDefault(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
                if (!updateP6DatesBackgroundWorker.IsBusy)
                    updateP6DatesBackgroundWorker.RunWorkerAsync(new object[] { liveBASELINE, loadPROGRESS, p6UOW });

                if (!calculatePlannedBackgroundWorker.IsBusy)
                    calculatePlannedBackgroundWorker.RunWorkerAsync();
            }
        }

        protected bool extrapolateDataDate = false;
        protected virtual void InitializeSummarizer()
        {
            //when view is closed too fast
            if (MainViewModel == null)
                return;

            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            List<VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONCollection.AsQueryable(), ReportableCollection);
            projectSummary = new ProjectSummaryStats(MainViewModel.Entities, DataDate, reportInterval, firstAlignedDataDate, projectVariationAdjustment, extrapolateDataDate ? DateTime.Now : (DateTime?)null);
            DateTime reporting_data_date = DataDate;
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT.NUMBER, loadPROJECT.CURRENCYCONVERSION, reportInterval, firstAlignedDataDate, SUBJOBCollection, reporting_data_date, primeroUnitOfWork);
            fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, loadPROJECT.NUMBER);
        }

        protected abstract IEnumerable<IReportable> ReportableCollection { get; }

        protected void calculatePlannedBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (calculatePlannedBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            BackgroundWorkerBuildStats();
        }

        protected virtual void BackgroundWorkerBuildStats()
        {
            if (fullSummarizer != null)
            {
                fullSummarizer.BuildBudgeted(1, 1, false, false);
                //fullSummarizer.BuildEarned();
                fullSummarizer.BuildRemaining();
                fullSummarizer.BuildBurnedDataPoints(false, false, false, false, true);
                //fullSummarizer.Summarize();
                //mainThreadDispatcher.BeginInvoke(new Action(() => BackgroundRefresh()));

                //when user closes the view before it gets the chance to update
                if(Entities != null)
                    foreach (var deliverable in Entities)
                    {
                        deliverable.Update();
                    }
            }
        }

        public bool IsCalculationCompleted { get; set; }
        protected void calculatePlannedBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (calculatePlannedBackgroundWorker.CancellationPending)
                return;

            IsCalculationCompleted = true;
            onCalculatePlannedBackgroundWorkerCompleted();
            this.RaisePropertyChanged(x => x.IsCalculationCompleted);
        }

        protected virtual void onCalculatePlannedBackgroundWorkerCompleted()
        {

        }
        #endregion

        #region Set current data date
        protected void SetPROGRESStoCurrentDateOnLoaded(PROGRESS entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live progress not found")));

            loadPROGRESS = entity;
            if (!isFirstLoaded)
                mainThreadDispatcher.BeginInvoke(new Action(() => DateChange(DateNavigationType.Current)));
        }

        protected virtual void delayedPROGRESSSavingDispatcher_Tick(object sender, EventArgs e)
        {
            delayedPROGRESSSavingDispatcher.Stop();
            var PROGRESSCollectionViewModel = (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS>();
            mainThreadDispatcher.BeginInvoke(new Action(() => PROGRESSCollectionViewModel.Save(loadPROGRESS)));
            FullRefresh();
        }

        public DateTime DataDate
        {
            get => loadPROGRESS == null ? DateTime.Now : isUseReportDate ? loadPROGRESS.REPORT_DATE == null ? loadPROGRESS.DATA_DATE : (DateTime)loadPROGRESS.REPORT_DATE : loadPROGRESS.DATA_DATE;
            set
            {
                if (!IsLoading && isUseReportDate)
                {
                    DateTime newValue = value.Date.AddDays(1).AddSeconds(-1);

                    if (isUseReportDate)
                    {
                        if (loadPROGRESS.REPORT_DATE == newValue)
                            return;

                        loadPROGRESS.REPORT_DATE = newValue;
                    }
                    else
                    {
                        if (loadPROGRESS.DATA_DATE == newValue)
                            return;

                        loadPROGRESS.DATA_DATE = newValue;
                    }

                    //!= null is used because set method can be invoked in quick succession (when full refresh is called and PROGRESSCollectionViewModel is disposed)
                    if (PROGRESSCollectionViewModel != null)
                    {
                        PROGRESSCollectionViewModel.Save(loadPROGRESS);
                        this.RaisePropertyChanged(x => x.DataDate);
                        this.RaisePropertyChanged(x => x.DataDateStr);
                        FullRefresh();
                    }
                }
            }
        }

        public override bool CanExportToPDF()
        {
            if (!IsCalculationCompleted)
                return false;

            return base.CanExportToPDF();
        }

        public override bool CanExportToExcel()
        {
            if (!IsCalculationCompleted)
                return false;

            return base.CanExportToExcel();
        }

        public override bool CanFullRefresh()
        {
            if (IsLoading || !IsCalculationCompleted)
                return false;

            return base.CanFullRefresh();
        }

        public override void FullRefresh()
        {
            calculatePlannedBackgroundWorker.CancelAsync();
            progressSaveBackgroundWorker.CancelAsync();
            loadExoBackgroundWorker.CancelAsync();
            IsCalculationCompleted = false;
            this.RaisePropertyChanged(x => x.IsCalculationCompleted);

            base.FullRefresh();
        }

        public string DataDateStr => DataDate.ToString("dd-MMM-yy");

        public override bool CanKeyboardCopy()
        {
            if (IsLoading)
                return false;

            return base.CanKeyboardCopy();
        }

        public override bool CanKeyboardPaste()
        {
            if (IsLoading)
                return false;

            return base.CanKeyboardPaste();
        }

        public override bool CanSaveLayout()
        {
            if (IsLoading)
                return false;

            return base.CanSaveLayout();
        }

        public bool CanDateBackward()
        {
            if (!IsCalculationCompleted)
                return false;

            if (loadPROGRESS.DATA_DATE > loadPROGRESS.PROGRESS_START)
                return true;

            return false;
        }

        public bool CanDateForward()
        {
            if (!IsCalculationCompleted)
                return false;

            return true;
        }

        public void DateForward()
        {
            DateChange(DateNavigationType.Forward);
        }

        public void DateBackward()
        {
            DateChange(DateNavigationType.Backward);
        }

        protected void DateChange(DateNavigationType navigationType)
        {
            //if (!IsCalculationCompleted)
            //    return;

            if (BluePrintsUtils.ProgressDateChange(navigationType, loadPROGRESS, isUseReportDate))
                delayedPROGRESSSavingDispatcher.Start();
        }
        #endregion

        #region Refresh
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if (senderKey != PROGRESS_ITEMSCollectionViewModel.Key)
                return false;

            if (changedType == typeof(PROGRESS_ITEM))
            {
                PROGRESS_ITEM newPROGRESSITEM = PROGRESS_ITEMCollection.FirstOrDefault(x => x.GUID == (Guid)key);
                if (newPROGRESSITEM != null)
                {
                    IReportable affectedDisplayEntity = getAffectedDisplayEntity(newPROGRESSITEM);
                    if (affectedDisplayEntity != null)
                        affectedDisplayEntity.Update();
                }

                return true;
            }

            return false;
        }

        private IReportable getAffectedDisplayEntity(PROGRESS_ITEM newPROGRESS_ITEM)
        {
            foreach (IReportable entity in MainViewModel.Entities)
            {
                if (entity.OriginalEntityKey == newPROGRESS_ITEM.GUID_ORIBASEITEM)
                {
                    setReportableNewProgress(entity, newPROGRESS_ITEM);
                    return entity;
                }
            }

            return null;
        }

        private void setReportableNewProgress(IReportable updateEntity, PROGRESS_ITEM newPROGRESS_ITEM)
        {
            if (updateEntity.PROGRESS_ITEM_Current == null)
            {
                updateEntity.AppendProgressItem(newPROGRESS_ITEM);
            }
        }
        #endregion

        #region Book Time
        public bool CanShowBookable()
        {
            if (MainViewModel == null || SelectedEntities == null || SelectedEntities.Count() == 0 || exoAuthorisations == null)
                return false;

            return true;
        }

        bool showBookable;
        public bool ShowBookable
        {
            get
            {
                return showBookable;
            }
            set
            {
                showBookable = value;
                BluePrintsUtils.ApplyShowBookableFilter(GridControlService, value);
            }
        }
        #endregion

        #region Progress P6
        public bool CanPushToP6Original()
        {
            return CanPushToP6();
        }

        public bool CanPushToP6Modified()
        {
            return CanPushToP6();
        }

        private bool CanPushToP6()
        {
            if (IsLoading || isPushingToP6 || loadPROGRESS == null || loadPROGRESS.P6PROGRESS_NAME == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return false;

            return true;
        }

        public void PushToP6Original()
        {
            PushToP6(BaselineMappingSelectionType.Original);
        }

        public void PushToP6Modified()
        {
            PushToP6(BaselineMappingSelectionType.Modified);
        }

        public bool IsInteractWithP6Visible
        {
            get
            {
                return true;
            }
        }

        public bool CanInteractWithP6(object button)
        {
            if (isInteractingWithP6 || loadPROGRESS == null || loadPROGRESS.P6PROGRESS_NAME == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return false;

            return true;
        }

        bool isInteractingWithP6;
        public void InteractWithP6(object button)
        {
            BarButtonItem contextButton = button as BarButtonItem;
            if (contextButton == null)
                return;

            isInteractingWithP6 = true;
            bool isSync = contextButton.Content.ToString().ToUpper().Contains("SYNC");
            DateTime dateToFreeze = loadPROGRESS.PREVIOUS_REPORT_DATE == null ? loadPROGRESS.REPORT_DATE == null ? loadPROGRESS.PROGRESS_START : (DateTime)loadPROGRESS.REPORT_DATE : (DateTime)loadPROGRESS.PREVIOUS_REPORT_DATE;
            if (MessageBoxService.ShowMessage("Datapoints between " + dateToFreeze.ToShortDateString() + " and " + loadPROGRESS.DATA_DATE.ToShortDateString() + " will be edited, do you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            if (loadPROGRESS.P6PROGRESS_NAME == string.Empty)
            {
                MessageBoxService.ShowMessage("Please define P6 progress schedule", BluePrintsResources.Warning_Caption, MessageButton.OK);
                return;
            }

            PROGRESS backupPROGRESS = new PROGRESS();
            DataUtils.ShallowCopy(backupPROGRESS, loadPROGRESS);
            backupPROGRESS.GUID = Guid.Empty;
            backupPROGRESS.NAME = "BACKUP " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString();
            backupPROGRESS.STATUS = ProgressStatus.Superseded;
            bluePrintsUOW.PROGRESSES.Add(backupPROGRESS);
            //need to save progress to get GUID
            bluePrintsUOW.SaveChanges();

            LoadingScreenManager.ShowLoadingScreen(loadPROGRESS.PROGRESS_ITEM.Count());
            LoadingScreenManager.SetMessage("Phase 1 of 2: Creating Backup of Progress");

            if (loadPROGRESS.PROGRESS_ITEM != null)
                foreach (PROGRESS_ITEM progress_item in loadPROGRESS.PROGRESS_ITEM)
                {
                    PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                    DataUtils.ShallowCopy(newPROGRESS_ITEM, progress_item);
                    newPROGRESS_ITEM.GUID = Guid.Empty;
                    newPROGRESS_ITEM.GUID_PROGRESS = backupPROGRESS.GUID;
                    bluePrintsUOW.PROGRESS_ITEMS.Add(newPROGRESS_ITEM);
                    LoadingScreenManager.Progress();
                }

            bluePrintsUOW.SaveChanges();
            LoadingScreenManager.CloseLoadingScreen();
            //MessageBoxService.ShowMessage("Progress backup completed");

            //Stats will be built in SummarizeSinglePROJECTDashboard within SummarizeBASELINE_ITEMDashboard in ConstructMainViewModelProjection
            MainViewModel.Refresh();
            if (isSync)
                scheduling_view_model.OnViewModelLoaded = SyncProgressP6_Loaded;
            else
                scheduling_view_model.OnViewModelLoaded = ReverseProgressP6_Loaded;

            scheduling_view_model.OnViewModelLoadFailed = onSchedulingViewModelLoadFailed;
            var ParameterObj = scheduling_view_model as ISupportParameter;
            ParameterObj.Parameter = new object[] { loadPROGRESS, BaselineMappingSelectionType.Original };
        }

        public void SyncProgressP6_Loaded(IEnumerable<ICanAssignP6> entities)
        {
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);

            //specify the oldest date data points can edit to
            DateTime progressLowerLimitDate = loadPROGRESS.PREVIOUS_REPORT_DATE == null ? loadPROGRESS.REPORT_DATE == null ? loadPROGRESS.PROGRESS_START : (DateTime)loadPROGRESS.REPORT_DATE : (DateTime)loadPROGRESS.PREVIOUS_REPORT_DATE;
            TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            progressLowerLimitDate.AddDays(interval.Days);

            foreach (ICanAssignP6 deliverable in entities)
            {
                foreach (P6_ASSIGNMENT assignment in deliverable.P6_Assignments)
                {
                    TASK task = PROJECTTASK.FirstOrDefault(x => x.task_code == assignment.P6_ACTIVITYID);
                    DateTime? startDateToUse = task.act_start_date == null ? task.target_start_date : task.act_start_date;

                    if (deliverable.TaskAssignmentStartDate == null)
                        deliverable.TaskAssignmentStartDate = startDateToUse;
                    else if (deliverable.TaskAssignmentStartDate < startDateToUse)
                        deliverable.TaskAssignmentStartDate = startDateToUse;
                }
            }

            List<string> processedTasks = new List<string>();
            LoadingScreenManager.ShowLoadingScreen(entities.Count());
            LoadingScreenManager.SetMessage("Phase 2 of 2: Fixing P6 progress and sync");
            foreach (ICanAssignP6 deliverable in entities.OrderByDescending(x => x.TaskAssignmentStartDate))
            {
                //check if all deliverables tasks is mark as not started
                List<TASK> allAssignmentTasks = new List<TASK>();
                foreach (P6_ASSIGNMENT assignment in deliverable.P6_Assignments.OrderByDescending(x => x.HIGH_VALUE))
                {
                    TASK task = PROJECTTASK.FirstOrDefault(x => x.task_code == assignment.P6_ACTIVITYID);
                    if (task != null)
                        allAssignmentTasks.Add(task);
                }

                if (allAssignmentTasks.All(x => x.act_work_qty == 0))
                {
                    decimal allEarnedUnits = deliverable.Progresses.Sum(x => x.EARNED_UNITS);
                    allEarnedUnits *= -1;
                    removeOrReduceDataPointsForTasks(deliverable, allEarnedUnits);
                    bluePrintsUOW.SaveChanges();
                }
                else
                {
                    foreach (P6_ASSIGNMENT assignment in deliverable.P6_Assignments.OrderByDescending(x => x.HIGH_VALUE))
                    {
                        List<P6_ASSIGNMENT> allCompletedAssignments = new List<P6_ASSIGNMENT>();
                        TASK task = PROJECTTASK.FirstOrDefault(x => x.task_code == assignment.P6_ACTIVITYID);

                        //use repository task because iterations has been updating it
                        TASK repositoryTASK = p6UOW.TASK.First(x => x.task_id == task.task_id);
                        if (repositoryTASK != null && repositoryTASK.target_work_qty != null)
                        {
                            if (repositoryTASK.act_work_qty > 0)
                            {
                                //assuming previous assignment is completed what the units on this deliverable should at least be
                                decimal supposedUnitsForPreviousAssignment = deliverable.Total_Units * (assignment.LOW_VALUE - 0.0001m);
                                decimal supposedUnitsForCurrentAssignment = deliverable.Total_Units * ((assignment.HIGH_VALUE - assignment.LOW_VALUE) + 0.0001m);

                                //because current task can be assigned to multiple deliverable, actual earned units needs to be pro-rated
                                decimal proRateValue = supposedUnitsForCurrentAssignment / (decimal)repositoryTASK.target_work_qty;
                                decimal proRateUnitsForCurrentAssignment = (decimal)repositoryTASK.act_work_qty * proRateValue;

                                decimal totalSupposedUnits = supposedUnitsForPreviousAssignment + proRateUnitsForCurrentAssignment;

                                decimal totalUnitsEarned = deliverable.Progresses.Sum(x => x.EARNED_UNITS);
                                decimal unitsParity = totalSupposedUnits - totalUnitsEarned;

                                //Add units for previous task assignments
                                IEnumerable<P6_ASSIGNMENT> completedAssignments = deliverable.P6_Assignments.Where(x => x.HIGH_VALUE < assignment.LOW_VALUE);

                                //Each period got to give up some units to compensate for those already earned
                                decimal individualPeriodFactor = 1;
                                if (totalUnitsEarned != 0)
                                    individualPeriodFactor = (totalSupposedUnits - totalUnitsEarned) / totalSupposedUnits;

                                if (unitsParity > 0.0001m)
                                {
                                    addOrIncreaseDataPointsForTasks(completedAssignments, deliverable, firstAlignedDataDate, progressLowerLimitDate, individualPeriodFactor);
                                    allCompletedAssignments.AddRange(completedAssignments);
                                    markP6TaskAsCompleted(allCompletedAssignments, processedTasks);
                                    //Add units for current task assignment
                                    List<P6_ASSIGNMENT> currentAssignment = new List<P6_ASSIGNMENT>();
                                    currentAssignment.Add(assignment);
                                    addOrIncreaseDataPointsForTasks(currentAssignment, deliverable, firstAlignedDataDate, progressLowerLimitDate, individualPeriodFactor, proRateUnitsForCurrentAssignment);
                                }
                                else if (unitsParity < -0.0001m)
                                {
                                    removeOrReduceDataPointsForTasks(deliverable, unitsParity);
                                    bluePrintsUOW.SaveChanges();
                                }
                                else
                                {
                                    allCompletedAssignments.AddRange(completedAssignments);
                                    markP6TaskAsCompleted(allCompletedAssignments, processedTasks);
                                }

                                break;
                            }
                        }
                    }
                }

                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            destroy_scheduling_view_model();
            MessageBoxService.ShowMessage("P6 activities fixed and progress synced");
        }

        private void markP6TaskAsCompleted(IEnumerable<P6_ASSIGNMENT> allCompletedAssignments, List<string> processedTasks)
        {
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            //Fix p6 for task that should be completed
            foreach (P6_ASSIGNMENT assignment in allCompletedAssignments)
            {
                if (!processedTasks.Any(x => x == assignment.P6_ACTIVITYID))
                {
                    TASK p6Task = PROJECTTASK.FirstOrDefault(x => x.task_code == assignment.P6_ACTIVITYID);
                    if (p6Task != null && p6Task.status_code != P6TASKSTATUS.TK_Complete.ToString())
                    {
                        TASK repositoryTASK = p6UOW.TASK.FirstOrDefault(x => x.task_id == p6Task.task_id);
                        //DataUtils.ShallowCopy(repositoryTASK, p6Task);
                        if (repositoryTASK != null)
                        {
                            repositoryTASK.status_code = P6TASKSTATUS.TK_Complete.ToString();
                            repositoryTASK.act_work_qty = repositoryTASK.target_work_qty;

                            if (repositoryTASK.act_start_date == null)
                                repositoryTASK.act_start_date = repositoryTASK.target_start_date;

                            if (repositoryTASK.act_end_date == null)
                                repositoryTASK.act_end_date = repositoryTASK.target_end_date;

                            repositoryTASK.target_drtn_hr_cnt += repositoryTASK.remain_drtn_hr_cnt;
                            repositoryTASK.remain_drtn_hr_cnt = 0;
                        }
                    }

                    processedTasks.Add(assignment.P6_ACTIVITYID);
                }
            }

            p6UOW.SaveChanges();
        }

        private void removeOrReduceDataPointsForTasks(ICanAssignP6 deliverable, decimal reduceUnits)
        {
            reduceUnits *= -1;
            foreach (PROGRESS_ITEM progress in deliverable.Progresses.OrderByDescending(x => x.EARNED_DATE))
            {
                PROGRESS_ITEM currentDateProgress = bluePrintsUOW.PROGRESS_ITEMS.FirstOrDefault(x => x.GUID == progress.GUID);
                if (reduceUnits > 0)
                {
                    if (currentDateProgress.EARNED_UNITS < reduceUnits)
                    {
                        reduceUnits -= currentDateProgress.EARNED_UNITS;
                        currentDateProgress.EARNED_UNITS = 0;
                    }
                    else if (currentDateProgress.EARNED_UNITS >= reduceUnits)
                    {
                        currentDateProgress.EARNED_UNITS -= reduceUnits;
                        reduceUnits = 0;
                    }
                }
            }
        }

        private void addOrIncreaseDataPointsForTasks(IEnumerable<P6_ASSIGNMENT> completedAssignments, ICanAssignP6 deliverable, DateTime firstAlignedDataDate, DateTime progressLowerLimitDate, decimal individualPeriodFactor, decimal? manualParity = null)
        {
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            foreach (P6_ASSIGNMENT completedAssignment in completedAssignments.OrderBy(x => x.HIGH_VALUE))
            {
                TASK task = PROJECTTASK.FirstOrDefault(x => x.task_code == completedAssignment.P6_ACTIVITYID);
                DateTime taskStartDate;
                DateTime taskEndDate;
                TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
                DateTime progressUpperLimitDate = loadPROGRESS.DATA_DATE.AddDays(-1 * interval.Days);

                if (task != null)
                {
                    if (task.act_start_date != null)
                        taskStartDate = (DateTime)task.act_start_date;
                    else if (task.target_start_date != null)
                        taskStartDate = (DateTime)task.target_start_date;
                    else
                        taskStartDate = progressLowerLimitDate;

                    if (task.act_end_date != null)
                        taskEndDate = (DateTime)task.act_end_date;
                    else if (task.target_end_date != null)
                        taskEndDate = (DateTime)task.target_end_date;
                    else
                        taskEndDate = loadPROGRESS.DATA_DATE;

                    if (taskStartDate < progressLowerLimitDate)
                        taskStartDate = progressLowerLimitDate;
                    else if (taskStartDate > progressUpperLimitDate)
                        taskStartDate = progressUpperLimitDate;

                    if (taskEndDate > progressUpperLimitDate)
                        taskEndDate = progressUpperLimitDate;
                    else if (taskEndDate < progressLowerLimitDate)
                        taskEndDate = progressLowerLimitDate;

                    decimal totalUnitsToAddToDeliverable;
                    if (manualParity == null)
                    {
                        //assuming previous assignment is completed what the units on this deliverable should at least be
                        decimal supposedUnitsForAssignments = deliverable.Total_Units * completedAssignment.HIGH_VALUE;
                        IQueryable<PROGRESS_ITEM> repositoryProgress = bluePrintsUOW.PROGRESS_ITEMS.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.GUID_ORIBASEITEM == deliverable.OriginalEntityKey);
                        decimal earnedUnits = 0;
                        if (repositoryProgress.Count() == 0)
                            earnedUnits = 0;
                        else
                            repositoryProgress.Sum(x => x.EARNED_UNITS);

                        if (earnedUnits >= supposedUnitsForAssignments)
                            continue;

                        totalUnitsToAddToDeliverable = deliverable.Total_Units * ((completedAssignment.HIGH_VALUE - completedAssignment.LOW_VALUE) + 0.0001m);
                    }
                    else
                        totalUnitsToAddToDeliverable = (decimal)manualParity;

                    IEnumerable<DateTime> interpolatedDates = getInterpolationDataDate(taskStartDate, taskEndDate, firstAlignedDataDate);
                    if (interpolatedDates.Count() > 0)
                    {
                        decimal totalUnitsToAddPerPeriod = (totalUnitsToAddToDeliverable * individualPeriodFactor) / interpolatedDates.Count();
                        foreach (DateTime interpolatedDate in interpolatedDates)
                        {
                            DateTime interpolationDateFormat = interpolatedDate.Date.AddDays(1).AddSeconds(-1);
                            PROGRESS_ITEM currentDateProgress = bluePrintsUOW.PROGRESS_ITEMS.FirstOrDefault(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.GUID_ORIBASEITEM == deliverable.OriginalEntityKey && x.EARNED_DATE == interpolationDateFormat);
                            if (currentDateProgress != null)
                                currentDateProgress.EARNED_UNITS += totalUnitsToAddPerPeriod;
                            else
                            {
                                PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                                newPROGRESS_ITEM.EARNED_UNITS = totalUnitsToAddPerPeriod;
                                newPROGRESS_ITEM.EARNED_DATE = interpolationDateFormat;
                                newPROGRESS_ITEM.GUID_ORIBASEITEM = deliverable.OriginalEntityKey;
                                newPROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
                                bluePrintsUOW.PROGRESS_ITEMS.Add(newPROGRESS_ITEM);
                            }

                            bluePrintsUOW.SaveChanges();
                        }
                    }
                }
            }
        }

        public void ReverseProgressP6_Loaded(IEnumerable<ICanAssignP6> entities)
        {
            //need to change all of this to foreach task and lookup task table to see total pushed hours and delta current hours
            //then distribute those delta hours + or - into each period datapoints

            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;


            List<P6ErrorMessage> errorMessages = new List<P6ErrorMessage>();
            List<P6Simulation> simulations = push_units_to_p6(entities, true, errorMessages);

            List<string> processedTasks = new List<string>();
            LoadingScreenManager.ShowLoadingScreen(PROJECTTASK.Count() * 2);
            LoadingScreenManager.SetMessage("Phase 2 of 2: Syncing progress from P6");

            taskParityAdjustment(simulations, true);
            //simulations = push_units_to_p6(entities, true, errorMessage);
            taskParityAdjustment(simulations, false);

            LoadingScreenManager.CloseLoadingScreen();

            destroy_scheduling_view_model();

            mainThreadDispatcher.BeginInvoke(new Action(() => showP6ErrorMessage("Progress is synced from P6 with the following error", "Progress is synced from P6", errorMessages)));
        }

        private void taskParityAdjustment(IEnumerable<P6Simulation> simulations, bool isPositiveAdjustment)
        {
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);

            //specify the oldest date data points can edit to
            DateTime progressLowerLimitDate = loadPROGRESS.PREVIOUS_REPORT_DATE == null ? loadPROGRESS.REPORT_DATE == null ? loadPROGRESS.PROGRESS_START : (DateTime)loadPROGRESS.REPORT_DATE : (DateTime)loadPROGRESS.PREVIOUS_REPORT_DATE;
            TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            progressLowerLimitDate.AddDays(interval.Days);

            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            foreach (TASK task in PROJECTTASK)
            {
                if (task.target_work_qty == 0)
                    continue;

                IEnumerable<P6Simulation> allTaskSimulation = simulations.Where(x => x.Assignment.P6_ACTIVITYID == task.task_code);
                decimal totalPushUnits = allTaskSimulation.Sum(x => x.PushUnits);
                decimal totalP6Units = task.act_work_qty == null ? 0 : (decimal)task.act_work_qty;

                decimal totalParity = totalP6Units - totalPushUnits;
                decimal remainingParity = totalParity;
                totalParity = Math.Round(totalParity, 2);
                if ((!isPositiveAdjustment && totalParity < 0) || (isPositiveAdjustment && totalParity > 0))
                {
                    do
                    {
                        remainingParity = doParityAdjustments(allTaskSimulation, remainingParity, firstAlignedDataDate, progressLowerLimitDate);
                    } while (Math.Round(remainingParity, 2) != 0);
                }

                LoadingScreenManager.Progress();
            }
        }

        private decimal doParityAdjustments(IEnumerable<P6Simulation> simulations, decimal totalParity, DateTime firstAlignedDataDate, DateTime progressLowerLimitDate)
        {
            decimal remainingParity = totalParity;
            IEnumerable<P6Simulation> spareSimulations = simulations.Where(x => (x.MaxUnits - x.PostPushUnits) > 0);
            if (totalParity < 0)
                spareSimulations = simulations.Where(x => x.PostPushUnits > 0);
            else
                spareSimulations = simulations.Where(x => (x.MaxUnits - x.PostPushUnits) > 0);

            foreach (P6Simulation simulation in spareSimulations)
            {
                //pro-rate by pushed units for any addition or reduction
                decimal proRate = simulation.MaxUnits / spareSimulations.Sum(x => x.MaxUnits);
                decimal proratedParity = (totalParity * proRate);
                decimal postAdjustmentUnits = simulation.PostPushUnits + proratedParity;
                if (proratedParity < 0)
                {
                    if (postAdjustmentUnits < 0)
                    {
                        proratedParity = -1 * simulation.PostPushUnits;
                        postAdjustmentUnits = 0;
                    }
                }
                else
                {
                    if (postAdjustmentUnits > simulation.MaxUnits)
                    {
                        proratedParity = (simulation.MaxUnits - simulation.PostPushUnits);
                        postAdjustmentUnits = simulation.MaxUnits;
                    }
                }

                if ((postAdjustmentUnits >= 0 && postAdjustmentUnits <= simulation.MaxUnits))
                {
                    remainingParity -= proratedParity;
                    parityAdjustment(simulation, firstAlignedDataDate, progressLowerLimitDate, proratedParity);
                    simulation.PostPushUnits = postAdjustmentUnits;
                }
            }

            return remainingParity;
        }

        private void parityAdjustment(P6Simulation simulation, DateTime firstAlignedDataDate, DateTime progressLowerLimitDate, decimal proRateParity)
        {
            if (simulation.TaskStartDate == null || simulation.TaskEndDate == null)
                return;

            DateTime taskStartDate = (DateTime)simulation.TaskStartDate;
            DateTime taskEndDate = (DateTime)simulation.TaskEndDate;
            TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime progressUpperLimitDate = loadPROGRESS.DATA_DATE.AddDays(-1 * interval.Days);

            if (simulation.TaskStartDate < progressLowerLimitDate)
                taskStartDate = progressLowerLimitDate;
            else if (simulation.TaskStartDate > progressUpperLimitDate)
                taskStartDate = progressUpperLimitDate;

            if (simulation.TaskEndDate > progressUpperLimitDate)
                taskEndDate = progressUpperLimitDate;
            else if (simulation.TaskEndDate < progressLowerLimitDate)
                taskEndDate = progressLowerLimitDate;

            if (proRateParity < 0)
            {
                IEnumerable<PROGRESS_ITEM> currentDeliverableProgresses = bluePrintsUOW.PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID && x.GUID_ORIBASEITEM == simulation.DeliverableOriginalEntityKey);
                removeOrReduceDataPointsForTasks(currentDeliverableProgresses, proRateParity);
                bluePrintsUOW.SaveChanges();
            }
            else
            {
                IEnumerable<DateTime> interpolatedDates = getInterpolationDataDate(taskStartDate, taskEndDate, firstAlignedDataDate);
                if (interpolatedDates.Count() > 0)
                {
                    decimal proratedParityPerPeriod = proRateParity / interpolatedDates.Count();
                    foreach (DateTime interpolatedDate in interpolatedDates)
                    {
                        DateTime interpolationDateFormat = interpolatedDate.Date.AddDays(1).AddSeconds(-1);
                        PROGRESS_ITEM currentDateProgress = bluePrintsUOW.PROGRESS_ITEMS.FirstOrDefault(x => x.GUID_PROGRESS == loadPROGRESS.GUID && x.GUID_ORIBASEITEM == simulation.DeliverableOriginalEntityKey && x.EARNED_DATE == interpolationDateFormat);
                        if (currentDateProgress != null)
                        {
                            currentDateProgress.EARNED_UNITS += proratedParityPerPeriod;
                            //simulation.PostPushUnits += proratedParityPerPeriod;
                        }
                        else
                        {
                            PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                            newPROGRESS_ITEM.EARNED_UNITS = proratedParityPerPeriod;
                            newPROGRESS_ITEM.EARNED_DATE = interpolationDateFormat;
                            newPROGRESS_ITEM.GUID_ORIBASEITEM = simulation.DeliverableOriginalEntityKey;
                            newPROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
                            bluePrintsUOW.PROGRESS_ITEMS.Add(newPROGRESS_ITEM);

                            //simulation.PostPushUnits += proratedParityPerPeriod;
                        }

                        bluePrintsUOW.SaveChanges();
                    }
                }
            }
        }

        private void removeOrReduceDataPointsForTasks(IEnumerable<PROGRESS_ITEM> progresses, decimal reduceUnits)
        {
            reduceUnits *= -1;
            foreach (PROGRESS_ITEM progress in progresses.OrderByDescending(x => x.EARNED_DATE))
            {
                if (reduceUnits > 0)
                {
                    if (progress.EARNED_UNITS < reduceUnits)
                    {
                        reduceUnits -= progress.EARNED_UNITS;
                        progress.EARNED_UNITS = 0;
                    }
                    else if (progress.EARNED_UNITS >= reduceUnits)
                    {
                        progress.EARNED_UNITS -= reduceUnits;
                        reduceUnits = 0;
                    }
                }
            }
        }

        public List<DateTime> getInterpolationDataDate(DateTime startDate, DateTime endDate, DateTime firstAlignedDataDate)
        {
            List<DateTime> interpolationDate = new List<DateTime>();
            TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime startLimitInterpolationDate = ChronologicalHelpers.GenerateAlignedDataDate(firstAlignedDataDate, startDate, interval);
            double periodToLoop = 0;
            TimeSpan taskInterval;
            taskInterval = endDate - startDate;

            periodToLoop = Convert.ToDouble(taskInterval.Days) / Convert.ToDouble(interval.Days);
            periodToLoop = Math.Ceiling(periodToLoop);

            //DateTime endDateToLoop = task.act_end_date == null ? loadPROGRESS.DATA_DATE : task.status_code == P6TASKSTATUS.TK_Complete.ToString() ? (DateTime)task.act_end_date : loadPROGRESS.DATA_DATE;
            //if (loadPROGRESS.PREVIOUS_REPORT_DATE != null && (DateTime)loadPROGRESS.PREVIOUS_REPORT_DATE > startLimitInterpolationDate)
            //    startLimitInterpolationDate = (DateTime)loadPROGRESS.PREVIOUS_REPORT_DATE;

            DateTime loopStartDate = startLimitInterpolationDate;
            double loopCounter = 1;
            do
            {
                interpolationDate.Add(loopStartDate);
                loopStartDate = loopStartDate.AddDays(interval.Days);
                loopCounter += 1;
            } while (loopCounter <= periodToLoop);

            return interpolationDate.OrderBy(x => x.Date).ToList();
        }

        bool isPushingToP6;
        protected abstract IEntitiesSchedulingCollectionWrapper scheduling_view_model { get; }
        private void PushToP6(BaselineMappingSelectionType mappingSelectionType)
        {
            if (loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return;

            isPushingToP6 = true;
            //Stats will be built in SummarizeSinglePROJECTDashboard within SummarizeBASELINE_ITEMDashboard in ConstructMainViewModelProjection
            MainViewModel.Refresh();
            scheduling_view_model.OnViewModelLoaded = onSchedulingViewModelLoaded;
            scheduling_view_model.OnViewModelLoadFailed = onSchedulingViewModelLoadFailed;
            scheduling_view_model.SetParentViewModel(this);
            var ParameterObj = scheduling_view_model as ISupportParameter;
            ParameterObj.Parameter = new object[] { loadPROGRESS, mappingSelectionType, loadPROJECT, false };
        }

        private void updateP6DatesBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (updateP6DatesBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            var argumentObject = (object[])e.Argument;
            BASELINE baseline = (BASELINE)argumentObject[0];
            PROGRESS progress = (PROGRESS)argumentObject[1];
            IP6EntitiesUnitOfWork p6UnitOfWork = (IP6EntitiesUnitOfWork)argumentObject[2];
            UpdateTrueP6Dates(baseline, progress, p6UnitOfWork);
        }

        private void updateP6DatesBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (updateP6DatesBackgroundWorker.CancellationPending)
                return;

            if (typeof(TMainProjectionEntity).GetInterfaces().Contains(typeof(IHaveTrueP6Dates)))
            {
                foreach (IHaveTrueP6Dates entity in Entities)
                {
                    entity.Update();
                }
            }
        }

        private void UpdateTrueP6Dates(BASELINE liveBASELINE, PROGRESS livePROGRESS, IP6EntitiesUnitOfWork p6UnitOfWork)
        {
            if (typeof(TMainProjectionEntity).GetInterfaces().Contains(typeof(IHaveTrueP6Dates)))
            {
                if(liveBASELINE != null)
                {
                    P6Data.PROJECT p6BaselinePROJECT = p6UnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == liveBASELINE.P6BASELINE_NAME);
                    if(p6BaselinePROJECT != null)
                    {
                        List<TASK> P6PlannedTASKS = p6BaselinePROJECT.TASK.ToList();
                        foreach (IHaveTrueP6Dates entity in Entities)
                        {
                            entity.PopulateTrueP6Dates(P6PlannedTASKS, true);
                        }
                    }
                }

                P6Data.PROJECT p6ProgressPROJECT = p6UnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == loadPROGRESS.P6PROGRESS_NAME);
                if(p6ProgressPROJECT != null)
                {
                    List<TASK> P6RemainingTASKS = p6ProgressPROJECT.TASK.ToList();
                    foreach (IHaveTrueP6Dates entity in Entities)
                    {
                        entity.PopulateTrueP6Dates(P6RemainingTASKS, false);
                    }
                }
            }
        }

        private List<P6Simulation> push_units_to_p6(IEnumerable<ICanAssignP6> deliverables, bool isSimulation, List<P6ErrorMessage> errorMessages)
        {
            List<TASK> processedP6Task = new List<TASK>();
            TimeSpan intervalTimeSpan = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            LoadingScreenManager.ShowLoadingScreen(deliverables.Count(), false);
            if (isSimulation)
                LoadingScreenManager.SetMessage("Simulating progress to P6");
            else
                LoadingScreenManager.SetMessage("Pushing to P6");

            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            List<P6Simulation> simulationResults = new List<P6Simulation>();
            decimal periodAssignedUnitsForTaskFound = 0;
            decimal periodAssignedUnitsOnMapping = 0;
            decimal periodAssignedUnits = 0;
            foreach (ICanAssignP6 deliverable in deliverables)
            {
                LoadingScreenManager.Progress();
                if (deliverable.Total_Units == 0 && deliverable.Earned_Units_ToDate == 0)
                    continue;

                IReportable current_progress_deliverable = deliverable as IReportable;
                if (current_progress_deliverable == null)
                    continue;

                bool isNullProgress = false;
                //comment this off because duration needs to be calculated even if deliverable is not progressed
                if (current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate == null || current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate.Where(x => x.EARNED_UNITS > 0).Count() == 0)
                    isNullProgress = true;

                DateTime? first_progress_date = isNullProgress ? (DateTime?)null : current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate.Where(x => x.EARNED_UNITS > 0).Min(x => x.EARNED_DATE);
                DateTime? last_progress_date = isNullProgress ? (DateTime?)null : current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate.Where(x => x.EARNED_UNITS > 0).Max(x => x.EARNED_DATE);

                decimal total_percentage_to_date;

                total_percentage_to_date = current_progress_deliverable.Total_Percentage_ToDate;
                periodAssignedUnits += current_progress_deliverable.Earned_Units_ToDate;
                if (deliverable.P6_Assignments.Count == 0)
                {
                    errorMessages.Add(new P6ErrorMessage("Mapping not found", "", current_progress_deliverable.Deliverable_Name, current_progress_deliverable.Earned_Units_ToDate, null));
                    continue;
                }

                //only process applicable assignments
                //List<P6_ASSIGNMENT> p6_assignments = deliverable.P6_Assignments.Where(assignment => assignment.LOW_VALUE <= (total_percentage_to_date + 0.0001m)).OrderBy(assignment => assignment.LOW_VALUE).ToList();
                List<P6_ASSIGNMENT> all_assignments = deliverable.P6_Assignments.OrderBy(assignment => assignment.LOW_VALUE).ToList();
                for (int i = 0; i < all_assignments.Count; i++)
                {
                    P6_ASSIGNMENT p6_assignment = all_assignments[i];

                    //sometimes deliverable is cancelled after units has been earned
                    bool isDeliverableCancelled = deliverable.Total_Units == 0 && deliverable.Earned_Units_ToDate > 0;

                    //current activity assignment value must be limited to total earned percentage
                    decimal high_percentage_to_use;
                    if (isDeliverableCancelled)
                        high_percentage_to_use = 1.00m;
                    else
                        high_percentage_to_use = p6_assignment.HIGH_VALUE > total_percentage_to_date ? total_percentage_to_date : p6_assignment.HIGH_VALUE;

                    //current percentage pro-rate
                    decimal current_percentage = p6_assignment.LOW_VALUE <= (total_percentage_to_date + 0.0001m) ? ((high_percentage_to_use - p6_assignment.LOW_VALUE) + 0.0001m) : 0;

                    //full assignment percentage used to calculate remaining units
                    decimal full_assignment_percentage = ((p6_assignment.HIGH_VALUE - p6_assignment.LOW_VALUE) + 0.0001m);

                    //current activity assignment unit
                    decimal current_assignment_units;
                    if (isDeliverableCancelled)
                        current_assignment_units = current_percentage * deliverable.Earned_Units_ToDate;
                    else
                        current_assignment_units = current_percentage * deliverable.Total_Units;

                    //current activity full assignment units to calculate remaining units
                    decimal full_assignment_units = full_assignment_percentage * deliverable.Total_Units;

                    if (full_assignment_units <= 0)
                        continue;

                    periodAssignedUnitsOnMapping += current_assignment_units;
                    if (isDeliverableCancelled && MessageBoxService.ShowMessage("Deliverable " + deliverable.ToString() + " has earned " + current_assignment_units.ToString("n2") + " units but there are no budget units in P6 task " + p6_assignment.P6_ACTIVITYID + "\nThis can happen when variation is not client approved\nDo you still want to earn the units on P6 task " + p6_assignment.P6_ACTIVITYID + "?", "Warning", MessageButton.OKCancel) == MessageResult.Cancel)
                        continue;

                    TASK P6TASK = PROJECTTASK.FirstOrDefault(P6Task => P6Task.task_code == p6_assignment.P6_ACTIVITYID);
                    if (P6TASK != null && P6TASK.delete_date == null)
                    {
                        //defines how much percentage of units this assignment will take up when it is fully assigned, so that we can estimate the total duration to apply productivity to
                        decimal current_task_to_activity_percentage = (P6TASK.target_work_qty == null || P6TASK.target_work_qty == 0) ? 0 : full_assignment_units / (decimal)P6TASK.target_work_qty;

                        P6Simulation simulation = new P6Simulation(p6_assignment);
                        simulation.PushUnits = current_assignment_units;
                        simulation.PostPushUnits = current_assignment_units;
                        simulation.MaxUnits = full_assignment_units;
                        simulation.CurrentTaskAssignmentPct = current_task_to_activity_percentage;
                        simulation.TaskStartDate = P6TASK.act_start_date == null ? P6TASK.target_start_date : P6TASK.act_start_date;
                        simulation.TaskEndDate = P6TASK.act_end_date == null ? P6TASK.target_end_date : P6TASK.act_end_date;
                        simulation.DeliverableOriginalEntityKey = deliverable.OriginalEntityKey;
                        simulationResults.Add(simulation);

                        if (isSimulation)
                            continue;

                        errorMessages.Add(new P6ErrorMessage("Pushed", P6TASK.task_code, current_progress_deliverable.Deliverable_Name, current_assignment_units, last_progress_date));

                        //set activity start date
                        DateTime? first_earned_week_start_date = isNullProgress ? (DateTime?)null : ((DateTime)first_progress_date).AddDays(-1 * intervalTimeSpan.Days).AddSeconds(1);
                        bool any_write_exclusions = P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.NONE.ToString()) || P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.FINISH.ToString());

                        if ((P6TASK.act_start_date == null || !any_write_exclusions))
                            if (!isNullProgress)
                            {
                                if (P6TASK.act_start_date == null || P6TASK.act_start_date > ((DateTime)first_earned_week_start_date).Date)
                                    P6TASK.act_start_date = ((DateTime)first_earned_week_start_date).Date;
                            }

                        //if this is the first time processing the task
                        //another way of doing this is to reset everything to zero and not started, but we do not want to override user changes on the p6 schedule
                        if (!processedP6Task.Any(x => x.task_code == P6TASK.task_code))
                            P6TASK.act_work_qty = current_assignment_units;
                        else
                            P6TASK.act_work_qty += current_assignment_units;

                        periodAssignedUnitsForTaskFound += current_assignment_units;
                        if (P6TASK.act_work_qty == 0)
                        {
                            P6TASK.act_start_date = null;
                            P6TASK.act_end_date = null;
                            P6TASK.status_code = P6TASKSTATUS.TK_NotStart.ToString();
                            break;
                        }

                        if (P6TASK.target_work_qty <= 0)
                        {
                            if (!isDeliverableCancelled)
                            {
                                errorMessages.Add(new P6ErrorMessage("No budgeted units", P6TASK.task_code, "", 0, null));
                                break;
                            }
                        }

                        if (P6TASK.remain_work_qty >= 0)
                            P6TASK.remain_work_qty = P6TASK.target_work_qty - P6TASK.act_work_qty;

                        TASKRSRC P6TASKRSRC = p6UOW.TASKRSRC.FirstOrDefault(x => x.task_id == P6TASK.task_id);
                        if (P6TASKRSRC != null)
                        {
                            P6TASKRSRC.act_reg_qty = P6TASK.act_work_qty;
                            P6TASKRSRC.remain_qty = P6TASK.remain_work_qty;
                        }

                        if (P6TASK.remain_work_qty < 0)
                        {
                            #region Commercially approved variation temporary fix
                            //this happens when variation is not pushed to P6, so we have to re-Adjust P6 units act_work_qty back to target_work_qty and set remain_work_qty as 0
                            P6TASK.act_work_qty = P6TASK.target_work_qty;
                            P6TASK.remain_work_qty = 0;
                            #endregion

                            //if (!isDeliverableCancelled)
                            //{
                            //    errorMessage = "Negative remaining units on " + P6TASK.task_code + " because budgeted units is less than earned units, please re-populate budgeted units on baseline";
                            //    break;
                            //}
                        }


                        if (P6TASK.remain_work_qty == 0)
                        {
                            P6TASK.status_code = P6TASKSTATUS.TK_Complete.ToString();
                            P6TASK.remain_drtn_hr_cnt = 0;
                            //when user select none or user select start only, don't update finish
                            any_write_exclusions = P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.NONE.ToString()) || P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.START.ToString());
                            if (!any_write_exclusions)
                                if (!isNullProgress)
                                {
                                    //find all deliverable with relevant p6 assignment by id and percentage
                                    IEnumerable<ICanAssignP6> currentActivityDeliverables = deliverables.Where(x => x.P6_Assignments.Any(y => y.P6_ACTIVITYID == p6_assignment.P6_ACTIVITYID && y.LOW_VALUE >= p6_assignment.LOW_VALUE && y.HIGH_VALUE <= p6_assignment.HIGH_VALUE));
                                    IEnumerable<IReportable> currentActivityReportables = currentActivityDeliverables.Select(x => (IReportable)x);
                                    IEnumerable<IReportable> currentActivityValidReportables = currentActivityReportables.Where(x => x.Stats != null && x.Stats.Earned != null && x.Stats.Earned.CumulativeDataPoints != null);

                                    //find a date where all deliverables has achieved p6 assignment high value percentage
                                    DateTime? latestFirstHighestPercentageOccuranceDate = null;
                                    foreach (var currentActivityValidReportable in currentActivityValidReportables)
                                    {
                                        if (currentActivityValidReportable.Stats.Earned.CumulativeDataPoints.Any(x => x.UnitsPercentage >= p6_assignment.HIGH_VALUE))
                                        {
                                            DateTime firstPercentageOccuranceDate = currentActivityValidReportable.Stats.Earned.CumulativeDataPoints.OrderBy(x => x.ProgressDate).First(x => x.UnitsPercentage >= p6_assignment.HIGH_VALUE).ProgressDate;
                                            if (latestFirstHighestPercentageOccuranceDate == null || latestFirstHighestPercentageOccuranceDate < firstPercentageOccuranceDate)
                                                latestFirstHighestPercentageOccuranceDate = firstPercentageOccuranceDate;
                                        }
                                    }

                                    if (latestFirstHighestPercentageOccuranceDate != null)
                                    {
                                        P6TASK.act_end_date = latestFirstHighestPercentageOccuranceDate;
                                        P6TASK.late_start_date = null;
                                        P6TASK.late_end_date = null;
                                        P6TASK.early_start_date = null;
                                        P6TASK.early_end_date = null;
                                    }
                                }
                        }
                        else if (P6TASK.remain_work_qty > 0)
                        {
                            P6TASK.status_code = P6TASKSTATUS.TK_Active.ToString();
                            //when user select none or user select finish only, don't update start
                            any_write_exclusions = P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.NONE.ToString()) || P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.FINISH.ToString());
                            if (P6TASK.act_start_date == null && !any_write_exclusions)
                                if (!isNullProgress)
                                    P6TASK.act_start_date = ((DateTime)first_progress_date).Date;

                            P6TASK.act_end_date = null;


                            decimal current_full_remaining_duration = (decimal)P6TASK.target_drtn_hr_cnt * current_task_to_activity_percentage;

                            decimal current_assignment_remaining_units = full_assignment_units - current_assignment_units;
                            decimal current_assignment_remaining_duration = current_full_remaining_duration * (current_assignment_remaining_units / full_assignment_units);
                            IHaveDBProductivityOverride productivityOverride = deliverable as IHaveDBProductivityOverride;

                            //productivity override determines whether we should tamper with the remaining duration
                            if (productivityOverride != null)
                            {
                                //need to cast to IReportable to get Override_Productivity properties that determine whether to use db productivity or current productivity
                                IReportable reportable = deliverable as IReportable;
                                if (P6TASK.target_drtn_hr_cnt != null && P6TASK.target_work_qty > 0 && P6TASK.remain_work_qty > 0)
                                {
                                    //in the first progress current productivity will be null and if user doesn't override the productivity, we will have 0 productivity
                                    //decimal override_productivity;
                                    //if (reportable.Current_Productivity == 0 && reportable.Override_Productivity == 0)
                                    //    override_productivity = 1;
                                    //else if (reportable.Override_Productivity != null)
                                    //    override_productivity = (decimal)reportable.Override_Productivity;
                                    //else
                                    //    override_productivity = reportable.Current_Productivity;

                                    //currently user isn't familiar with productivity calculation so set productivity to 1
                                    decimal override_productivity = 1;
                                    decimal current_assignment_remaining_duration_per_productivity = current_assignment_remaining_duration / override_productivity;
                                    if (!processedP6Task.Any(x => x.task_code == P6TASK.task_code))
                                        P6TASK.remain_drtn_hr_cnt = current_assignment_remaining_duration_per_productivity;
                                    else
                                        P6TASK.remain_drtn_hr_cnt += current_assignment_remaining_duration_per_productivity;
                                }
                            }
                            else
                            {
                                if (!processedP6Task.Any(x => x.task_code == P6TASK.task_code))
                                    P6TASK.remain_drtn_hr_cnt = current_assignment_remaining_duration;
                                else
                                    P6TASK.remain_drtn_hr_cnt += current_assignment_remaining_duration;
                            }
                        }
                        else if (P6TASK.status_code == P6TASKSTATUS.TK_NotStart.ToString())
                            P6TASK.status_code = P6TASKSTATUS.TK_Active.ToString();

                        if (!processedP6Task.Any(x => x.task_code == P6TASK.task_code))
                            processedP6Task.Add(P6TASK);

                        TASK repositoryTASK = p6UOW.TASK.FirstOrDefault(x => x.task_id == P6TASK.task_id);
                        DataUtils.ShallowCopy(repositoryTASK, P6TASK);
                        //scheduling_view_model.Save_Task(P6TASK);
                    }
                    else
                    {
                        decimal assignedUnits = 0;
                        if (deliverable.Total_Units > 0)
                        {
                            foreach (PROGRESS_ITEM progressItem in current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate)
                            {
                                assignedUnits += progressItem.EARNED_UNITS;
                                decimal currentPercentage = assignedUnits / deliverable.Total_Units;

                                if (currentPercentage > p6_assignment.LOW_VALUE && currentPercentage <= p6_assignment.HIGH_VALUE)
                                    errorMessages.Add(new P6ErrorMessage("Task not found", p6_assignment.P6_ACTIVITYID, current_progress_deliverable.Deliverable_Name, progressItem.EARNED_UNITS, progressItem.EARNED_DATE));
                            }
                        }

                        break;
                    }
                }
            }

            //for troubleshooting
            //string test = periodAssignedUnitsForTaskFound.ToString();
            //string test2 = periodAssignedUnitsOnMapping.ToString();
            //string test3 = periodAssignedUnits.ToString();

            //second pass to make sure at completion work quantity is same for resource and activity
            foreach (TASK P6TASK in processedP6Task)
            {
                if (P6TASK.remain_work_qty >= 0)
                    P6TASK.remain_work_qty = P6TASK.target_work_qty - P6TASK.act_work_qty;

                TASKRSRC P6TASKRSRC = p6UOW.TASKRSRC.FirstOrDefault(x => x.task_id == P6TASK.task_id);
                if (P6TASKRSRC != null)
                {
                    P6TASKRSRC.act_reg_qty = P6TASK.act_work_qty;
                    P6TASKRSRC.remain_qty = P6TASK.remain_work_qty;
                }
            }

            p6UOW.SaveChanges();
            LoadingScreenManager.CloseLoadingScreen();

            return simulationResults;
        }

        protected void onSchedulingViewModelLoaded(IEnumerable<ICanAssignP6> entities)
        {
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            if (PROJECTTASK.Count() == 0)
            {
                onSchedulingViewModelLoadFailed("No activities found, please check if activity code is marked as " + progress_type);
                return;
            }

            List<StatsCalculationType> calcTypes = new List<StatsCalculationType>();
            calcTypes.Add(StatsCalculationType.Earned);
            foreach (var displayEntity in Entities)
            {
                displayEntity.BuildStats(1, calcTypes);
            }

            IEnumerable<ICanAssignP6> deliverables = Entities;
            #region reset budgeted on progress
            IEnumerable<TASK> task_source = scheduling_view_model.TASK_Source;

            //reset all tasks target to 0
            foreach (TASK task in task_source)
            {
                task.act_work_qty = 0;
                task.target_work_qty = 0;
                task.remain_work_qty = 0;
                task.status_code = P6TASKSTATUS.TK_NotStart.ToString();
                task.act_start_date = null;
                task.act_end_date = null;
                task.duration_type = P6DURATION_TYPE.DT_FixedQty.ToString();
                task.complete_pct_type = P6COMPLETE_TYPE.CP_Units.ToString();

                TASKRSRC primaryTASKRSRC = p6UOW.TASKRSRC.FirstOrDefault(x => x.task_id == task.task_id);
                if (primaryTASKRSRC != null)
                {
                    primaryTASKRSRC.act_reg_qty = 0;
                    primaryTASKRSRC.remain_qty = 0;
                    primaryTASKRSRC.target_qty = 0;
                    primaryTASKRSRC.remain_qty_per_hr = 0;
                    primaryTASKRSRC.remain_cost = 0;
                    primaryTASKRSRC.target_cost = 0;
                }
            }

            foreach (ICanAssignP6 deliverable in deliverables)
            {
                IEnumerable<P6_ASSIGNMENT> deliverable_assignments = deliverable.P6_Assignments;
                foreach (P6_ASSIGNMENT deliverable_assignment in deliverable_assignments)
                {
                    TASK actual_context_task = task_source.FirstOrDefault(x => x.task_code == deliverable_assignment.P6_ACTIVITYID);
                    P6_AssignmentProjection p6_assignment = new P6_AssignmentProjection(deliverable, deliverable_assignment, false);

                    if (actual_context_task != null && actual_context_task.delete_date == null)
                    {
                        actual_context_task.target_work_qty += p6_assignment.UNITS;
                        actual_context_task.remain_work_qty += p6_assignment.UNITS;

                        TASK repositoryTASK = p6UOW.TASK.FirstOrDefault(x => x.task_id == actual_context_task.task_id);
                        DataUtils.ShallowCopy(repositoryTASK, actual_context_task);

                        TASKRSRC actual_context_taskrsrc = p6UOW.TASKRSRC.FirstOrDefault(x => x.task_id == actual_context_task.task_id);
                        if (actual_context_taskrsrc != null)
                        {
                            actual_context_taskrsrc.remain_qty += p6_assignment.UNITS;
                            actual_context_taskrsrc.target_qty += p6_assignment.UNITS;

                            if (actual_context_task.target_drtn_hr_cnt != null && actual_context_task.target_drtn_hr_cnt != 0)
                                actual_context_taskrsrc.remain_qty_per_hr = actual_context_taskrsrc.target_qty / actual_context_task.target_drtn_hr_cnt;
                        }
                    }
                }
            }
            #endregion

            List<string> processedP6Task = new List<string>();
            TimeSpan intervalTimeSpan = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            List<P6ErrorMessage> errorMessages = new List<P6ErrorMessage>();

            push_units_to_p6(deliverables, false, errorMessages);
            destroy_scheduling_view_model();

            mainThreadDispatcher.BeginInvoke(new Action(() => showP6ErrorMessage(BluePrintsResources.P6_Assignment_Progress_Write_Success, "Progress in P6 is synced with the following error", errorMessages)));
        }

        private void showP6ErrorMessage(string dialogMessage, string successMessage, List<P6ErrorMessage> errorMessages)
        {
            if (errorMessages.Count > 0)
            {
                DialogCollectionViewModel<P6ErrorMessage> viewModel = DialogCollectionViewModel<P6ErrorMessage>.Create(errorMessages, dialogMessage);
                ErrorMessagesDialogService.ShowDialog(MessageButton.OK, string.Empty, "ListP6ErrorMessages", viewModel);
            }
            else
                MessageBoxService.ShowMessage(successMessage);
        }

        /// <summary>
        /// Progress and return a collection of progressed milestones
        /// </summary>
        private IEnumerable<TASK> getProgressedMilestones(IEnumerable<TASK> task_source, IEnumerable<TASKPRED> taskpred_source)
        {
            IEnumerable<TASK> milestones = task_source.Where(x => x.task_type == P6TASKTYPE.TT_Mile.ToString() || x.task_type == P6TASKTYPE.TT_FinMile.ToString());
            List<TASK> progressed_milestones = new List<TASK>();
            foreach (TASK milestone in milestones)
            {
                List<TASK> task_collector = new List<TASK>();
                recurseGetPredecessorTask(milestone, task_source, taskpred_source, task_collector);

                if (task_collector.All(x => x.status_code == P6TASKSTATUS.TK_Complete.ToString()))
                {
                    milestone.status_code = P6TASKSTATUS.TK_Complete.ToString();
                    milestone.act_start_date = task_collector.Max(x => x.act_end_date);
                    milestone.act_end_date = task_collector.Max(x => x.act_end_date);
                    progressed_milestones.Add(milestone);
                }
            }

            return progressed_milestones;
        }

        private void recurseGetPredecessorTask(TASK parent_task, IEnumerable<TASK> task_source, IEnumerable<TASKPRED> taskpred_source, List<TASK> task_collector)
        {
            IEnumerable<TASKPRED> predecessor_relationships = taskpred_source.Where(x => x.task_id == parent_task.task_id);
            if (predecessor_relationships.Count() > 0)
            {
                IEnumerable<TASK> predecessor_tasks = task_source.Where(x => predecessor_relationships.Any(y => y.pred_task_id == x.task_id));
                foreach (TASK predecessor_task in predecessor_tasks)
                {
                    task_collector.Add(predecessor_task);
                    recurseGetPredecessorTask(predecessor_task, task_source, taskpred_source, task_collector);
                }
            }
        }

        private void onSchedulingViewModelLoadFailed(string error_message)
        {
            MessageBoxService.ShowMessage(error_message, "Error", MessageButton.OK, MessageIcon.Exclamation);
            destroy_scheduling_view_model();
        }

        private void destroy_scheduling_view_model()
        {
            //Dispose viewmodel
            IDocumentContent documentContentViewModel = scheduling_view_model as IDocumentContent;
            documentContentViewModel.OnDestroy();
            dispose_scheduling_view_model();

            isPushingToP6 = false;
            isInteractingWithP6 = false;
            //Need to perform full refresh because MainViewModel repository entity state is messed from scheduling view model, i.e. productivity doesn't update anymore after pushing to P6
            //FullRefresh();
        }

        protected abstract void dispose_scheduling_view_model();
        #endregion

        #region Custom Summary
        bool allowSummaryCalculationOnUpdate;
        public bool AllowSummaryCalculationOnUpdate
        {
            get => allowSummaryCalculationOnUpdate;
            set
            {
                allowSummaryCalculationOnUpdate = value;
                if (AllowSummaryCalculationOnUpdate)
                    GridControlService.RefreshSummary();

                BluePrintsDataUtils.SaveUserPreference(DataUtils.GetNameOf(() => UserPreferences.DesignProgress_AllowSummaryUpdate), value ? UserPreferences.PreferenceTrueValue : UserPreferences.PreferenceFalseValue);
            }
        }

        bool isSummaryRecalculation = false;
        private decimal totalUnits = 0;
        private decimal budgetUnits = 0;

        private string earnedPercentageFieldName => BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage);
        private decimal earnedPercentageTotalUnits = 0;

        private string earnedPeriodPercentageFieldName => BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Earned_Percentage_OnDataDate);
        private decimal earnedPeriodPercentageTotalUnits = 0;

        private string earnedBudgetPercentageFieldName => BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Baseline_Percentage);
        private decimal earnedBudgetPercentageTotalUnits = 0;

        private string schedulePeriodPercentageFieldName => BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().ScheduleCurrentPeriodPercentage);
        private decimal schedulePeriodPercentageTotalUnits = 0;

        private string schedulePercentageFieldName => BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().SchedulePercentage);
        private decimal schedulePercentageTotalUnits = 0;
        public void CustomSummary(CustomSummaryEventArgs e)
        {
            if (isSummaryRecalculation && !AllowSummaryCalculationOnUpdate)
                return;

            string fieldName = ((GridSummaryItem)e.Item).FieldName;
            IReportable reportable = ((IReportable)e.Row);

            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                totalUnits = 0;
                budgetUnits = 0;
                earnedPercentageTotalUnits = 0;
                earnedPeriodPercentageTotalUnits = 0;
                earnedBudgetPercentageTotalUnits = 0;
                schedulePeriodPercentageTotalUnits = 0;
                schedulePercentageTotalUnits = 0;
            }
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                totalUnits += reportable.Total_Units;
                budgetUnits += reportable.Budget_Units;

                if (fieldName == earnedPercentageFieldName)
                {
                    earnedPercentageTotalUnits += reportable.Earned_Units_ToDate;
                    e.TotalValue = totalUnits == 0 ? 0 : earnedPercentageTotalUnits / totalUnits;
                }
                else if (fieldName == earnedPeriodPercentageFieldName)
                {
                    earnedPeriodPercentageTotalUnits += reportable.Earned_Units_OnDataDate;
                    e.TotalValue = totalUnits == 0 ? 0 : earnedPeriodPercentageTotalUnits / totalUnits;
                }
                else if (fieldName == earnedBudgetPercentageFieldName)
                {
                    earnedBudgetPercentageTotalUnits += reportable.Earned_Units_ToDate;
                    e.TotalValue = budgetUnits == 0 ? 0 : earnedBudgetPercentageTotalUnits / budgetUnits;
                }
                else if (fieldName == schedulePeriodPercentageFieldName)
                {
                    if (reportable.Stats != null && reportable.Stats.Budgeted != null && reportable.Stats.Budgeted.CurrentPeriodDataPoint != null)
                        schedulePeriodPercentageTotalUnits += reportable.Stats.Budgeted.CurrentPeriodDataPoint.Units;

                    e.TotalValue = totalUnits == 0 ? 0 : schedulePeriodPercentageTotalUnits / totalUnits;
                }
                else if (fieldName == schedulePercentageFieldName)
                {
                    if (reportable.Stats != null && reportable.Stats.Budgeted != null && reportable.Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                        schedulePercentageTotalUnits += reportable.Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Units;

                    e.TotalValue = totalUnits == 0 ? 0 : schedulePercentageTotalUnits / totalUnits;
                }
            }
        }
        #endregion

        #region Reporting
        protected override string ExportFilename()
        {
            return loadPROJECT.NUMBER + "_Progress_" + loadPROGRESS.DATA_DATE.ToString("dd-MMM-yy");
        }
        #endregion

        #region Disposing
        protected override void OnClose(CancelEventArgs e)
        {
            calculatePlannedBackgroundWorker.CancelAsync();
            progressSaveBackgroundWorker.CancelAsync();
            loadExoBackgroundWorker.CancelAsync();
            //            if (loadPROJECT != null)
            //#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
            //#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            base.OnClose(e);
        }


        #endregion

        #region Entities Wrapper Properties
        public CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROGRESS>();
            }
        }

        public CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESS_ITEMSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROGRESS_ITEM>();
            }
        }

        public CollectionViewModel<PROGRESS_ETC, PROGRESS_ETC, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESS_ETCSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROGRESS_ETC, PROGRESS_ETC, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROGRESS_ETC>();
            }
        }

        public CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECT_REPORTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>();
            }
        }

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<Data.PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<VARIATION> VARIATIONCollection
        {
            get
            {
                var collection = GetEntities<VARIATION>();
                return collection;
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }

        public IEnumerable<PROGRESS_ETC> PROGRESS_ETCCollection
        {
            get
            {
                return GetEntities<PROGRESS_ETC>();
            }
        }

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
                return collection;
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
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

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                return GetSUBAREACollection();
            }
        }

        private IEnumerable<AREA> GetSUBAREACollection()
        {
            var collection = GetEntities<AREA>();
            if (collection != null)
                collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
            return collection;
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }
        #endregion

        #region Book Time
        private DevExpress.Mvvm.IDialogService BookTimeDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BookTimeDialog"); }
        }

        public bool CanBookTime()
        {
            return !IsLoading;
        }

        public void BookTime()
        {
            if (exoAuthorisations == null)
                MessageBoxService.ShowMessage("Exo data is still loading, please wait awhile before using this function");
            else
            {
                ProjectUnitOfWorkContext projectContext = getProjectContexts().FirstOrDefault(x => x.ProjectNumber == SelectedEntity.Project_Number);
                if (projectContext != null)
                {
                    BluePrintsUtils.BookTime(SelectedEntity, projectContext.PrimeroEntitiesUnitOfWork, exoAuthorisations, SelectedEntity.Deliverable_Name, MessageBoxService, BookTimeDialogService, projectContext.Project, USERCollection);
                }
            }
        }
        #endregion
    }
}
