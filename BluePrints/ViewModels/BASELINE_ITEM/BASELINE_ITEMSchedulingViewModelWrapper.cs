using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class BASELINE_ITEMSchedulingViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINE_ITEMSchedulingViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMSchedulingViewModelWrapper());
        }

        #region Used as Dependency Delegate

        public Action<IEnumerable<BASELINE_ITEMProjection>> OnMappingViewModelLoaded { get; set; }

        private bool isFromPROGRESS
        {
            get { return OnMappingViewModelLoaded != null; }
        }

        #endregion

        #region Database Operation

        private Data.PROJECT loadPROJECT;
        private P6Data.PROJECT loadP6PROJECT;
        private PROGRESS loadPROGRESS;
        private BASELINE loadBASELINE;
        private BaselineMappingSelectionType mappingType;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IDialogService ActivityDetailDialogService
        {
            get { return this.GetRequiredService<IDialogService>("ActivityIdDialog"); }
        }

        protected override void InitializeParameters(object parameter)
        {
            var obj = (object[]) parameter;

            if (isFromPROGRESS)
                loadPROGRESS = (PROGRESS) obj[0];
            else
                loadBASELINE = (BASELINE) obj[0];

            mappingType = (BaselineMappingSelectionType) obj[1];
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => loadBASELINE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => loadPROGRESS = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEM_ASSIGNMENTS, BASELINE_ITEM_ASSIGNMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJECT, P6PROJECTProjectionFunc, x => loadP6PROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.TASK, P6TASKProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, PROJWBSProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadBASELINE.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            if (isFromPROGRESS)
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID);
            else
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == ProgressType.Design);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            if (isFromPROGRESS)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROGRESS.GUID_PROJECT && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadBASELINE.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>>
            BASELINE_ITEM_ASSIGNMENTProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x =>
                            x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        private Func<IRepositoryQuery<P6Data.PROJECT>, IQueryable<P6Data.PROJECT>> P6PROJECTProjectionFunc
            ()
        {
            string projectName;
            if (isFromPROGRESS)
                projectName = loadPROGRESS.P6PROGRESS_NAME;
            else if (mappingType == BaselineMappingSelectionType.Modified)
                projectName = loadBASELINE.P6MODBASELINE_NAME;
            else
                projectName = loadBASELINE.P6BASELINE_NAME;

            return query => query.Where(x => x.proj_short_name == projectName);
        }

        private Func<IRepositoryQuery<TASK>, IQueryable<TASK>> P6TASKProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> PROJWBSProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            BASELINE BASELINE = loaderCollection.GetObject<BASELINE>();
            IEnumerable<RATE> RATES = loaderCollection.GetCollection<RATE>();
            IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSES = loaderCollection.GetCollection<DELIVERABLES_STATUS>();
            IEnumerable<P6_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENTS = loaderCollection.GetCollection<P6_ASSIGNMENT>();

            return
                query =>
                    BASELINE_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(query.OrderBy(x => x.INTERNAL_NUM), BASELINE, RATES, DELIVERABLES_STATUSES, BASELINE_ITEM_ASSIGNMENTS);
        }

        public
            Action
            <BluePrints.Data.PROJECT, IEnumerable<TASK>, IEnumerable<PROJWBS>, IEnumerable<BASELINE_ITEMProjection>,
                CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>, bool
            > WinformFormHostInitialization { get; set; }

        public Action RefreshWinformView { get; set; }

        //Used by baseline_item scheduling view model to fix assignment
        public Func<object> OnEntitiesLoadedParameterCallBack;
        public Action<IEnumerable<BASELINE_ITEMProjection>, object> OnEntitiesLoadedWithParameterCallBack;

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProjection> entities)
        {
            if (OnEntitiesLoadedWithParameterCallBack != null)
            {
                object onLoadedParameter = OnEntitiesLoadedParameterCallBack?.Invoke();
                OnEntitiesLoadedWithParameterCallBack?.Invoke(entities, onLoadedParameter);

                //Self destruct after entities has been returned
                CleanUpEntitiesLoader();
                return;
            }

            if (isFromPROGRESS)
                mainThreadDispatcher.BeginInvoke(new Action(() => OnMappingViewModelLoaded(entities)));
            else
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            WinformFormHostInitialization(loadPROJECT, loaderCollection.GetCollection<TASK>(),
                                loaderCollection.GetCollection<PROJWBS>(), entities,
                                BASELINE_ITEM_ASSIGNMENTSCollectionViewModel,
                                mappingType == BaselineMappingSelectionType.Modified)));
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTBASELINE_ITEMSMappingViewModelWrapper"; }
        }

        public IEnumerable<TASK> P6TASKCollection
        {
            get
            {
                var collection = GetEntities<TASK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.task_name);
                return collection;
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
                return collection;
            }
        }

        public ICollectionViewModel<BluePrints.P6Data.TASK> P6TASKCollectionViewModel
        {
            get { return (ICollectionViewModel<BluePrints.P6Data.TASK>) loaderCollection.GetViewModel<TASK>(); }
        }

        public bool CanCopyWorkpackAssignments()
        {
            return LoginCredentials.CurrentUser.NAME == BluePrintsResources.AdminUsername;
        }

        public void CopyWorkpackAssignments()
        {
            if (MessageBoxService.ShowMessage("This will clear current assignments and attempt to copy from workpack assignments, are you sure you want to continue?", "Warning", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            BASELINE_ITEM_ASSIGNMENTSCollectionViewModel.BaseBulkDelete(BASELINE_ITEM_ASSIGNMENTCollection);
            CreateWORKPACKSchedulingViewModelWrapper();
        }

        public WORKPACKSchedulingViewModelWrapper CreateWORKPACKSchedulingViewModelWrapper()
        {
            WORKPACKSchedulingViewModelWrapper workpackSchedulingViewModelWrapper = null;

            if (loadPROJECT != null)
            {
                workpackSchedulingViewModelWrapper = new WORKPACKSchedulingViewModelWrapper();
                workpackSchedulingViewModelWrapper.SuppressNotification = true;
                workpackSchedulingViewModelWrapper.OnEntitiesLoadedWithParameterCallBack = OnWorkpackDashboardLoaded;

                object[] parameter = new object[] { loadBASELINE, BaselineMappingSelectionType.Original };
                var supportParameterViewModel = workpackSchedulingViewModelWrapper as ISupportParameter;
                supportParameterViewModel.Parameter = parameter;
            }

            return workpackSchedulingViewModelWrapper;
        }

        private void OnWorkpackDashboardLoaded(IEnumerable<WORKPACK_Dashboard> projections, object parameter)
        {
            LoadingScreenManager.ShowLoadingScreen(projections.Count());

            List<P6_ASSIGNMENT> newAssignments = new List<P6_ASSIGNMENT>();
            foreach (WORKPACK_Dashboard workpack in projections)
            {
                IEnumerable<WORKPACK_ASSIGNMENT> projectWORKPACK_ASSIGNMENTS = workpack.ObservableWORKPACK_ASSIGNMENTS;
                foreach (WORKPACK_ASSIGNMENT WORKPACK_ASSIGNMENT in projectWORKPACK_ASSIGNMENTS)
                {
                    IEnumerable<BASELINE_ITEMProjection> baseline_items = MainViewModel.Entities.Where(x => x.Entity.GUID_WORKPACK == workpack.GUID);
                    foreach(var baseline_item in baseline_items)
                    {
                        newAssignments.Add(new P6_ASSIGNMENT()
                        {
                            GUID = Guid.Empty,
                            GUID_PROJECT = loadPROJECT.GUID,
                            HIGH_VALUE = WORKPACK_ASSIGNMENT.HIGH_VALUE,
                            LOW_VALUE = WORKPACK_ASSIGNMENT.LOW_VALUE,
                            P6_ACTIVITYID = WORKPACK_ASSIGNMENT.P6_ACTIVITYID,
                            GUID_ORIGINAL = baseline_item.Entity.GUID_ORIGINAL,
                            ISMODIFIEDBASELINE = mappingType == BaselineMappingSelectionType.Modified
                        });
                    }
                }

                LoadingScreenManager.Progress();
            }

            BASELINE_ITEM_ASSIGNMENTSCollectionViewModel.BulkSave(newAssignments);
        }

        public void PushToP6()
        {
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string ProjectName;
            if (mappingType == BaselineMappingSelectionType.Modified)
                ProjectName = loadBASELINE.P6MODBASELINE_NAME;
            else
                ProjectName = loadBASELINE.P6BASELINE_NAME;


            BluePrints.P6Data.PROJECT P6PROJECT = IP6EntitiesUnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == ProjectName && x.delete_date == null);
            if (P6PROJECT != null)
            {
                IEnumerable<TASK> P6Tasks = P6PROJECT.TASK.ToArray().AsEnumerable();
                foreach (TASK Task in P6Tasks)
                {
                    Task.act_work_qty = 0;
                    Task.remain_work_qty = 0;
                    Task.target_work_qty = 0;
                }

                IEnumerable<TASKRSRC> ExistingTaskResource = P6PROJECT.TASKRSRC.ToArray().AsEnumerable();

                double taskrsrcCount = ExistingTaskResource.Count();
                foreach (var TaskRsrc in ExistingTaskResource)
                {
                    IP6EntitiesUnitOfWork.TASKRSRC.Remove(TaskRsrc);
                }

                List<P6ActivityAssignment> missingActivities = new List<P6ActivityAssignment>();
                foreach(BASELINE_ITEMProjection baseline_item in MainViewModel.Entities)
                {
                    IEnumerable<P6_ASSIGNMENT> projectBASELINE_ITEM_ASSIGNMENTS = baseline_item.BASELINE_ITEM_ASSIGNMENTS;

                    foreach (P6_ASSIGNMENT BASELINE_ITEM_ASSIGNMENT in projectBASELINE_ITEM_ASSIGNMENTS)
                    {
                        TASK existingTask = P6Tasks.FirstOrDefault(x => x.task_code == BASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID);
                        P6ActivityAssignment P6Assignment = new P6ActivityAssignment(baseline_item, BASELINE_ITEM_ASSIGNMENT);

                        if (existingTask != null && existingTask.delete_date == null)
                        {
                            existingTask.target_work_qty += P6Assignment.UNITS;
                            existingTask.remain_work_qty += P6Assignment.UNITS;
                        }
                        else
                        {
                            missingActivities.Add(P6Assignment);
                        }
                    }
                }

                ((P6EntitiesUnitOfWork)IP6EntitiesUnitOfWork).Context.SaveChanges();
                if (missingActivities.Count > 0)
                {
                    DialogCollectionViewModel<P6ActivityAssignment> missingActivitiesViewModel = DialogCollectionViewModel<P6ActivityAssignment>.Create(missingActivities);
                    ActivityDetailDialogService.ShowDialog(MessageButton.OK,
                    "Missing P6 Activities", "MissingAssignments", missingActivitiesViewModel);
                }
                else
                    MessageBoxService.ShowMessage(BluePrintsResources.P6AssignmentWriteComplete);
            }
        }

        public void ReassignP6Ids()
        {
            IEnumerable<TASK> validTASKS;
            List<P6ActivityAssignment> missingActivities = getMissingP6Activities(out validTASKS, true);
            List<P6ActivityRemap> p6ActivitiesRemap = new List<P6ActivityRemap>();

            if(missingActivities.Count > 0)
            {
                foreach(P6ActivityAssignment missingActivity in missingActivities)
                {
                    if (!p6ActivitiesRemap.Any(x => x.P6_OLD_ACTIVITY == missingActivity.P6_ACTIVITY))
                        p6ActivitiesRemap.Add(ViewModelSource.Create(() => new P6ActivityRemap() { P6_OLD_ACTIVITY = missingActivity.P6_ACTIVITY }));
                }

                P6ActivityAssignmentDialogViewModel<P6ActivityRemap> activitiesRemapViewModel = P6ActivityAssignmentDialogViewModel<P6ActivityRemap>.CreateViewModel(p6ActivitiesRemap, loadPROJECT.NUMBER, validTASKS);
                if(ActivityDetailDialogService.ShowDialog(MessageButton.OKCancel, "Re-Assign", "MissingAssignmentsRemap", activitiesRemapViewModel) == MessageResult.OK)
                {
                    IEnumerable<P6ActivityRemap> userRemappedActivities = p6ActivitiesRemap.Where(x => x.P6_NEW_ACTIVITY != null && x.P6_NEW_ACTIVITY != string.Empty);
                    List<P6ActivityRemap> validUserRemappedActivities = new List<P6ActivityRemap>();

                    foreach(P6ActivityRemap userRemappedActivity in userRemappedActivities)
                    {
                        if (validTASKS.Any(x => x.task_code == userRemappedActivity.P6_NEW_ACTIVITY))
                        {
                            validUserRemappedActivities.Add(userRemappedActivity);
                        }
                    }

                    List<P6ActivityAssignment> reassignActivities = new List<P6ActivityAssignment>();
                    if (userRemappedActivities.Count() > 0)
                    {
                        List<P6ActivityAssignment> validReassignments = new List<P6ActivityAssignment>();
                        foreach (P6ActivityAssignment missingActivity in missingActivities)
                        {
                            P6ActivityRemap userRemappedActivity = validUserRemappedActivities.FirstOrDefault(x => x.P6_OLD_ACTIVITY == missingActivity.P6_ACTIVITY);
                            if(userRemappedActivity != null)
                            {
                                missingActivity.Reassign(userRemappedActivity.P6_NEW_ACTIVITY);
                                reassignActivities.Add(missingActivity);
                            }
                        }
                    }

                    if(reassignActivities.Count > 0)
                    {
                        BASELINE_ITEM_ASSIGNMENTSCollectionViewModel.BulkSave(reassignActivities.Select(x => x.baseline_item_assignment));
                        RefreshWinformView?.Invoke();

                        MessageBoxService.ShowMessage(reassignActivities.Count + " activities re-assigned");
                    }
                }
            }
            else
                MessageBoxService.ShowMessage("All Assignments Valid");
        }

        public void DeleteAssignments()
        {
            IEnumerable<TASK> validTASKS;
            List<P6ActivityAssignment> missingActivities = getMissingP6Activities(out validTASKS);
            if (missingActivities.Count > 0)
            {
                DialogCollectionViewModel<P6ActivityAssignment> missingActivitiesViewModel = DialogCollectionViewModel<P6ActivityAssignment>.Create(missingActivities);
                ActivityDetailDialogService.ShowDialog(MessageButton.OK, "Invalid Assignments", "MissingAssignments", missingActivitiesViewModel);

                if (MessageBoxService.ShowMessage("Do you want to continue deleting all invalid assignments?", "Warning", MessageButton.OKCancel) == MessageResult.OK)
                {
                    BASELINE_ITEM_ASSIGNMENTSCollectionViewModel.BaseBulkDelete(missingActivities.Select(x => x.baseline_item_assignment));
                    FullRefresh();
                    RefreshWinformView?.Invoke();
                }
            }
            else
                MessageBoxService.ShowMessage("All Assignments Valid");
        }

        private List<P6ActivityAssignment> getMissingP6Activities(out IEnumerable<TASK> validTASKS, bool getAllActivities = false)
        {
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string ProjectName;
            if (mappingType == BaselineMappingSelectionType.Modified)
                ProjectName = loadBASELINE.P6MODBASELINE_NAME;
            else
                ProjectName = loadBASELINE.P6BASELINE_NAME;

            List<P6ActivityAssignment> missingActivities = new List<P6ActivityAssignment>();
            BluePrints.P6Data.PROJECT P6PROJECT = IP6EntitiesUnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == ProjectName && x.delete_date == null);
            if (P6PROJECT != null)
            {
                validTASKS = P6PROJECT.TASK.ToArray().AsEnumerable();
                foreach (BASELINE_ITEMProjection baseline_item in MainViewModel.Entities)
                {
                    IEnumerable<P6_ASSIGNMENT> projectBASELINE_ITEM_ASSIGNMENTS = baseline_item.BASELINE_ITEM_ASSIGNMENTS;
                    foreach (P6_ASSIGNMENT BASELINE_ITEM_ASSIGNMENT in projectBASELINE_ITEM_ASSIGNMENTS)
                    {
                        if(getAllActivities)
                            missingActivities.Add(new P6ActivityAssignment(baseline_item, BASELINE_ITEM_ASSIGNMENT));
                        else
                        {
                            TASK existingTask = validTASKS.FirstOrDefault(x => x.task_code == BASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID);
                            if (existingTask == null || existingTask.delete_date != null)
                            {
                                missingActivities.Add(new P6ActivityAssignment(baseline_item, BASELINE_ITEM_ASSIGNMENT));
                            }
                        }
                    }
                }
            }
            else
                validTASKS = null;

            return missingActivities;
        }

        public void Refresh()
        {
            RefreshWinformView?.Invoke();
        }

        public IEnumerable<P6_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENTCollection
        {
            get
            {
                var collection = GetEntities<P6_ASSIGNMENT>();
                return collection;
            }
        }

        public CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
        BASELINE_ITEMSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<BASELINE_ITEM>();
            }
        }

        public CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEM_ASSIGNMENTSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<P6_ASSIGNMENT>();
            }
        }
        #endregion
    }
}