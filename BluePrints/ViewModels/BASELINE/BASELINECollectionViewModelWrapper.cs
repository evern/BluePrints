using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.DataModel;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class BASELINECollectionViewModelWrapper : CollectionViewModelsWrapper<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>>, ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of BASELINECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINECollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINECollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        BluePrints.Data.PROJECT loadPROJECT;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            EntitiesParameter<BluePrints.Data.PROJECT> PROJECTParameter = (EntitiesParameter<BluePrints.Data.PROJECT>)parameter;
            this.loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<BluePrints.Data.PROJECT, BluePrints.Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BluePrints.P6Data.PROJWBS, BluePrints.P6Data.PROJWBS, int, IP6EntitiesUnitOfWork>(1, p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
            //loaderCollection.AddEntitiesLoader<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, WORKPACK_ASSIGNMENTProjectionFunc, typeof(BluePrints.Data.PROJECT));
            //loaderCollection.AddEntitiesLoader<TASKRSRC, TASKRSRC, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, TASKRSRCProjectionFunc, typeof(BluePrints.P6Data.PROJECT));
            //loaderCollection.AddEntitiesLoader<TASK, TASK, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, TASKProjectionFunc, typeof(BluePrints.P6Data.PROJECT));
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<BluePrints.Data.PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        Func<IRepositoryQuery<BluePrints.Data.PROJECT>, IQueryable<BluePrints.Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<BluePrints.P6Data.PROJWBS>, IQueryable<BluePrints.P6Data.PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y").OrderBy(proj => proj.wbs_short_name);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.BASELINES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE> entities)
        {
            MainViewModel.OnBeforeEntitySavedCallBack = this.OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected override void OnEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender == MainViewModel)
                return;

            if (loadPROJECT != null && changedType == typeof(BluePrints.Data.PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(BASELINE entity)
        {
            entity.GUID_PROJECT = this.loadPROJECT.GUID;
        }
        #endregion
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "BASELINECollectionViewModelWrapper";
            }
        }

        public IEnumerable<BluePrints.P6Data.PROJWBS> P6PROJECTSCollection
        {
            get
            {
                var collection = GetEntities<BluePrints.P6Data.PROJWBS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.wbs_short_name);
                return collection;
            }
        }

        public void PushToP6_Original()
        {
            PushToP6(false);
        }

        public void PushToP6_Modified()
        {
            PushToP6(true);
        }

        void PushToP6(bool isGetModifiedP6BASELINE)
        {
            var IBluePrintsEntitiesUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string ProjectName;
            if (isGetModifiedP6BASELINE)
                ProjectName = MainViewModel.SelectedEntity.P6MODBASELINE_NAME;
            else
                ProjectName = MainViewModel.SelectedEntity.P6BASELINE_NAME;

            BluePrints.P6Data.PROJECT P6PROJECT = IP6EntitiesUnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == ProjectName && x.delete_date == null);
            if (P6PROJECT != null)
            {
                IEnumerable<WORKPACK_ASSIGNMENT> currentPROJECTWORKPACK_ASSIGNMENTS = loadPROJECT.WORKPACK.Where(x => x.DELETED == null).SelectMany(x => x.WORKPACK_ASSIGNMENT.Where(y => y.DELETED == null && y.ISMODIFIEDBASELINE == isGetModifiedP6BASELINE)).ToArray().AsEnumerable();
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
            }
        }
        #endregion

        #region ISupportCustomDocumentTypeNameAndParameter
        public bool CanEdit(BASELINE entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService { get { return this.GetService<IDocumentManagerService>(); } }
        public void Edit(BASELINE entity)
        {
            if (entity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<BASELINE_ITEM, Guid>(this, entity.GUID, string.Empty);
        }

        BaselineMappingSelectionType mappingSelectionType = new BaselineMappingSelectionType();
        public void P6BASELINE_ASSIGN()
        {
            mappingSelectionType = BaselineMappingSelectionType.Original;
            Edit(MainViewModel.SelectedEntity);
            mappingSelectionType = BaselineMappingSelectionType.None;
        }

        public void P6MODBASELINE_ASSIGN()
        {
            mappingSelectionType = BaselineMappingSelectionType.Modified;
            Edit(MainViewModel.SelectedEntity);
            mappingSelectionType = BaselineMappingSelectionType.None;
        }

        public string GetCustomDocumentTypeName()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.None)
                return "BASELINE_ITEMCollectionView";

            return "PROJECTWORKPACKDetailsMappingViewHost";
        }

        public object GetCustomDocumentParameter()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.None)
                return new OptionalEntitiesParameter<BluePrints.Data.PROJECT, BASELINE>(null, MainViewModel.SelectedEntity);

            return new object[] { MainViewModel.SelectedEntity, mappingSelectionType };
        }

        public string GetCustomDocumentTitle()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.Original)
                return MainViewModel.SelectedEntity.NAME + " - " + MainViewModel.SelectedEntity.P6BASELINE_NAME + " Mapping";
            else if (mappingSelectionType == BaselineMappingSelectionType.Modified)
                return MainViewModel.SelectedEntity.NAME + " - " + MainViewModel.SelectedEntity.P6MODBASELINE_NAME + " Mapping";
            else
                return "[" + loadPROJECT.NUMBER + "] BASELINE";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}
