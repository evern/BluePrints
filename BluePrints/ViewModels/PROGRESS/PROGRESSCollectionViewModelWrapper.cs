using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.DataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Common.Helpers;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using PROJECT = BluePrints.Data.PROJECT;

namespace BluePrints.ViewModels
{
    public class PROGRESSCollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>>,
        ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of PROGRESSCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROGRESSCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROGRESSCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the PROGRESSCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROGRESSCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROGRESSCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0,
                bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT,
                OnAfterEntitiesChanged);
            loaderCollection
                .AddEntitiesLoader<PROJWBS, PROJWBS, int, IP6EntitiesUnitOfWork>(1,
                    p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
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

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y").OrderBy(proj => proj.wbs_short_name);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS> entities)
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

            if (loadPROJECT != null && changedType == typeof(PROJECT) &&
                loadPROJECT.GUID.ToString() == key.ToString())
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored,
                        StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                        StringFormatUtils.GetEntityNameByType(changedType)));

            if (loadPROJECT != null)
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));

            base.OnAfterEntitiesChanged(key, changedType, messageType, sender);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(PROGRESS entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.PROGRESS_START = entity.PROGRESS_START.Date;
            entity.DATA_DATE = entity.DATA_DATE.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        #endregion

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROGRESSCollectionViewModelWrapper"; }
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

        public bool CanEdit(PROGRESS entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit(PROGRESS entity)
        {
            if (entity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<PROGRESS_ITEM, Guid>(this, entity.GUID, string.Empty);
        }

        public string GetCustomDocumentTypeName()
        {
            return "PROGRESS_ITEMCollectionView";
        }

        public object GetCustomDocumentParameter()
        {
            return new OptionalEntitiesParameter<PROJECT, PROGRESS>(null, MainViewModel.SelectedEntity);
        }

        public string GetCustomDocumentTitle()
        {
            return "[" + loadPROJECT.NUMBER + "] PROGRESS";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }

        #endregion
    }
}