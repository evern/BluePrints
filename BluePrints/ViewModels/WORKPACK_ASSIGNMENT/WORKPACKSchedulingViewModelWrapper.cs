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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, WORKPACK_ASSIGNMENTProjectionFunc);
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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK_Dashboard>>
            ConstructMainViewModelProjection()
        {
            var getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            var getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            var getBASELINE_ITEMSFunc = loaderCollection.GetCollectionFunc<BASELINE_ITEM>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getDELIVERABLES_STATUSESFunc = loaderCollection.GetCollectionFunc<DELIVERABLES_STATUS>();

            return
                query =>
                    WORKPACK_DashboardQueries.SummarizeWORKPACKDashboard(
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), getPROGRESSFunc, getBASELINEFunc,
                        getBASELINE_ITEMSFunc, getPROGRESS_ITEMSFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, 
                        mappingType == BaselineMappingSelectionType.Modified);
        }

        public
            Action
            <Func<IEnumerable<TASK>>, Func<IEnumerable<PROJWBS>>, Func<IEnumerable<WORKPACK_Dashboard>>,
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

                List<MissingP6Activities> missingActivities = new List<MissingP6Activities>();

                foreach (WORKPACK_ASSIGNMENT WORKPACK_ASSIGNMENT in currentPROJECTWORKPACK_ASSIGNMENTS)
                {
                    TASK existingTask = P6Tasks.FirstOrDefault(x => x.task_code == WORKPACK_ASSIGNMENT.P6_ACTIVITYID);
                    decimal remainingValue = (WORKPACK_ASSIGNMENT.HIGH_VALUE - WORKPACK_ASSIGNMENT.LOW_VALUE) + 1;

                    if (existingTask != null)
                    {
                        decimal remainingProductivity = (decimal)((existingTask.target_drtn_hr_cnt == null || existingTask.target_drtn_hr_cnt == 0) ? remainingValue : (remainingValue / existingTask.target_drtn_hr_cnt));

                        existingTask.target_work_qty += remainingValue;
                        existingTask.remain_work_qty += remainingValue;
                    }
                    else
                    {
                        WORKPACK missingWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID == WORKPACK_ASSIGNMENT.GUID_WORKPACK);
                        missingActivities.Add(new MissingP6Activities() { INTERNAL_NUM = missingWORKPACK == null ? string.Empty : missingWORKPACK.INTERNAL_NAME1, P6_ACTIVITY = WORKPACK_ASSIGNMENT.P6_ACTIVITYID, UNITS = remainingValue });
                    }
                }

                ((P6EntitiesUnitOfWork)IP6EntitiesUnitOfWork).Context.SaveChanges();

                if(missingActivities.Count > 0)
                {
                    DialogCollectionViewModel<MissingP6Activities> missingActivitiesViewModel = DialogCollectionViewModel<MissingP6Activities>.Create(missingActivities);
                    MissingActivitiesDialogService.ShowDialog(MessageButton.OK,
                    "Missing P6 Activities", "MissingAssignments", missingActivitiesViewModel);
                }
                else
                    MessageBoxService.ShowMessage(CommonResources.WORKPACK_ASSIGNMENT_P6WriteComplete);
            }
        }

        #endregion
    }
}