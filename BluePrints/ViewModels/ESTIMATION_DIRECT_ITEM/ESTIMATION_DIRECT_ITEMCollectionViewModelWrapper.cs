using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of ESTIMATION_DIRECT_ITEM_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT loadPROJECT;
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        private DEPARTMENT defaultConstructionDEPARTMENT;
        private bool isQueryForLiveStatus;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var receiveParameter =
                (OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadESTIMATION_DIRECT = receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => loadESTIMATION_DIRECT = x);
            loaderCollection.AddLoaderDescription<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, x => defaultConstructionDEPARTMENT = x);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == EstimationStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.SiteDirect);
        }

        private Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == BluePrintsResources.DefaultConstructionDepartment);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID && x.GUID == defaultConstructionDEPARTMENT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(query, loadESTIMATION_DIRECT, loaderCollection.GetCollection<RATE>());
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            //MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingRowAddUndoAndSaveCallBack;
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected bool ExistingRowAddUndoAndSaveCallBack(ESTIMATION_DIRECT_ITEMProjection projectionEntity, CellValueChangedEventArgs e)
        {
            return true;
        }

        private Guid? createProjectSpecificCOMMODITY_CODE(Guid commodity_code_guid)
        {
            if(!ProjectCOMMODITY_CODECollection.Any(x => x.GUID == commodity_code_guid))
            {
                COMMODITY_CODE commodity_code = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == commodity_code_guid);
                if (commodity_code != null)
                {
                    COMMODITY_CODE projectCOMMODITY_CODE = new COMMODITY_CODE();
                    DataUtils.ShallowCopy(projectCOMMODITY_CODE, commodity_code);
                    projectCOMMODITY_CODE.GUID = Guid.Empty;
                    projectCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
                    COMMODITY_CODESCollectionViewModel.Save(projectCOMMODITY_CODE);
                    return projectCOMMODITY_CODE.GUID;
                }
                else
                    return null;
            }

            return null;
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(ESTIMATION_DIRECT_ITEMProjection entity)
        {
            if(entity.Entity.GUID_COMMODITY_CODE != null)
            {
                Guid? projectCommodity_CodeGuid = createProjectSpecificCOMMODITY_CODE((Guid)entity.Entity.GUID_COMMODITY_CODE);
                if (projectCommodity_CodeGuid != null)
                    entity.Entity.GUID_COMMODITY_CODE = projectCommodity_CodeGuid;
            }

            entity.Entity.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;
        }
        #endregion
        #endregion

        #region View Interactions
        /// <summary>
        /// Allow undo-redo behavior to be added for automated cell value changing. This behavior doesn't have to be applied on new row because AddUndo for EntityMessageType.Added is already handling this
        /// </summary>
        protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_AREA))
            {
                var activeESTIMATION_DIRECT_ITEM = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
                Guid? oldValue = activeESTIMATION_DIRECT_ITEM.Entity.GUID_SUBAREA;
                if (e.Value != null && oldValue != null)
                {
                    Guid? newValue = (Guid?)null;
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().SubAreaGuid);
                    activeESTIMATION_DIRECT_ITEM.Entity.GUID_SUBAREA = newValue;
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(activeESTIMATION_DIRECT_ITEM, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
            }

            base.CellValueExistingRowChanging(e);
        }

        protected override void CellValueNewRowChanging(CellValueChangedEventArgs e)
        {
            var activeESTIMATION_DIRECT_ITEM = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
            if (e.Column.FieldName ==
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_AREA))
            {
                if (e.Value != null)
                {
                    activeESTIMATION_DIRECT_ITEM.Entity.GUID_AREA = (Guid)e.Value;
                    //Area is required immediately for subarea selection
                    activeESTIMATION_DIRECT_ITEM.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
                    activeESTIMATION_DIRECT_ITEM.Update();
                }

                //SubArea must be removed immediately to nullify subarea selection
                if (e.Value != null && activeESTIMATION_DIRECT_ITEM.Entity.GUID_SUBAREA != null)
                {
                    activeESTIMATION_DIRECT_ITEM.Entity.GUID_SUBAREA = null;
                    activeESTIMATION_DIRECT_ITEM.Update();
                }
            }

            base.CellValueNewRowChanging(e);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper"; }
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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            }
        }

        public CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>();
            }
        }

        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITY_CODESCollectionViewModel
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