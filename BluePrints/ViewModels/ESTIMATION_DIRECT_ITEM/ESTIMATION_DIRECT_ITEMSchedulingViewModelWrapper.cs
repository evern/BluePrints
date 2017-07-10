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
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>, IHaveCanvasWidth
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper());
        }

        #region Used as Dependency Delegate

        public Action<IEnumerable<ESTIMATION_DIRECT_ITEMProjection>> OnMappingViewModelLoaded { get; set; }

        private bool isFromPROGRESS
        {
            get { return OnMappingViewModelLoaded != null; }
        }

        #endregion

        #region Database Operation

        private Data.PROJECT loadPROJECT;
        private P6Data.PROJECT loadP6PROJECT;
        private PROGRESS loadPROGRESS;
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        private BaselineMappingSelectionType mappingType;
        private DEPARTMENT defaultConstructionDEPARTMENT;
        private Data.PHASE defaultConstructionPHASE;

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
            var obj = (object[])parameter;

            if (isFromPROGRESS)
                loadPROGRESS = (PROGRESS)obj[0];
            else
                loadESTIMATION_DIRECT = (ESTIMATION_DIRECT)obj[0];

            mappingType = (BaselineMappingSelectionType)obj[1];
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, x => defaultConstructionDEPARTMENT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, x => defaultConstructionPHASE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => loadESTIMATION_DIRECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => loadPROGRESS = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEM_ASSIGNMENTS, BASELINE_ITEM_ASSIGNMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS, ESTIMATION_DIRECT_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJECT, P6PROJECTProjectionFunc, x => loadP6PROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.TASK, P6TASKProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, PROJWBSProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == EstimationStatus.Live);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.SiteDirect);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == BluePrintsResources.DefaultConstructionDepartment);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.INTERNAL_NUM == BluePrintsResources.WorkpackDefaultConstructionPhase);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID && x.GUID_DEPARTMENT == defaultConstructionDEPARTMENT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == ProgressType.Construct);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM_ASSIGNMENT>, IQueryable<BASELINE_ITEM_ASSIGNMENT>>
            BASELINE_ITEM_ASSIGNMENTProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x =>
                            x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEM>> ESTIMATION_DIRECT_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_ESTIMATION_DIRECT == loadESTIMATION_DIRECT.GUID);
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
                projectName = loadESTIMATION_DIRECT.P6MODBASELINE_NAME;
            else
                projectName = loadESTIMATION_DIRECT.P6BASELINE_NAME;

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
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.ESTIMATION_DIRECT_ITEMProjectionQuery(query, loaderCollection.GetCollection<RATE>(), ProjectSTOCK_CODECollection, loaderCollection.GetCollection<COMMODITY_CODE>());
        }

        //Used by baseline_item scheduling view model to fix assignment
        public Func<object> OnEntitiesLoadedParameterCallBack;
        public Action<IEnumerable<ESTIMATION_DIRECT_ITEMProjection>, object> OnEntitiesLoadedWithParameterCallBack;

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            if (OnEntitiesLoadedWithParameterCallBack != null)
            {
                object onLoadedParameter = OnEntitiesLoadedParameterCallBack?.Invoke();
                OnEntitiesLoadedWithParameterCallBack?.Invoke(entities, onLoadedParameter);

                //Self destruct after entities has been returned
                CleanUpEntitiesLoader();
                return;
            }

            Beg = P6TASKCollection.Where(x => x.target_start_date != null).Min(x => (DateTime)x.target_start_date);
            End = P6TASKCollection.Where(x => x.target_start_date != null).Max(x => (DateTime)x.target_end_date);

            VisBeg = new DateTime(Beg.Ticks);
            VisEnd = new DateTime(End.Ticks);

            SelBeg = new DateTime(Beg.Ticks);
            SelEnd = new DateTime(End.Ticks);
            
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTESTIMATION_DIRECT_ITEMSMappingViewModelWrapper"; }
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

        public IEnumerable<PROJWBS> P6PROJWBSCollection
        {
            get
            {
                var collection = GetEntities<PROJWBS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.wbs_name);
                return collection;
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
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            }
        }

        public IEnumerable<STOCK_CODE> STOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> GlobalSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> ProjectSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<BASELINE_ITEM_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENTCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM_ASSIGNMENT>();
                return collection;
            }
        }

        public ICollectionViewModel<BluePrints.P6Data.TASK> P6TASKCollectionViewModel
        {
            get { return (ICollectionViewModel<BluePrints.P6Data.TASK>)loaderCollection.GetViewModel<TASK>(); }
        }

        public CollectionViewModel<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
        ESTIMATION_DIRECT_ITEMSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<ESTIMATION_DIRECT_ITEM>();
            }
        }

        public CollectionViewModel<BASELINE_ITEM_ASSIGNMENT, BASELINE_ITEM_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEM_ASSIGNMENTSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<BASELINE_ITEM_ASSIGNMENT, BASELINE_ITEM_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<BASELINE_ITEM_ASSIGNMENT>();
            }
        }

        public void PushToP6()
        {
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string ProjectName;
            if (mappingType == BaselineMappingSelectionType.Modified)
                ProjectName = loadESTIMATION_DIRECT.P6MODBASELINE_NAME;
            else
                ProjectName = loadESTIMATION_DIRECT.P6BASELINE_NAME;


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
                foreach (ESTIMATION_DIRECT_ITEMProjection ESTIMATION_DIRECT_ITEM in MainViewModel.Entities)
                {
                    IEnumerable<BASELINE_ITEM_ASSIGNMENT> projectBASELINE_ITEM_ASSIGNMENTS = ESTIMATION_DIRECT_ITEM.P6Assignments;

                    foreach (BASELINE_ITEM_ASSIGNMENT BASELINE_ITEM_ASSIGNMENT in projectBASELINE_ITEM_ASSIGNMENTS)
                    {
                        TASK existingTask = P6Tasks.FirstOrDefault(x => x.task_code == BASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID);
                        P6ActivityAssignment P6Assignment = new P6ActivityAssignment(ESTIMATION_DIRECT_ITEM, BASELINE_ITEM_ASSIGNMENT);

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

            if (missingActivities.Count > 0)
            {
                foreach (P6ActivityAssignment missingActivity in missingActivities)
                {
                    if (!p6ActivitiesRemap.Any(x => x.P6_OLD_ACTIVITY == missingActivity.P6_ACTIVITY))
                        p6ActivitiesRemap.Add(ViewModelSource.Create(() => new P6ActivityRemap() { P6_OLD_ACTIVITY = missingActivity.P6_ACTIVITY }));
                }

                P6ActivityAssignmentDialogViewModel<P6ActivityRemap> activitiesRemapViewModel = P6ActivityAssignmentDialogViewModel<P6ActivityRemap>.CreateViewModel(p6ActivitiesRemap, loadPROJECT.NUMBER, validTASKS);
                if (ActivityDetailDialogService.ShowDialog(MessageButton.OKCancel, "Re-Assign", "MissingAssignmentsRemap", activitiesRemapViewModel) == MessageResult.OK)
                {
                    IEnumerable<P6ActivityRemap> userRemappedActivities = p6ActivitiesRemap.Where(x => x.P6_NEW_ACTIVITY != null && x.P6_NEW_ACTIVITY != string.Empty);
                    List<P6ActivityRemap> validUserRemappedActivities = new List<P6ActivityRemap>();

                    foreach (P6ActivityRemap userRemappedActivity in userRemappedActivities)
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
                            if (userRemappedActivity != null)
                            {
                                missingActivity.Reassign(userRemappedActivity.P6_NEW_ACTIVITY);
                                reassignActivities.Add(missingActivity);
                            }
                        }
                    }

                    if (reassignActivities.Count > 0)
                    {
                        BASELINE_ITEM_ASSIGNMENTSCollectionViewModel.BulkSave(reassignActivities.Select(x => x.baseline_item_assignment));
                        //RefreshWinformView?.Invoke();

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
                    //RefreshWinformView?.Invoke();
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
                ProjectName = loadESTIMATION_DIRECT.P6MODBASELINE_NAME;
            else
                ProjectName = loadESTIMATION_DIRECT.P6BASELINE_NAME;

            List<P6ActivityAssignment> missingActivities = new List<P6ActivityAssignment>();
            BluePrints.P6Data.PROJECT P6PROJECT = IP6EntitiesUnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == ProjectName && x.delete_date == null);
            if (P6PROJECT != null)
            {
                validTASKS = P6PROJECT.TASK.ToArray().AsEnumerable();
                foreach (ESTIMATION_DIRECT_ITEMProjection ESTIMATION_DIRECT_ITEM in MainViewModel.Entities)
                {
                    IEnumerable<BASELINE_ITEM_ASSIGNMENT> projectBASELINE_ITEM_ASSIGNMENTS = ESTIMATION_DIRECT_ITEM.P6Assignments;
                    foreach (BASELINE_ITEM_ASSIGNMENT BASELINE_ITEM_ASSIGNMENT in projectBASELINE_ITEM_ASSIGNMENTS)
                    {
                        if (getAllActivities)
                            missingActivities.Add(new P6ActivityAssignment(ESTIMATION_DIRECT_ITEM, BASELINE_ITEM_ASSIGNMENT));
                        else
                        {
                            TASK existingTask = validTASKS.FirstOrDefault(x => x.task_code == BASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID);
                            if (existingTask == null || existingTask.delete_date != null)
                            {
                                missingActivities.Add(new P6ActivityAssignment(ESTIMATION_DIRECT_ITEM, BASELINE_ITEM_ASSIGNMENT));
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
            //RefreshWinformView?.Invoke();
        }
        #endregion

        #region DragDrop
        

        #endregion

        #region GanttChart Properties
        private List<GanttData> p6Activities;
        public virtual List<GanttData> P6Activities
        {
            get
            {
                if(MainViewModel != null && p6Activities == null)
                {
                    p6Activities = new List<GanttData>();
                    p6Activities.AddRange(P6TASKCollection.OrderBy(x => x.target_start_date).Select(x => GanttData.Create(x, this)).ToArray().AsEnumerable());
                    p6Activities.AddRange(P6PROJWBSCollection.Select(x => GanttData.Create(x, this)).ToArray().AsEnumerable());
                    summarizeActivities(P6Activities);
                }

                return p6Activities;
            }
        }

        private void summarizeActivities(IEnumerable<GanttData> activities)
        {
            foreach (var activity in activities)
            {
                if (activity.ActivityType == AppointmentActivityType.WBS)
                {
                    activity.AssignedUnits = 0;
                }
            }

            foreach (var activity in activities)
                if (activity.ActivityType == AppointmentActivityType.WBS)
                {
                    List<GanttData> allChildrenActivities = new List<GanttData>();
                    getAllChildrens(activities, activity, allChildrenActivities);
                    //return childTASKInfos.Sum(x => x.AssignedUnits);
                    if (allChildrenActivities.Count() != 0)
                    {
                        activity.Start = allChildrenActivities.Min(x => x.Start);
                        activity.End = allChildrenActivities.Max(x => x.End);
                    }
                }
        }

        private void getAllChildrens(IEnumerable<GanttData> allActivities, GanttData parentActivity, List<GanttData> childrenCollection)
        {
            IEnumerable<GanttData> childActivities = allActivities.Where(x => x.ParentId == parentActivity.Id);

            if(childActivities.Count() > 0)
                parentActivity.WBSLevel += 1;

            foreach (var childActivity in childActivities)
            {
                childrenCollection.Add(childActivity);
                getAllChildrens(allActivities, childActivity, childrenCollection);
            }
        }

        public virtual DateTime Beg { get; set; }
        public virtual DateTime End { get; set; }
        public virtual DateTime VisBeg { get; set; }
        public virtual DateTime VisEnd { get; set; }
        public virtual DateTime SelBeg { get; set; }
        public virtual DateTime SelEnd { get; set; }
        public virtual double CanvasWidth { get; set; }

        public void NodeExpanded()
        {
            Task.Factory
                .StartNew(() => Thread.Sleep(100))
                .ContinueWith(t => { this.RaisePropertyChanged(x => x.SelBeg); });
        }
        #endregion
    }
}