using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class COMMODITY_CODECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <COMMODITY_CODE, COMMODITY_CODEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public bool IsProjectSpecific
        {
            get { return loadPROJECT != null; }
        }

        protected override void resolveParameters(object parameter)
        {
            if (parameter != null)
            {
                var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
                loadPROJECT = PROJECTParameter.GetEntity();
            }
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            //need to add another viewmodel so that all stock codes are loaded for stock codes generation
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            if (IsProjectSpecific)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            else
                //not necessary to load anything when it's global commodity code
                return query => query.Where(x => x.GUID_PROJECT == Guid.Empty);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            if (IsProjectSpecific)
                return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID));
            else
                return query => query.Where(x => x.GUID == null);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODEProjection>> specifyMainViewModelProjection()
        {
            if(IsProjectSpecific)
                return query => COMMODITY_CODEProjectionQueries.COMMODITY_CODE_Transformation(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
            else
                return query => COMMODITY_CODEProjectionQueries.COMMODITY_CODE_Transformation(query.Where(x => x.GUID_PROJECT == null));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_CODEProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(COMMODITY_CODEProjection entity)
        {
            if (IsProjectSpecific)
                entity.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, COMMODITY_CODEProjection projection, bool isNew)
        {
            if(isNew && field_name.Contains(BindableBase.GetPropertyName(() => new COMMODITY_CODEProjection().Entity.CODE)))
            {
                if (projection.Entity.DEFAULT_STOCKCODE == string.Empty)
                    projection.Entity.DEFAULT_STOCKCODE = projection.Entity.CODE;
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedRowValidation(COMMODITY_CODEProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(COMMODITY_CODEProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }
        #endregion

        #endregion

        #region View Commands

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "COMMODITY_CODECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "COMMODITY_CODECollectionViewModelWrapper_v1" + view_project_specific_affix; }
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

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
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

        public IEnumerable<UOM> UOMCollection
        {
            get
            {
                var collection = GetEntities<UOM>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.UOM1);
                return collection;
            }
        }
       
        public IEnumerable<COMMODITY_CODE> GlobalCOMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITY_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<COMMODITY_CODE>();
            }
        }
        #endregion
    }
}