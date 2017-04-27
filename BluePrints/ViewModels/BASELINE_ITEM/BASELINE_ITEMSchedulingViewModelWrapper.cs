using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.DataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.Data.Helpers;
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
        CollectionViewModelsWrapper
        <BASELINE_ITEM, BASELINE_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<BASELINE_ITEM, BASELINE_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
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

        private IDialogService MissingActivitiesDialogService
        {
            get { return this.GetRequiredService<IDialogService>("MissingActivityIdDialog"); }
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
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
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

        private Func<IRepositoryQuery<BASELINE_ITEM_ASSIGNMENT>, IQueryable<BASELINE_ITEM_ASSIGNMENT>>
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
            IEnumerable<BASELINE_ITEM_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENTS = loaderCollection.GetCollection<BASELINE_ITEM_ASSIGNMENT>();

            return
                query =>
                    BASELINE_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(query, BASELINE, RATES, DELIVERABLES_STATUSES, BASELINE_ITEM_ASSIGNMENTS);
        }

        public
            Action
            <Func<IEnumerable<TASK>>, Func<IEnumerable<PROJWBS>>, Func<IEnumerable<BASELINE_ITEMProjection>>,
                CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>, bool
            > windowsFormHostViewInitialization { get; set; }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProjection> entities)
        {
            var
                WORKPACK_ASSIGNMENTCollectionViewModel =
                    (CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<WORKPACK_ASSIGNMENT>();

            if (isFromPROGRESS)
                mainThreadDispatcher.BeginInvoke(new Action(() => OnMappingViewModelLoaded(entities)));
            else
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            windowsFormHostViewInitialization(loaderCollection.GetCollectionFunc<TASK>(),
                                loaderCollection.GetCollectionFunc<PROJWBS>(), () => entities.AsQueryable(),
                                WORKPACK_ASSIGNMENTCollectionViewModel,
                                mappingType == BaselineMappingSelectionType.Modified)));
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTWORKPACKSMappingViewModelWrapper"; }
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

        public void PushToP6()
        {
            var IBluePrintsEntitiesUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
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

                List<MissingP6Activities> missingActivities = new List<MissingP6Activities>();
                foreach(BASELINE_ITEMProjection baseline_item in MainViewModel.Entities)
                {
                    IEnumerable<BASELINE_ITEM_ASSIGNMENT> projectBASELINE_ITEM_ASSIGNMENTS = baseline_item.BASELINE_ITEM_ASSIGNMENTS;
                    decimal totalUnits = baseline_item.TOTAL_UNITS;

                    foreach (BASELINE_ITEM_ASSIGNMENT BASELINE_ITEM_ASSIGNMENT in projectBASELINE_ITEM_ASSIGNMENTS)
                    {
                        TASK existingTask = P6Tasks.FirstOrDefault(x => x.task_code == BASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID);
                        decimal currentAssignmentUnits = ((BASELINE_ITEM_ASSIGNMENT.HIGH_VALUE - BASELINE_ITEM_ASSIGNMENT.LOW_VALUE) + 0.01m) * totalUnits;

                        if (existingTask != null)
                        {
                            existingTask.target_work_qty += currentAssignmentUnits;
                            existingTask.remain_work_qty += currentAssignmentUnits;
                        }
                    }
                }

                ((P6EntitiesUnitOfWork)IP6EntitiesUnitOfWork).Context.SaveChanges();
                if (missingActivities.Count > 0)
                {
                    DialogCollectionViewModel<MissingP6Activities> missingActivitiesViewModel = DialogCollectionViewModel<MissingP6Activities>.Create(missingActivities);
                    MissingActivitiesDialogService.ShowDialog(MessageButton.OK,
                    "Missing P6 Activities", "MissingAssignments", missingActivitiesViewModel);
                }
                else
                    MessageBoxService.ShowMessage(CommonResources.WORKPACK_ASSIGNMENT_P6WriteComplete);
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

        public CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<WORKPACK_ASSIGNMENT>();
            }
        }
        #endregion
    }
}