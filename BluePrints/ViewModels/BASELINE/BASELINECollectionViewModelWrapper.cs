using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class BASELINECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>
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
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }
        
        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc
            ()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderByDescending(x => x.REVISION);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(BASELINE entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
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

        #region ISupportCustomDocumentTypeAndParameter

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(),
                new DualEntitiesParameter<Data.PROJECT, BASELINE>(null,
                    DisplaySelectedEntity), 
                    "BASELINE_ITEMCollectionView", 
                    "[" + loadPROJECT.NUMBER + "] Baseline");

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
        }

        public bool CanP6BASELINE_ASSIGN()
        {
            return DisplaySelectedEntity != null && DisplaySelectedEntity.P6BASELINE_NAME != null &&
                   DisplaySelectedEntity.P6BASELINE_NAME != string.Empty;
        }

        public void P6BASELINE_ASSIGN()
        {
            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(),
                new object[] { DisplaySelectedEntity, BaselineMappingSelectionType.Original },
                "BASELINE_ITEMSchedulingView", 
                DisplaySelectedEntity.NAME + " - " + DisplaySelectedEntity.P6BASELINE_NAME + " Mapping");

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
        }

        public bool CanP6MODBASELINE_ASSIGN()
        {
            return DisplaySelectedEntity != null && DisplaySelectedEntity.P6MODBASELINE_NAME != null &&
                   DisplaySelectedEntity.P6MODBASELINE_NAME != string.Empty;
        }

        public void P6MODBASELINE_ASSIGN()
        {
            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(),
                new object[] { DisplaySelectedEntity, BaselineMappingSelectionType.Modified },
                "BASELINE_ITEMSchedulingView",
                DisplaySelectedEntity.NAME + " - " + DisplaySelectedEntity.P6MODBASELINE_NAME + " Mapping");

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
        }
        #endregion
    }
}