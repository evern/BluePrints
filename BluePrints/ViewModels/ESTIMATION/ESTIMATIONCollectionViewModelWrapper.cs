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
    public class ESTIMATIONCollectionViewModelWrapper : CollectionViewModelsWrapper<ESTIMATION, ESTIMATION, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<ESTIMATION, ESTIMATION, Guid, IBluePrintsEntitiesUnitOfWork>>, ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of ESTIMATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATIONCollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATIONCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATIONCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
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

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.ESTIMATIONS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION>, IQueryable<ESTIMATION>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION> entities)
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
        public void OnBeforeEntitySaved(ESTIMATION entity)
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
                return "ESTIMATIONCollectionViewModelWrapper";
            }
        }

        public IEnumerable<BluePrints.P6Data.PROJWBS> P6PROJECTSCollection
        {
            get
            {
                return GetEntities<BluePrints.P6Data.PROJWBS>();
            }
        }
        #endregion

        #region ISupportCustomDocumentTypeNameAndParameter
        public bool CanEdit(ESTIMATION entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService { get { return this.GetService<IDocumentManagerService>(); } }
        public void Edit(ESTIMATION entity)
        {
            if (entity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<ESTIMATION_ITEM, Guid>(this, entity.GUID, string.Empty);
        }

        BaselineMappingSelectionType mappingSelectionType = new BaselineMappingSelectionType();
        public void P6ESTIMATION_ASSIGN()
        {
            mappingSelectionType = BaselineMappingSelectionType.Original;
            Edit(MainViewModel.SelectedEntity);
            mappingSelectionType = BaselineMappingSelectionType.None;
        }

        public void P6MODESTIMATION_ASSIGN()
        {
            mappingSelectionType = BaselineMappingSelectionType.Modified;
            Edit(MainViewModel.SelectedEntity);
            mappingSelectionType = BaselineMappingSelectionType.None;
        }

        public string GetCustomDocumentTypeName()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.None)
                return "ESTIMATION_ITEMCollectionView";

            return "PROJECTWORKPACKDetailsMappingViewHost";
        }

        public object GetCustomDocumentParameter()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.None)
                return new OptionalEntitiesParameter<BluePrints.Data.PROJECT, ESTIMATION>(null, MainViewModel.SelectedEntity);

            return new object[] { MainViewModel.SelectedEntity, mappingSelectionType };
        }

        public string GetCustomDocumentTitle()
        {
            if (mappingSelectionType == BaselineMappingSelectionType.Original)
                return MainViewModel.SelectedEntity.NAME + " - " + MainViewModel.SelectedEntity.P6ESTIMATION_NAME + " Mapping";
            else if (mappingSelectionType == BaselineMappingSelectionType.Modified)
                return MainViewModel.SelectedEntity.NAME + " - " + MainViewModel.SelectedEntity.P6MODESTIMATION_NAME + " Mapping";
            else
                return "[" + loadPROJECT.NUMBER + "] ESTIMATION";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}
