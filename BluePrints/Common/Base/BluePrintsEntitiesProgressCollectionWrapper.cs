using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        where TMainProjectionEntity : class, IGuidEntityKey, IReportable, new()
        where TMainEntityUnitOfWork : IUnitOfWork
    {
        #region Initialization
        protected Data.PROJECT loadPROJECT;
        public P6Data.PROJECT p6PROJECT;
        protected PROGRESS loadPROGRESS;
        protected bool isQueryForLiveStatus;
        protected abstract CostGroup cost_group { get; }
        protected abstract ProgressType progress_type { get; }
        //ensure mainviewmodel is loaded before calling background worker
        protected DispatcherTimer onMainViewModelFirstLoadedTimer;
        //calculates the planned values only for each deliverables
        protected BackgroundWorker calculatePlannedBackgroundWorker;
        //set current data date timer
        protected DispatcherTimer delayedPROGRESSSavingDispatcher;
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected bool is_single_project_mode = true;
        protected bool is_load_p6_task = false;
        public BluePrintsEntitiesProgressCollectionWrapper()
        {
            onMainViewModelFirstLoadedTimer = new DispatcherTimer();
            onMainViewModelFirstLoadedTimer.Interval = new TimeSpan(0, 0, 0, 1);
            onMainViewModelFirstLoadedTimer.Tick += onMainViewModelFirstLoaded;
            calculatePlannedBackgroundWorker = new BackgroundWorker();
            calculatePlannedBackgroundWorker.DoWork += calculatePlannedBackgroundWorker_DoWork;
            calculatePlannedBackgroundWorker.RunWorkerCompleted += CalculatePlannedBackgroundWorker_RunWorkerCompleted;
            calculatePlannedBackgroundWorker.WorkerSupportsCancellation = true;
        }

        protected override void resolveParameters(object parameter)
        {
            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 10);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            var receiveParameter =
                (DualEntitiesParameter<Data.PROJECT, PROGRESS>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadPROGRESS = receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
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

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        protected Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.APPROVED != null && x.TYPE == VariationType.External && x.GUID_PROJECT == loadPROJECT.GUID);
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

        protected Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COST_GROUP == cost_group);
        }

        protected Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x =>
                            x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Progress_Report.ToString());
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainProjectionEntity> entities)
        {
            //MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingRowAddUndoAndSaveCallBack;
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterEntitySavedCallBack;
            MainViewModel.OnMappingAdditionalChangedEntitiesProperties = OnMappingAdditionalChangedEntitiesProperties;
            MainViewModel.OnBeforeAssignRepositoryToExistingProjection = OnBeforeAssignRepositoryToExistingProjection;
            MainViewModel.ValidateSetValueIsContinueCallBack = validateSetValueCallBack;
            PROGRESS_ITEMSCollectionViewModel.SetParentViewModel(this);

            MainViewModel.SetParentViewModel(this);
            //mainThreadDispatcher.BeginInvoke(new Action(() => InitializeSummarizer(entities)));
            onMainViewModelFirstLoadedTimer.Start();
            isFirstLoaded = true;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        //when the inherited view model have group entity, OnBeforeEntitySavedCallBack will be used instead of OnAfterEntitySavedCallBack to identify whether the edited entity is group
        protected abstract bool have_group_entity { get; }
        /// <summary>
        /// Save progress item during BASELINE_ITEM Undo/Redo operation
        /// </summary>
        /// <param name="projectionEntity"></param>
        /// <param name="isNewEntity"></param>
        protected void OnAfterEntitySavedCallBack(TMainProjectionEntity projectionEntity, bool isNewEntity)
        {
            if (!have_group_entity && projectionEntity.ShouldSaveProgress)
            {
                IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = projectionEntity.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                PROGRESS_ITEMSCollectionViewModel.Save(newPRORESS_ITEMS.First());
            }
        }

        protected void OnMappingAdditionalChangedEntitiesProperties(TMainProjectionEntity existingProjectionEntity, TMainProjectionEntity projectionEntity)
        {
            projectionEntity.Stats = existingProjectionEntity.Stats;
        }

        protected void OnBeforeAssignRepositoryToExistingProjection(TMainProjectionEntity existingProjection, TMainProjectionEntity repositoryProjection)
        {
            repositoryProjection.Stats = existingProjection.Stats;
        }

        public bool validateSetValueCallBack(TMainProjectionEntity entity, string column_name, object newValue)
        {
            if (column_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage))
            {
                var newPercentage = (decimal)newValue;
                if (newPercentage > entity.MaxPercentage)
                    return false;
                else if (newPercentage < entity.MinPercentage)
                    return false;
            }

            return true;
        }
        #endregion

        #region Stats Calculation
        protected FullSummarizer fullSummarizer;
        protected ProjectSummaryStats projectSummary;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        protected virtual void onMainViewModelFirstLoaded(object sender, EventArgs e)
        {
            onMainViewModelFirstLoadedTimer.Stop();
            InitializeSummarizer();
            calculatePlannedBackgroundWorker.RunWorkerAsync();
        }

        protected virtual void InitializeSummarizer()
        {
            //when view is closed too fast
            if (MainViewModel == null)
                return;

            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            List<VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONCollection.AsQueryable(), ReportableCollection);
            projectSummary = new ProjectSummaryStats(MainViewModel.Entities, loadPROGRESS.DATA_DATE, reportInterval, firstAlignedDataDate, projectVariationAdjustment);
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT.NUMBER, loadPROJECT.CURRENCYCONVERSION, reportInterval, firstAlignedDataDate, SUBJOBCollection, primeroUnitOfWork);
            fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, loadPROJECT.NUMBER);
        }

        protected abstract IEnumerable<IReportable> ReportableCollection { get; }

        protected void calculatePlannedBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            isBusy = true;
            if (calculatePlannedBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            BackgroundWorkerBuildStats();
        }

        protected virtual void BackgroundWorkerBuildStats()
        {
            if(fullSummarizer != null)
            {
                fullSummarizer.BuildBudgetedOnly();
                fullSummarizer.BuildEarnedAndRemaining();
            }
        }

        public bool isBusy { get; set; }
        protected void CalculatePlannedBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isBusy = false;
            mainThreadDispatcher.BeginInvoke(new Action(() => BackgroundRefresh()));
        }

        #endregion

        #region Set current data date
        bool isFirstLoaded;
        protected void SetPROGRESStoCurrentDateOnLoaded(PROGRESS entity)
        {
            if(entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live progress not found")));

            loadPROGRESS = entity;
            if (!isFirstLoaded)
                mainThreadDispatcher.BeginInvoke(new Action(() => DateChange(DateNavigationType.Current)));
        }

        protected void delayedPROGRESSSavingDispatcher_Tick(object sender, EventArgs e)
        {
            delayedPROGRESSSavingDispatcher.Stop();
            var PROGRESSCollectionViewModel =
                (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROGRESS>();
            mainThreadDispatcher.BeginInvoke(new Action(() => PROGRESSCollectionViewModel.Save(loadPROGRESS)));
            FullRefresh();
        }

        public string DataDate
        {
            get
            {
                if (loadPROGRESS == null || loadPROGRESS.DATA_DATE == null)
                    return string.Empty;

                return loadPROGRESS.DATA_DATE.ToString("g");
            }
        }

        public bool CanDateBackward()
        {
            if (MainViewModel == null || MainViewModel.IsLoading)
                return false;

            if (loadPROGRESS.DATA_DATE > loadPROGRESS.PROGRESS_START)
                return true;

            return false;
        }

        public bool CanDateForward()
        {
            //if (isBusy)
            //    return false;

            if (MainViewModel == null || MainViewModel.IsLoading)
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
            if (isBusy)
                return;

            var interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            int multiplier;
            if (navigationType == DateNavigationType.Current)
            {
                var timeDifferenceFromCurrent = loadPROGRESS.DATA_DATE - DateTime.Now;

                if (timeDifferenceFromCurrent.TotalSeconds > interval.TotalSeconds)
                {
                    do
                    {
                        loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(-1 * interval.Days);
                    } while (loadPROGRESS.DATA_DATE > DateTime.Now);
                }
                else
                {
                    if (timeDifferenceFromCurrent.TotalSeconds < -1 * interval.TotalSeconds)
                        do
                        {
                            loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(1 * interval.Days);
                        } while (loadPROGRESS.DATA_DATE < DateTime.Now - interval);
                    else
                        return;
                }
            }
            else
            {
                multiplier = navigationType == DateNavigationType.Forward ? 1 : -1;
                loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(multiplier * interval.Days);
            }

            delayedPROGRESSSavingDispatcher.Start();
        }
        #endregion

        #region Refresh
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
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
            if (isPushingToP6 || loadPROGRESS == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
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
            var ParameterObj = scheduling_view_model as ISupportParameter;
            ParameterObj.Parameter = new object[] { loadPROGRESS, mappingSelectionType, loadPROJECT };
        }

        protected void onSchedulingViewModelLoaded(IEnumerable<ICanAssignP6> entities)
        {
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            if (PROJECTTASK.Count() == 0)
            {
                onSchedulingViewModelLoadFailed("No activities found, please check if activity code is marked as " + progress_type);
                return;
            }

            IEnumerable<ICanAssignP6> deliverables = entities.Where(x => x.Total_Units > 0);
            #region reset budgeted on progress
            IEnumerable<TASK> task_source = scheduling_view_model.TASK_Source;

            //reset all tasks target to 0
            foreach(TASK task in task_source)
            {
                task.act_work_qty = 0;
                task.target_work_qty = 0;
                task.remain_work_qty = 0;
                task.duration_type = P6DURATION_TYPE.DT_FixedQty.ToString();
                task.complete_pct_type = P6COMPLETE_TYPE.CP_Units.ToString();
            }

            foreach (ICanAssignP6 deliverable in deliverables)
            {
                IEnumerable<P6_ASSIGNMENT> deliverable_assignments = deliverable.P6_Assignments;
                foreach (P6_ASSIGNMENT deliverable_assignment in deliverable_assignments)
                {
                    TASK actual_context_task = task_source.FirstOrDefault(x => x.task_code == deliverable_assignment.P6_ACTIVITYID);
                    P6_AssignmentProjection p6_assignment = new P6_AssignmentProjection(deliverable, deliverable_assignment);

                    if (actual_context_task != null && actual_context_task.delete_date == null)
                    {
                        actual_context_task.target_work_qty += p6_assignment.UNITS;
                        actual_context_task.remain_work_qty += p6_assignment.UNITS;
                    }
                }
            }
            #endregion

            List<string> processedP6Task = new List<string>();
            TimeSpan intervalTimeSpan = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            LoadingScreenManager.ShowLoadingScreen(deliverables.Count());
            string errorMessage = string.Empty;

            foreach (ICanAssignP6 deliverable in deliverables)
            {
                LoadingScreenManager.Progress();

                IReportable current_progress_deliverable = deliverable as IReportable;
                if (current_progress_deliverable == null)
                    continue;

                bool isNullProgress = false;
                //comment this off because duration needs to be calculated even if deliverable is not progressed
                if (current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate == null || current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate.Count() == 0)
                    isNullProgress = true;

                DateTime? first_progress_date = isNullProgress ? (DateTime?)null : current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate.Min(x => x.EARNED_DATE);
                DateTime? last_progress_date = isNullProgress ? (DateTime?)null : current_progress_deliverable.PROGRESS_ITEM_UpToCurrentDataDate.Max(x => x.EARNED_DATE);

                decimal total_percentage_to_date = current_progress_deliverable.Total_Percentage_ToDate;
                if (deliverable.P6_Assignments.Count == 0)
                    continue;

                //only process applicable assignments
                List<P6_ASSIGNMENT> p6_assignments = deliverable.P6_Assignments.Where(assignment => assignment.LOW_VALUE <= (total_percentage_to_date + 0.01m)).OrderBy(assignment => assignment.LOW_VALUE).ToList();

                for (int i = 0; i < p6_assignments.Count; i++)
                {
                    P6_ASSIGNMENT p6_assignment = p6_assignments[i];
                    TASK P6TASK = PROJECTTASK.FirstOrDefault(P6Task => P6Task.task_code == p6_assignment.P6_ACTIVITYID);
                    if (P6TASK != null && P6TASK.delete_date == null)
                    {
                        //set activity start date
                        DateTime? first_earned_week_start_date = isNullProgress ? (DateTime?)null : ((DateTime)first_progress_date).AddDays(-1 * intervalTimeSpan.Days).AddSeconds(1);
                        bool any_write_exclusions = P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.NONE.ToString()) || P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.FINISH.ToString());

                        if ((P6TASK.act_start_date == null || !any_write_exclusions))
                            if(!isNullProgress)
                                P6TASK.act_start_date = ((DateTime)first_earned_week_start_date).Date.AddHours(6);

                        //current activity assignment value must be limited to total earned percentage
                        decimal high_percentage_to_use = p6_assignment.HIGH_VALUE > total_percentage_to_date ? total_percentage_to_date : p6_assignment.HIGH_VALUE;

                        //current percentage pro-rate
                        decimal current_percentage = ((high_percentage_to_use - p6_assignment.LOW_VALUE) + 0.01m);

                        //full assignment percentage used to calculate remaining units
                        decimal full_assignment_percentage = ((p6_assignment.HIGH_VALUE - p6_assignment.LOW_VALUE) + 0.01m);

                        //current activity assignment unit
                        decimal current_assignment_units = current_percentage * deliverable.Total_Units;

                        //current activity full assignment units to calculate remaining units
                        decimal full_assignment_units = full_assignment_percentage * deliverable.Total_Units;

                        //if this is the first time processing the task
                        //another way of doing this is to reset everything to zero and not started, but we do not want to override user changes on the p6 schedule
                        if (!processedP6Task.Any(x => x == P6TASK.task_code))
                            P6TASK.act_work_qty = current_assignment_units;
                        else
                            P6TASK.act_work_qty += current_assignment_units;

                        if (P6TASK.target_work_qty <= 0)
                        {
                            errorMessage = P6TASK.task_code +  " doesn't have budgeted units, please re-populate budgeted units on baseline";
                            break;
                        }

                        if (P6TASK.remain_work_qty >= 0)
                            P6TASK.remain_work_qty = P6TASK.target_work_qty - P6TASK.act_work_qty;

                        if (P6TASK.remain_work_qty < 0)
                        {
                            errorMessage = "Negative remaining units on " + P6TASK.task_code + " because budgeted units is less than earned units, please re-populate budgeted units on baseline";
                            break;
                        }

                        if (P6TASK.remain_work_qty == 0)
                        {
                            P6TASK.status_code = P6TASKSTATUS.TK_Complete.ToString();
                            //when user select none or user select start only, don't update finish
                            any_write_exclusions = P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.NONE.ToString()) || P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.START.ToString());
                            if(P6TASK.act_end_date == null || !any_write_exclusions)
                                if(!isNullProgress)
                                P6TASK.act_end_date = ((DateTime)last_progress_date).Date.AddHours(18);
                        }
                        else if (P6TASK.remain_work_qty > 0)
                        {
                            P6TASK.status_code = P6TASKSTATUS.TK_Active.ToString();
                            //when user select none or user select finish only, don't update start
                            any_write_exclusions = P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.NONE.ToString()) || P6TASK.TASKACTV.Any(x => x.ACTVCODE.short_name == P6_BluePrints_Override.FINISH.ToString());
                            if(P6TASK.act_start_date == null || !any_write_exclusions)
                                if (!isNullProgress)
                                    P6TASK.act_start_date = ((DateTime)first_progress_date).Date.AddHours(6);

                            P6TASK.act_end_date = null;

                            //defines how much percentage of units this assignment will take up when it is fully assigned, so that we can estimate the total duration to apply productivity to
                            decimal current_task_to_activity_percentage = full_assignment_units / (decimal)P6TASK.target_work_qty;
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
                                    decimal override_productivity;
                                    if (reportable.Current_Productivity == 0 && reportable.Override_Productivity == 0)
                                        override_productivity = 1;
                                    else if (reportable.Override_Productivity != null)
                                        override_productivity = (decimal)reportable.Override_Productivity;
                                    else
                                        override_productivity = reportable.Current_Productivity;

                                    decimal current_assignment_remaining_duration_per_productivity = current_assignment_remaining_duration / override_productivity;
                                    if (!processedP6Task.Any(x => x == P6TASK.task_code))
                                        P6TASK.remain_drtn_hr_cnt = current_assignment_remaining_duration_per_productivity;
                                    else
                                        P6TASK.remain_drtn_hr_cnt += current_assignment_remaining_duration_per_productivity;
                                }
                            }
                            else
                            {
                                if (!processedP6Task.Any(x => x == P6TASK.task_code))
                                    P6TASK.remain_drtn_hr_cnt = current_assignment_remaining_duration;
                                else
                                    P6TASK.remain_drtn_hr_cnt += current_assignment_remaining_duration;
                            }
                        }
                        else if (P6TASK.status_code == P6TASKSTATUS.TK_NotStart.ToString())
                            P6TASK.status_code = P6TASKSTATUS.TK_Active.ToString();

                        if (!processedP6Task.Any(x => x == P6TASK.task_code))
                            processedP6Task.Add(P6TASK.task_code);

                        scheduling_view_model.Save_Task(P6TASK);
                    }
                    else
                    {
                        errorMessage = "P6 activity named " + p6_assignment.P6_ACTIVITYID + " not found, please check deliverable's assignment";
                        break;
                    }
                }
            }

            LoadingScreenManager.CloseLoadingScreen();

            //if(errorMessage == string.Empty)
            //{
            //    IEnumerable<TASK> progressed_milestones = getProgressedMilestones(scheduling_view_model.TASK_Source, scheduling_view_model.TASKPRED_Source);
            //    LoadingScreenManager.ShowLoadingScreen(progressed_milestones.Count());
            //    foreach (TASK progressed_milestone in progressed_milestones)
            //    {
            //        scheduling_view_model.Save_Task(progressed_milestone);
            //        LoadingScreenManager.Progress();
            //    }

            //    LoadingScreenManager.CloseLoadingScreen();
            //}

            destroy_scheduling_view_model();

            if (errorMessage == string.Empty)
                MessageBoxService.ShowMessage(BluePrintsResources.P6_Assignment_Progress_Write_Success);
            else
                MessageBoxService.ShowMessage(errorMessage);
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
                foreach(TASK predecessor_task in predecessor_tasks)
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
            //Need to perform full refresh because MainViewModel repository entity state is messed from scheduling view model, i.e. productivity doesn't update anymore after pushing to P6
            FullRefresh();
        }

        protected abstract void dispose_scheduling_view_model();
        #endregion
        
        #region Custom Summary
        private decimal cumulative_total_units = 0;
        private decimal cumulative_current_units = 0;
        public void CustomSummary(CustomSummaryEventArgs e)
        {
            if (e.IsTotalSummary || e.IsGroupSummary)
            {
                if (e.SummaryProcess == CustomSummaryProcess.Start)
                {
                    cumulative_total_units = 0;
                    cumulative_current_units = 0;
                }
                if (e.SummaryProcess == CustomSummaryProcess.Calculate)
                {
                    var total_units = ((IReportable)e.Row).Total_Units;

                    cumulative_total_units += total_units;
                    if ((((GridSummaryItem)e.Item).FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage))
                    {
                        cumulative_current_units += ((BASELINE_ITEMProgress)e.Row).Earned_Units_ToDate;
                        if (cumulative_total_units > 0)
                            e.TotalValue = cumulative_current_units / cumulative_total_units;
                    }
                    else if ((((GridSummaryItem)e.Item).FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Earned_Percentage_OnDataDate))
                    {
                        cumulative_current_units += ((IReportable)e.Row).Earned_Units_OnDataDate;
                        if (cumulative_total_units > 0)
                            e.TotalValue = cumulative_current_units / cumulative_total_units;
                    }
                    else if ((((GridSummaryItem)e.Item).FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Baseline_Percentage))
                    {
                        IReportable reportable = ((IReportable)e.Row);
                        if (reportable.Stats != null && reportable.Stats.Budgeted != null && reportable.Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                        {
                            cumulative_current_units += reportable.Stats.Budgeted.CurrentPeriodCumulativeDataPoint.BudgetedUnits;
                            e.TotalValue = cumulative_current_units / cumulative_total_units;
                        }
                    }
                    else if ((((GridSummaryItem)e.Item).FieldName) == "Stats.Budgeted.CurrentPeriodDataPoint." + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Stats.Budgeted.CurrentPeriodDataPoint.UnitsPercentage))
                    {
                        IReportable reportable = ((IReportable)e.Row);
                        if (reportable.Stats != null && reportable.Stats.Budgeted != null && reportable.Stats.Budgeted.CurrentPeriodDataPoint != null)
                        {
                            cumulative_current_units += reportable.Stats.Budgeted.CurrentPeriodDataPoint.Units;
                            e.TotalValue = cumulative_current_units / cumulative_total_units;
                        }
                    }
                    else
                    {
                        e.TotalValue = 0;
                    }
                }
            }
        }
        #endregion

        #region Reporting
        protected override string ExportExcelFilename()
        {
            return loadPROJECT.NUMBER + "_Progress_" + loadPROGRESS.DATA_DATE.ToString("dd-MMM-yy") + ".xlsx";
        }
        #endregion

        #region Disposing
        protected override void OnClose(CancelEventArgs e)
        {
//            if (loadPROJECT != null)
//#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
//#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            base.OnClose(e);
        }


        #endregion

        #region Entities Wrapper Properties
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
    }
}
