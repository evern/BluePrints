using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class STOCK_CODECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <STOCK_CODE, STOCK_CODE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of STOCK_CODECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static STOCK_CODECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new STOCK_CODECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the STOCK_CODECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the STOCK_CODECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected STOCK_CODECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        private bool isProjectSpecific
        {
            get { return loadPROJECT != null; }
        }

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            if (parameter != null)
            {
                var projectCodeTypeParameter = (DualEntitiesParameter<PROJECT, StockCodeTypeClass>)parameter;
                loadPROJECT = projectCodeTypeParameter.GetFirstEntity();
            }
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            if (isProjectSpecific)
                return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
            else
                return query => query.Where(x => x.GUID_PROJECT == null);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> specifyMainViewModelProjection()
        {
            if (isProjectSpecific)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
            else
                return query => query.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<STOCK_CODE> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(STOCK_CODE entity)
        {
            if(isProjectSpecific)
                entity.GUID_PROJECT = loadPROJECT.GUID;

            //new item is not allowed, so stock code type will be whatever it was previously
            //entity.STOCK_CODE_TYPE = loadCommodityCodeType;
            return true;
        }

        #endregion

        #endregion

        #region View Commands
        public void Trim_Unused()
        {
            create_estimation_direct_item_view_model_wrapper();
        }

        private void create_estimation_direct_item_view_model_wrapper()
        {
            ESTIMATE_ITEMCollectionViewModelWrapper variation_itemsViewModelWrapper = ESTIMATE_ITEMCollectionViewModelWrapper.Create();
            variation_itemsViewModelWrapper.SetParentViewModel(this);
            variation_itemsViewModelWrapper.OnEntitiesLoadedCallBack = trim_unused_stock_code;
            var baselineSupportParameterObj = variation_itemsViewModelWrapper as ISupportParameter;
            baselineSupportParameterObj.Parameter = new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(loadPROJECT, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Both, EstimateViewMode.Both));
        }

        private void trim_unused_stock_code(IEnumerable<ESTIMATE_ITEMProgress> estimation_direct_items, object parentId)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => main_thread_trim_unused_stock_code(estimation_direct_items, parentId)));
        }

        private void main_thread_trim_unused_stock_code(IEnumerable<ESTIMATE_ITEMProgress> estimation_direct_items, object parentId)
        {
            List<STOCK_CODE> removeStockCodes = new List<STOCK_CODE>();
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            foreach (STOCK_CODE projectStockCode in MainViewModel.Entities)
            {
                if (!estimation_direct_items.Any(x => x.Entity.Entity.GUID_ESTIMATE_STOCK_CODE == projectStockCode.GUID || x.Entity.Entity.GUID_BUDGET_STOCK_CODE == projectStockCode.GUID))
                {
                    removeStockCodes.Add(projectStockCode);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectStockCode, null, null, null, EntityMessageType.Deleted);
                }
            }
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            MainViewModel.BaseBulkDelete(removeStockCodes);
        }

        public override string UnifiedRowValidation(STOCK_CODE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(STOCK_CODE projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "STOCK_CODECollectionViewModelWrapper"; }
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
        #endregion
    }
}