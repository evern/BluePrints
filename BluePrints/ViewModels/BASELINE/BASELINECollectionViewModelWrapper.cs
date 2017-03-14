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
    public class BASELINECollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>>,
        ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of BASELINECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private Data.PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter =
                (EntitiesParameter<Data.PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection
                .AddEntitiesLoader(0,
                    bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null,
                    isContinueLoadingAfterPROJECT, null, OnAfterEntitiesChanged, null, true);
            loaderCollection
                .AddEntitiesLoader(1,
                    p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
            //loaderCollection.AddEntitiesLoader<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, WORKPACK_ASSIGNMENTProjectionFunc, typeof(BluePrints.Data.PROJECT));
            //loaderCollection.AddEntitiesLoader<TASKRSRC, TASKRSRC, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, TASKRSRCProjectionFunc, typeof(BluePrints.P6Data.PROJECT));
            //loaderCollection.AddEntitiesLoader<TASK, TASK, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, TASKProjectionFunc, typeof(BluePrints.P6Data.PROJECT));
            InvokeEntitiesLoaderDescriptionLoading();
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

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc
            ()
        {
            return query => query.Where(x => x.proj_node_flag == "Y").OrderBy(proj => proj.wbs_short_name);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE> entities)
        {
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            if (sender.ToString() == MainViewModel.ToString())
                return;

            if (loadPROJECT != null && changedType == typeof(Data.PROJECT) &&
                loadPROJECT.GUID.ToString() == key.ToString())
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored,
                        StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                        StringFormatUtils.GetEntityNameByType(changedType)));

            //if (loadPROJECT != null)
            //    if (MainViewModel != null)
            //        mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
            //    else if (loadPROJECT != null)
            //        mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));

            base.OnAfterEntitiesChanged(key, changedType, messageType, sender);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(BASELINE entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
        }

        #endregion

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "BASELINECollectionViewModelWrapper"; }
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

        #endregion

        #region ISupportCustomDocumentTypeNameAndParameter

        public bool CanEdit(BASELINE entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit(BASELINE entity)
        {
            if (entity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<BASELINE_ITEM, Guid>(this, entity.GUID, string.Empty);
        }

        private BaselineMappingSelectionType mappingSelectionType = new BaselineMappingSelectionType();

        public bool CanP6BASELINE_ASSIGN(BASELINE assignEntity)
        {
            return assignEntity != null && assignEntity.P6BASELINE_NAME != null &&
                   assignEntity.P6BASELINE_NAME != string.Empty;
        }

        public void P6BASELINE_ASSIGN(BASELINE assignEntity)
        {
            mappingSelectionType = BaselineMappingSelectionType.Original;
            Edit(assignEntity);
            mappingSelectionType = BaselineMappingSelectionType.None;
        }

        public bool CanP6MODBASELINE_ASSIGN(BASELINE assignEntity)
        {
            return assignEntity != null && assignEntity.P6MODBASELINE_NAME != null &&
                   assignEntity.P6MODBASELINE_NAME != string.Empty;
        }

        public void P6MODBASELINE_ASSIGN(BASELINE assignEntity)
        {
            mappingSelectionType = BaselineMappingSelectionType.Modified;
            Edit(assignEntity);
            mappingSelectionType = BaselineMappingSelectionType.None;
        }

        public string GetCustomDocumentTypeName()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.None)
                return "BASELINE_ITEMCollectionView";

            return "WORKPACKSchedulingViewHost";
        }

        public object GetCustomDocumentParameter()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.None)
                return new OptionalEntitiesParameter<Data.PROJECT, BASELINE>(null,
                    MainViewModel.SelectedEntity);

            return new object[] {MainViewModel.SelectedEntity, mappingSelectionType};
        }

        public string GetCustomDocumentTitle()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.Original)
                return MainViewModel.SelectedEntity.NAME + " - " + MainViewModel.SelectedEntity.P6BASELINE_NAME +
                       " Mapping";
            else if (mappingSelectionType == BaselineMappingSelectionType.Modified)
                return MainViewModel.SelectedEntity.NAME + " - " + MainViewModel.SelectedEntity.P6MODBASELINE_NAME +
                       " Mapping";
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