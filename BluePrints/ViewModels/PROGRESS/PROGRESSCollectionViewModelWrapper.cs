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
using System;
using System.Collections.Generic;
using System.Linq;
using PROJECT = BluePrints.Data.PROJECT;

namespace BluePrints.ViewModels
{
    public class PROGRESSCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>
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
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(PROGRESS entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.PROGRESS_START = entity.PROGRESS_START.Date;
            entity.DATA_DATE = entity.DATA_DATE.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
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
            //get { return "PROGRESSCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "PROGRESSCollectionViewModelWrapper_v1" + view_project_specific_affix; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
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

            DocumentInfo DocumentInfo;
            if(DisplaySelectedEntity.TYPE == ProgressType.Design)
                DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, PROGRESS>(null, DisplaySelectedEntity), "OffsiteDirectProgressCollectionView", "[" + loadPROJECT.NUMBER + "] Progress");
            else
                DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, PROGRESS>(null, DisplaySelectedEntity), "SiteDirectProgressCollectionView", "[" + loadPROJECT.NUMBER + "] Progress");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        #endregion
    }
}