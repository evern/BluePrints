using BaseModel.Data.Helpers;
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
        IBluePrintsEntitiesUnitOfWork bluePrintsUOW;
        IP6EntitiesUnitOfWork p6UOW;
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
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Progress_Report.ToString());
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainProjectionEntity> entities)
        {
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterEntitySavedCallBack;
            MainViewModel.OnMappingAdditionalChangedEntitiesProperties = OnMappingAdditionalChangedEntitiesProperties;
            MainViewModel.OnBeforeAssignRepositoryToExistingProjection = OnBeforeAssignRepositoryToExistingProjection;
            MainViewModel.DisablePasteRowLevel = true;
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
        protected void OnAfterEntitySavedCallBack(TMainProjectionEntity projectionEntity,TMainEntity entity, bool isNewEntity)
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

        public override string UnifiedValueValidation(TMainProjectionEntity projection, string field_name, object new_value)
        {
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage))
            {
                var newPercentage = (decimal)new_value;
                if (newPercentage > projection.MaxPercentage)
                    return "Percentage cannot exceed " + projection.MaxPercentage.ToString();
                else if (newPercentage < projection.MinPercentage)
                    return "Percentage cannot be less than " + projection.MinPercentage.ToString();
            }

            return string.Empty;
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
            DateTime reporting_data_date = loadPROGRESS.DATA_DATE;
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT.NUMBER, loadPROJECT.CURRENCYCONVERSION, reportInterval, firstAlignedDataDate, SUBJOBCollection, reporting_data_date, primeroUnitOfWork);
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

                return loadPROGRESS.DATA_DATE.ToString("dd-MMM-yy");
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

        private bool CanReverseProgressFromP6()
        {
            if (isReversingFromP6 || loadPROGRESS == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return false;

            return true;
        }

        bool isReversingFromP6;
        public void ReverseProgressFromP6()
        {
            if (MessageBoxService.ShowMessage("Warning\nThis action will update progresses based on p6 and is not reversible\nDo you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
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
            MessageBoxService.ShowMessage("Progress backup completed");

            isReversingFromP6 = true;
            //Stats will be built in SummarizeSinglePROJECTDashboard within SummarizeBASELINE_ITEMDashboard in ConstructMainViewModelProjection
            MainViewModel.Refresh();
            scheduling_view_model.OnViewModelLoaded = ReverseProgressP6_Loaded;
            scheduling_view_model.OnViewModelLoadFailed = onSchedulingViewModelLoadFailed;
            var ParameterObj = scheduling_view_model as ISupportParameter;
            ParameterObj.Parameter = new object[] { loadPROGRESS, BaselineMappingSelectionType.Original, loadPROJECT };
        }

        public void ReverseProgressP6_Loaded(IEnumerable<ICanAssignP6> entities)
        {
            List<P6_ASSIGNMENT> allCompletedAssignments = new List<P6_ASSIGNMENT>();
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);

            //specify the oldest date data points can edit to
            DateTime progressLimitDate = loadPROGRESS.PREVIOUS_REPORT_DATE == null ? loadPROGRESS.REPORT_DATE == null ? loadPROGRESS.PROGRESS_START : (DateTime)loadPROGRESS.REPORT_DATE : (DateTime)loadPROGRESS.PREVIOUS_REPORT_DATE;

            LoadingScreenManager.ShowLoadingScreen(entities.Count());
            foreach (ICanAssignP6 deliverable in entities)
            {
                foreach(P6_ASSIGNMENT assignment in deliverable.P6_Assignments.OrderByDescending(x => x.HIGH_VALUE))
                {
                    TASK task = PROJECTTASK.FirstOrDefault(x => x.task_code == assignment.P6_ACTIVITYID);
                    if(task != null && task.target_work_qty != null)
                    {
                        if(task.act_work_qty > 0)
                        {
                            //assuming previous assignment is completed what the units on this deliverable should at least be
                            decimal supposedUnitsForPreviousAssignment = deliverable.Total_Units * (assignment.LOW_VALUE - 0.01m);
                            decimal supposedUnitsForCurrentAssignment = deliverable.Total_Units * ((assignment.HIGH_VALUE - assignment.LOW_VALUE) + 0.01m);

                            //because current task can be assigned to multiple deliverable, actual earned units needs to be pro-rated
                            decimal proRateValue = supposedUnitsForCurrentAssignment / (decimal)task.target_work_qty;
                            decimal proRateUnits = (decimal)task.act_work_qty * proRateValue;

                            decimal totalSupposedUnits = supposedUnitsForPreviousAssignment + proRateUnits;

                            decimal totalUnitsEarned = deliverable.Progresses.Sum(x => x.EARNED_UNITS);
                            decimal unitsParity = totalSupposedUnits - totalUnitsEarned;
                            if (unitsParity > 0.001m)
                            {
                                //Add units for previous task assignments
                                IEnumerable<P6_ASSIGNMENT> completedAssignments = deliverable.P6_Assignments.Where(x => x.HIGH_VALUE < assignment.LOW_VALUE);
                                addOrIncreaseDataPointsForTasks(completedAssignments, deliverable, firstAlignedDataDate, progressLimitDate);
                                allCompletedAssignments.AddRange(completedAssignments);

                                //Add units for current task assignment
                                List<P6_ASSIGNMENT> currentAssignment = new List<P6_ASSIGNMENT>();
                                currentAssignment.Add(assignment);
                                addOrIncreaseDataPointsForTasks(currentAssignment, deliverable, firstAlignedDataDate, progressLimitDate, unitsParity);
                            }
                            else if (unitsParity < -0.001m)
                            {
                                removeOrReduceDataPointsForTasks(deliverable, unitsParity);
                                bluePrintsUOW.SaveChanges();
                            }

                            break;
                        }
                        //Unfinished : Support for reverse for above - mark activity as TK_NotStart for all forward assignments is first assignment has 0 units
                        //else
                        //{
                        //    //assuming previous assignment is completed what the units on this deliverable should at least be
                        //    decimal supposedUnitsForCurrentAssignment = deliverable.Total_Units * ((assignment.HIGH_VALUE - assignment.LOW_VALUE) + 0.01m);

                        //    decimal totalUnitsEarned = deliverable.Progresses.Sum(x => x.EARNED_UNITS);
                        //    decimal unitsParity = 0 - totalUnitsEarned;
                        //    if (unitsParity < 0)
                        //    {
                        //        removeOrReduceDataPointsForTasks(deliverable, unitsParity);
                        //        bluePrintsUOW.SaveChanges();

                        //        break;
                        //    }
                        //}
                    }
                }

                LoadingScreenManager.Progress();
            }

            List<string> processedTasks = new List<string>();
            //Fix p6 for task that should be completed
            foreach(P6_ASSIGNMENT assignment in allCompletedAssignments)
            {
                if(!processedTasks.Any(x => x == assignment.P6_ACTIVITYID))
                {
                    TASK p6Task = PROJECTTASK.FirstOrDefault(x => x.task_code == assignment.P6_ACTIVITYID);
                    if(p6Task != null)
                    {
                        TASK repositoryTASK = p6UOW.TASK.FirstOrDefault(x => x.task_id == p6Task.task_id);
                        //DataUtils.ShallowCopy(repositoryTASK, p6Task);
                        if(repositoryTASK != null)
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

            LoadingScreenManager.CloseLoadingScreen();
            MessageBoxService.ShowMessage("Progress from P6 is completed");
        }

        private void removeOrReduceDataPointsForTasks(ICanAssignP6 deliverable, decimal reduceUnits)
        {
            reduceUnits *= -1;
            foreach(PROGRESS_ITEM progress in deliverable.Progresses.OrderByDescending(x => x.EARNED_DATE))
            {
                PROGRESS_ITEM currentDateProgress = bluePrintsUOW.PROGRESS_ITEMS.FirstOrDefault(x => x.GUID == progress.GUID);
                if(reduceUnits > 0)
                {
                    if (currentDateProgress.EARNED_UNITS < reduceUnits)
                    {
                        reduceUnits -= currentDateProgress.EARNED_UNITS;
                        currentDateProgress.EARNED_UNITS = 0;
                    }
                    else if(currentDateProgress.EARNED_UNITS >= reduceUnits)
                    {
                        currentDateProgress.EARNED_UNITS -= reduceUnits;
                    }
                }
            }
        }

        private void addOrIncreaseDataPointsForTasks(IEnumerable<P6_ASSIGNMENT> completedAssignments, ICanAssignP6 deliverable, DateTime firstAlignedDataDate, DateTime progressLimitDate, decimal? manualParity = null)
        {
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            foreach (P6_ASSIGNMENT completedAssignment in completedAssignments.OrderBy(x => x.HIGH_VALUE))
            {
                TASK task = PROJECTTASK.FirstOrDefault(x => x.task_code == completedAssignment.P6_ACTIVITYID);
                DateTime taskStartDate;
                DateTime taskEndDate;

                 if(task != null)
                {
                    if (task.act_start_date != null)
                        taskStartDate = (DateTime)task.act_start_date;
                    else if (task.target_start_date != null)
                        taskStartDate = (DateTime)task.target_start_date;
                    else
                        taskStartDate = progressLimitDate;

                    if (task.act_end_date != null)
                        taskEndDate = (DateTime)task.act_end_date;
                    else if (task.target_end_date != null)
                        taskEndDate = (DateTime)task.target_end_date;
                    else
                        taskEndDate = progressLimitDate.AddDays(1);

                    if (taskStartDate < progressLimitDate)
                        taskStartDate = progressLimitDate;

                    if (taskEndDate < progressLimitDate)
                        taskEndDate = progressLimitDate.AddDays(1);

                    decimal totalUnitsToAddToDeliverable;
                    if (manualParity == null)
                    {
                        //assuming previous assignment is completed what the units on this deliverable should at least be
                        decimal supposedUnitsForAssignments = deliverable.Total_Units * completedAssignment.HIGH_VALUE;
                        IQueryable<PROGRESS_ITEM> repositoryProgress = bluePrintsUOW.PROGRESS_ITEMS.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.GUID_ORIBASEITEM == deliverable.OriginalEntityKey);
                        decimal earnedUnits = repositoryProgress.Sum(x => x.EARNED_UNITS);
                        if (earnedUnits >= supposedUnitsForAssignments)
                            continue;

                        totalUnitsToAddToDeliverable = deliverable.Total_Units * ((completedAssignment.HIGH_VALUE - completedAssignment.LOW_VALUE) + 0.01m);
                    }
                    else
                        totalUnitsToAddToDeliverable = (decimal)manualParity;

                    IEnumerable<DateTime> interpolatedDates = getInterpolationDataDate(taskStartDate, taskEndDate, firstAlignedDataDate);
                    if(interpolatedDates.Count() > 0)
                    {
                        decimal totalUnitsToAddPerPeriod = totalUnitsToAddToDeliverable / interpolatedDates.Count();
                        foreach(DateTime interpolatedDate in interpolatedDates)
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

        public void ReverseProgressFromP6_ModelLoaded(IEnumerable<ICanAssignP6> entities)
        {
            IEnumerable<TASK> PROJECTTASK = scheduling_view_model.TASK_Source;
            if (PROJECTTASK.Count() == 0)
            {
                onSchedulingViewModelLoadFailed("No activities found, please check if activity code is marked as " + progress_type);
                return;
            }

            //task to mark as completed because next assignment is completed
            List<TASK> taskAsFinished = new List<TASK>();

            //documenting any occurence when task is not found
            List<string> taskNotFound = new List<string>();

            //documenting task without start date, unlikely to happen because task already have actuals
            List<string> taskWithoutStartDate = new List<string>();

            //documenting deliverables without interpolation date
            List<string> deliverableWithoutDates = new List<string>();
            IEnumerable<ICanAssignP6> deliverables = entities;

            LoadingScreenManager.ShowLoadingScreen(PROJECTTASK.Count());
            foreach (TASK task in PROJECTTASK)
            {
                if (taskAsFinished.Any(x => x.task_code == task.task_code))
                    continue;

                if (task.act_work_qty == null || task.target_work_qty == null)
                    continue;

                decimal P6Units = (decimal)task.act_work_qty;
                decimal P6Budget = (decimal)task.target_work_qty;
                if (P6Budget == 0)
                    continue;

                string s = string.Empty;
                if (task.task_code == "A3260")
                    s = string.Empty;
                if (P6Units > 0)
                    s = string.Empty;

                //if (assignmentLookupPercentage == 0)
                //    assignmentLookupPercentage = 0.01m;
                //assignmentLookupPercentage = assignmentLookupPercentage > 1 ? 1 : assignmentLookupPercentage;

                //finds all deliverables associated to this task
                IEnumerable<ICanAssignP6> assignedDeliverables = deliverables.Where(x => x.P6_Assignments.Any(y => y.P6_ACTIVITYID == task.task_code));
                IEnumerable<P6_ASSIGNMENT> allAssignments = assignedDeliverables.SelectMany(x => x.P6_Assignments);
                P6_ASSIGNMENT currentTaskAssignments = allAssignments.First(x => x.P6_ACTIVITYID == task.task_code);
                DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
                decimal assignmentLookupPercentage = currentTaskAssignments.LOW_VALUE;
                //ICanAssignP6 deliverable = assignedDeliverables.First(x => x.OriginalEntityKey)

                foreach (ICanAssignP6 deliverable in assignedDeliverables)
                {
                    foreach(P6_ASSIGNMENT assignment in deliverable.P6_Assignments.OrderBy(x => x.HIGH_VALUE))
                    {
                        if(assignment.P6_ACTIVITYID == task.task_code && (assignment.LOW_VALUE <= assignmentLookupPercentage && assignmentLookupPercentage <= assignment.HIGH_VALUE))
                        {
                            decimal earnedUnits = deliverable.Progresses == null || deliverable.Progresses.Count() == 0 ? 0 : deliverable.Progresses.Sum(x => x.EARNED_UNITS);
                            //full assignment percentage used to calculate remaining units
                            decimal full_assignment_percentage = ((assignment.HIGH_VALUE - assignment.LOW_VALUE) + 0.01m);
                            //current activity full assignment units to calculate remaining units
                            decimal full_assignment_units = full_assignment_percentage * deliverable.Total_Units;
                            decimal proRateFactor = full_assignment_units / P6Budget;

                            //units supposedly earned for this deliverable
                            decimal supposedlyEarned = deliverable.Total_Units * (assignment.LOW_VALUE - 0.01m);

                            decimal P6UnitsUntilCurrent = (P6Units * proRateFactor) + supposedlyEarned;
                            decimal unitsDifferences = P6UnitsUntilCurrent - earnedUnits;

                            IEnumerable<P6_ASSIGNMENT> previousAssignments = deliverable.P6_Assignments.Where(x => x.HIGH_VALUE < full_assignment_percentage);
                            DateTime? startDate = null;
                            DateTime? endDate = null;
                            foreach (P6_ASSIGNMENT previousAssignment in previousAssignments)
                            {
                                TASK previousTASK = PROJECTTASK.FirstOrDefault(x => x.task_code == previousAssignment.P6_ACTIVITYID);
                                if(startDate == null)
                                    startDate = previousTASK.act_start_date != null ? (DateTime)previousTASK.act_start_date : (DateTime)previousTASK.target_start_date;

                                DateTime endDateToUse = previousTASK.act_end_date != null ? (DateTime)previousTASK.act_end_date : (DateTime)previousTASK.target_end_date;

                                if (endDate == null || endDate < endDateToUse)
                                    endDate = endDateToUse;

                                if (previousTASK != null && !taskAsFinished.Any(x => x.task_code == previousAssignment.P6_ACTIVITYID))
                                    taskAsFinished.Add(previousTASK);
                            }

                            if (unitsDifferences > 0)
                            {
                                List<DateTime> interpolationDataDate = getInterpolationDataDate((DateTime)startDate, (DateTime)endDate, firstAlignedDataDate);
                                if (interpolationDataDate.Count > 0)
                                {
                                    decimal unitsInflationPerPeriod = (unitsDifferences / interpolationDataDate.Count);

                                    if (unitsInflationPerPeriod > 0)
                                    {
                                        foreach (DateTime interpolationDate in interpolationDataDate)
                                        {
                                            DateTime interpolationDateFormat = interpolationDate.Date.AddDays(1).AddSeconds(-1);
                                            PROGRESS_ITEM currentDateProgress = bluePrintsUOW.PROGRESS_ITEMS.FirstOrDefault(x => x.GUID_ORIBASEITEM == deliverable.OriginalEntityKey && x.EARNED_DATE == interpolationDateFormat);
                                            if (currentDateProgress != null)
                                            {
                                                //PROGRESS_ITEM repositoryPROGRESS_ITEM = bluePrintsUOW.PROGRESS_ITEMS.First(x => x.GUID == currentDateProgress.GUID);
                                                currentDateProgress.EARNED_UNITS += unitsInflationPerPeriod;
                                                //bluePrintsUOW.SaveChanges();
                                                //currentDateProgress.EARNED_UNITS += unitsInflationPerPeriod;
                                                //PROGRESS_ITEMSCollectionViewModel.Save(currentDateProgress);
                                            }
                                            else
                                            {
                                                PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                                                newPROGRESS_ITEM.EARNED_UNITS = unitsInflationPerPeriod;
                                                newPROGRESS_ITEM.EARNED_DATE = interpolationDateFormat;
                                                newPROGRESS_ITEM.GUID_ORIBASEITEM = deliverable.OriginalEntityKey;
                                                newPROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
                                                bluePrintsUOW.PROGRESS_ITEMS.Add(newPROGRESS_ITEM);
                                                //bluePrintsUOW.SaveChanges();
                                                //PROGRESS_ITEMSCollectionViewModel.Save(newPROGRESS_ITEM);
                                            }
                                        }
                                    }
                                }
                                else
                                    deliverableWithoutDates.Add(deliverable.P6AssignmentName);
                            }
                            else if (unitsDifferences < 0)
                            {
                                IEnumerable<PROGRESS_ITEM> currentDeliverableProgresses = bluePrintsUOW.PROGRESS_ITEMS.Where(x => x.GUID_ORIBASEITEM == deliverable.OriginalEntityKey).OrderByDescending(x => x.EARNED_DATE);

                                //The otherwise is not likely to happen because units in P6 should be coming from deliverable
                                if (currentDeliverableProgresses.Count() > 0)
                                {
                                    decimal unitsDeflationPerPeriod = (unitsDifferences / currentDeliverableProgresses.Count()) * proRateFactor;
                                    foreach (PROGRESS_ITEM currentDeliverableProgress in currentDeliverableProgresses)
                                    {
                                        currentDeliverableProgress.EARNED_UNITS += unitsDeflationPerPeriod;
                                        //PROGRESS_ITEMSCollectionViewModel.Save(currentDeliverableProgress);
                                    }
                                }
                            }
                        }
                    }
                }

                LoadingScreenManager.Progress();
            }

            foreach (TASK incompleteTASK in taskAsFinished)
            {
                TASK repositoryTASK = p6UOW.TASK.FirstOrDefault(x => x.task_id == incompleteTASK.task_id);
                DataUtils.ShallowCopy(repositoryTASK, incompleteTASK);

                repositoryTASK.status_code = P6TASKSTATUS.TK_Complete.ToString();
                repositoryTASK.act_work_qty = incompleteTASK.target_work_qty;

                if (repositoryTASK.act_start_date == null)
                    repositoryTASK.act_start_date = incompleteTASK.target_start_date;

                if (repositoryTASK.act_end_date == null)
                    repositoryTASK.act_end_date = incompleteTASK.target_end_date;

                repositoryTASK.target_drtn_hr_cnt += repositoryTASK.remain_drtn_hr_cnt;
                repositoryTASK.remain_drtn_hr_cnt = 0;
            }

            p6UOW.SaveChanges();
            bluePrintsUOW.SaveChanges();
            LoadingScreenManager.CloseLoadingScreen();

            destroy_scheduling_view_model();
            MessageBoxService.ShowMessage("Progress from P6 completed");
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

                        if (P6TASK.act_work_qty == 0)
                        {
                            P6TASK.act_start_date = null;
                            P6TASK.act_end_date = null;
                            P6TASK.status_code = P6TASKSTATUS.TK_NotStart.ToString();
                            break;
                        }

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

                        TASK repositoryTASK = p6UOW.TASK.FirstOrDefault(x => x.task_id == P6TASK.task_id);
                        DataUtils.ShallowCopy(repositoryTASK, P6TASK);
                        //scheduling_view_model.Save_Task(P6TASK);
                    }
                    else
                    {
                        errorMessage = "P6 activity named " + p6_assignment.P6_ACTIVITYID + " not found, please check deliverable's assignment";
                        break;
                    }
                }
            }

            p6UOW.SaveChanges();
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
        private decimal cumulative_baseline_units = 0;
        private decimal cumulative_current_units = 0;
        public void CustomSummary(CustomSummaryEventArgs e)
        {
            if (e.IsTotalSummary || e.IsGroupSummary)
            {
                if (e.SummaryProcess == CustomSummaryProcess.Start)
                {
                    cumulative_total_units = 0;
                    cumulative_baseline_units = 0;
                    cumulative_current_units = 0;
                }
                if (e.SummaryProcess == CustomSummaryProcess.Calculate)
                {
                    var total_units = ((IReportable)e.Row).Total_Units;
                    var baseline_units = ((IReportable)e.Row).Budget_Units;

                    cumulative_total_units += total_units;
                    cumulative_baseline_units += baseline_units;

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
                    else if ((((GridSummaryItem)e.Item).FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().SchedulePercentage))
                    {
                        IReportable reportable = ((IReportable)e.Row);
                        if (reportable.Stats != null && reportable.Stats.Budgeted != null && reportable.Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                        {
                            cumulative_current_units += reportable.Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Units;
                            e.TotalValue = cumulative_current_units / cumulative_total_units;
                        }
                    }
                    else if ((((GridSummaryItem)e.Item).FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().ScheduleCurrentPeriodPercentage))
                    {
                        IReportable reportable = ((IReportable)e.Row);
                        if (reportable.Stats != null && reportable.Stats.Budgeted != null && reportable.Stats.Budgeted.CurrentPeriodDataPoint != null)
                        {
                            cumulative_current_units += reportable.Stats.Budgeted.CurrentPeriodDataPoint.Units;
                            e.TotalValue = cumulative_current_units / cumulative_total_units;
                        }
                    }
                    else if ((((GridSummaryItem)e.Item).FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Baseline_Percentage))
                    {
                        if(cumulative_baseline_units > 0)
                        {
                            IReportable reportable = ((IReportable)e.Row);
                            cumulative_current_units += reportable.Earned_Units_ToDate;
                            e.TotalValue = cumulative_current_units / cumulative_baseline_units;
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
