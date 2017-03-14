using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Scheduler;
using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using BluePrints.Common;
using DevExpress.Mvvm.POCO;
using BluePrints.Data.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Forms.Integration;
using BluePrints.Views;
using System.Windows.Threading;
using System.ComponentModel;
using BluePrints.Common.Projections;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;

namespace BluePrints.ViewModels
{
    public class WORKPACKSchedulingViewModelWrapper :
        CollectionViewModelsWrapper
        <WORKPACK, WORKPACK_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<WORKPACK, WORKPACK_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACKSchedulingViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new WORKPACKSchedulingViewModelWrapper());
        }

        #region Used as Dependency Delegate

        public Action<IEnumerable<WORKPACK_Dashboard>> OnPROJECTWORKPACKSMappingViewModelLoaded { get; set; }

        private bool isFromPROGRESS
        {
            get { return OnPROJECTWORKPACKSMappingViewModelLoaded != null; }
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
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection
                .AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(0,
                    bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, null,
                    isContinueLoadingAfterBASELINE, null, OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader
                <Data.PROJECT, Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(1,
                    bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, typeof(BASELINE),
                    isContinueLoadingAfterPROJECT, null, OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(2,
                    bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, typeof(BASELINE),
                    isContinueLoadingAfterPROGRESS, null, OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(3,
                    bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, typeof(BASELINE));
            loaderCollection
                .AddEntitiesLoader<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>(4,
                    bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, WORKPACK_ASSIGNMENTProjectionFunc,
                    typeof(WORKPACK));
            loaderCollection
                .AddEntitiesLoader<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(5,
                    bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc, typeof(BASELINE),
                    null, null,
                    OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(6,
                    bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROGRESS), null, null,
                    OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(7,
                    bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, typeof(PROGRESS),
                    null, null,
                    OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<P6Data.PROJECT, P6Data.PROJECT, int, IP6EntitiesUnitOfWork>(8,
                    p6UnitOfWorkFactory, x => x.PROJECT, P6PROJECTProjectionFunc, null, isContinueLoadingAfterP6PROJECT, null,
                    OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<P6Data.TASK, P6Data.TASK, int, IP6EntitiesUnitOfWork>(9, p6UnitOfWorkFactory,
                    x => x.TASK, P6TASKProjectionFunc, typeof(P6Data.PROJECT), null, null, OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<PROJWBS, PROJWBS, int, IP6EntitiesUnitOfWork>(10, p6UnitOfWorkFactory,
                    x => x.PROJWBS, PROJWBSProjectionFunc, typeof(P6Data.PROJECT));
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "BASELINE"))));
                return false;
            }

            loadBASELINE = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterPROJECT(IEnumerable<Data.PROJECT> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            loadPROJECT = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterP6PROJECT(IEnumerable<P6Data.PROJECT> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "P6PROJECT"))));
                return false;
            }

            loadP6PROJECT = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterPROGRESS(IEnumerable<PROGRESS> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROGRESS"))));
                return false;
            }

            loadPROGRESS = entities.First();
            return true;
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

        private Func<IRepositoryQuery<WORKPACK_ASSIGNMENT>, IQueryable<WORKPACK_ASSIGNMENT>>
            WORKPACK_ASSIGNMENTProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x =>
                            x.WORKPACK.GUID_PROJECT == loadPROJECT.GUID &&
                            x.ISMODIFIEDBASELINE == (mappingType == BaselineMappingSelectionType.Modified));
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
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK_Dashboard>>
            ConstructMainViewModelProjection()
        {
            var getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            var getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            var getBASELINE_ITEMSFunc = loaderCollection.GetCollectionFunc<BASELINE_ITEM>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();

            return
                query =>
                    WORKPACK_DashboardQueries.MappingWORKPACKDashboard(
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), getPROGRESSFunc, getBASELINEFunc,
                        getBASELINE_ITEMSFunc, getPROGRESS_ITEMSFunc, getRATESFunc,
                        mappingType == BaselineMappingSelectionType.Modified);
        }

        public
            Action
            <Func<IQueryable<TASK>>, Func<IQueryable<PROJWBS>>, Func<IQueryable<WORKPACK_Dashboard>>,
                CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>, bool
            > windowsFormHostViewInitialization { get; set; }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<WORKPACK_Dashboard> entities)
        {
            var
                WORKPACK_ASSIGNMENTCollectionViewModel =
                    (CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<WORKPACK_ASSIGNMENT>();

            if (isFromPROGRESS)
                mainThreadDispatcher.BeginInvoke(new Action(() => OnPROJECTWORKPACKSMappingViewModelLoaded(entities)));
            else
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            windowsFormHostViewInitialization(loaderCollection.GetCollectionFunc<TASK>(),
                                loaderCollection.GetCollectionFunc<PROJWBS>(), () => entities.AsQueryable(),
                                WORKPACK_ASSIGNMENTCollectionViewModel,
                                mappingType == BaselineMappingSelectionType.Modified)));
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            return;
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
                IEnumerable<WORKPACK_ASSIGNMENT> currentPROJECTWORKPACK_ASSIGNMENTS = loadPROJECT.WORKPACK.Where(x => x.DELETED == null).SelectMany(x => x.WORKPACK_ASSIGNMENT.Where(y => y.DELETED == null && y.ISMODIFIEDBASELINE == (mappingType == BaselineMappingSelectionType.Modified))).ToArray().AsEnumerable();
                IEnumerable<TASKRSRC> ExistingTaskResource = P6PROJECT.TASKRSRC.ToArray().AsEnumerable();
                IEnumerable<TASK> P6Tasks = P6PROJECT.TASK.ToArray().AsEnumerable();
                foreach (TASK Task in P6Tasks)
                {
                    Task.act_work_qty = 0;
                    Task.remain_work_qty = 0;
                    Task.target_work_qty = 0;
                }

                double taskrsrcCount = ExistingTaskResource.Count();
                foreach (var TaskRsrc in ExistingTaskResource)
                {
                    IP6EntitiesUnitOfWork.TASKRSRC.Remove(TaskRsrc);
                }

                foreach (WORKPACK_ASSIGNMENT WORKPACK_ASSIGNMENT in currentPROJECTWORKPACK_ASSIGNMENTS)
                {
                    TASK existingTask = P6Tasks.FirstOrDefault(x => x.task_code == WORKPACK_ASSIGNMENT.P6_ACTIVITYID);

                    if (existingTask != null)
                    {
                        decimal remainingValue = (WORKPACK_ASSIGNMENT.HIGH_VALUE - WORKPACK_ASSIGNMENT.LOW_VALUE) + 1;
                        decimal remainingProductivity = (decimal)((existingTask.target_drtn_hr_cnt == null || existingTask.target_drtn_hr_cnt == 0) ? remainingValue : (remainingValue / existingTask.target_drtn_hr_cnt));

                        existingTask.target_work_qty += remainingValue;
                        existingTask.remain_work_qty += remainingValue;
                    }
                }

                ((P6EntitiesUnitOfWork)IP6EntitiesUnitOfWork).Context.SaveChanges();
                MessageBoxService.ShowMessage(CommonResources.WORKPACK_ASSIGNMENT_P6WriteComplete);
            }
        }

        #endregion
    }
}