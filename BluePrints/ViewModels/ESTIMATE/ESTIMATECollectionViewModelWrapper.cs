using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class ESTIMATECollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<ESTIMATE, ESTIMATE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of ESTIMATE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private Data.PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES);
        }

        protected override Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>>
            specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(ESTIMATE projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "ESTIMATECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "ESTIMATECollectionViewModelWrapper_v1" + view_project_specific_affix; }
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
            if (SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            EstimateViewMode estimateViewMode = SelectedEntity.STATUS == BaselineStatus.Working ? EstimateViewMode.Estimate : EstimateViewMode.Budget;
            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(null, SelectedEntity, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Both, estimateViewMode)), "ESTIMATE_ITEMCollectionView", "[" + loadPROJECT.NUMBER + "] Direct Estimate");
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public bool CanP6BASELINE_ASSIGN()
        {
            return SelectedEntity != null && SelectedEntity.P6BASELINE_NAME != null &&
                   SelectedEntity.P6BASELINE_NAME != string.Empty;
        }

        public void P6BASELINE_ASSIGN()
        {
            string viewName;
            if (loadPROJECT.USE_WORKPACKS)
                viewName = "BUDGET_ITEMWorkpackSchedulingView";
            else
                viewName = "BUDGET_ITEMSchedulingView";

            string tabName = SelectedEntity.NAME + " - " + SelectedEntity.P6BASELINE_NAME + " Mapping";
            DocumentInfo DocumentInfo = new DocumentInfo(tabName, new object[] { SelectedEntity, BaselineMappingSelectionType.Original, loadPROJECT, true }, viewName, tabName);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        ESTIMATE_ITEMCollectionViewModelWrapper estimate_itemViewModelWrapper;

        public bool IsApproving { get; set; }
        public bool CanApprove()
        {
            return !IsApproving && SelectedEntity != null;
        }


        public void Approve()
        {
            if (SelectedEntity.STATUS == BaselineStatus.Live)
            {
                MessageBoxService.ShowMessage("Cannot approve live estimate");
                return;
            }

            IsApproving = true;
            this.RaisePropertyChanged(x => x.IsApproving);
            estimate_itemViewModelWrapper = ESTIMATE_ITEMCollectionViewModelWrapper.Create();
            estimate_itemViewModelWrapper.SetParentViewModel(this);
            estimate_itemViewModelWrapper.SuppressNotification = true;
            estimate_itemViewModelWrapper.SupressCompulsoryEntityNotFoundMessage = true;
            estimate_itemViewModelWrapper.OnEntitiesLoadedCallBackManualDispose = true;
            estimate_itemViewModelWrapper.OnEntitiesLoadedCallBack = onEstimateItemsLoaded;
            estimate_itemViewModelWrapper.OnEntitiesLoadedCallBackRelateParam = () => SelectedEntity.GUID;
            ISupportParameter receiveParameterViewModel = estimate_itemViewModelWrapper as ISupportParameter;
            EstimateViewMode estimateViewMode = SelectedEntity.STATUS == BaselineStatus.Working ? EstimateViewMode.Estimate : EstimateViewMode.Budget;
            receiveParameterViewModel.Parameter = new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(null, SelectedEntity, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Both, estimateViewMode));
        }

        private void onEstimateItemsLoaded(IEnumerable<ESTIMATE_ITEMProgress> projections, object parentId)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => reviseEstimate(projections, parentId)));
        }

        private void reviseEstimate(IEnumerable<ESTIMATE_ITEMProgress> projections, object parentId)
        {
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            List<ESTIMATE_ITEMProgress> estimateItemsToSave = new List<ESTIMATE_ITEMProgress>();
            LoadingScreenManager.ShowLoadingScreen(projections.Count());

            foreach (ESTIMATE_ITEMProgress projection in projections)
            {
                projection.Entity.Entity.BUDGET_QUANTITY = projection.Entity.Entity.ESTIMATE_QUANTITY;
                projection.Entity.Entity.BUDGET_INSTALL_RATE = projection.Entity.Entity.ESTIMATE_INSTALL_RATE;
                projection.Entity.Entity.BUDGET_TRUCK_PERCENTAGE = projection.Entity.Entity.ESTIMATE_TRUCK_PERCENTAGE;

                STOCK_CODE estimate_stock_code = bluePrintsUOW.STOCK_CODES.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_ESTIMATE_STOCK_CODE);
                if(estimate_stock_code != null)
                {
                    STOCK_CODE budget_stock_code = new STOCK_CODE();
                    DataUtils.ShallowCopy(budget_stock_code, estimate_stock_code);
                    budget_stock_code.GUID = Guid.Empty;
                    budget_stock_code.STOCK_CODE_TYPE = StockCodeType.Budget;
                    budget_stock_code.GUID_ORIGINAL = estimate_stock_code.GUID;
                    bluePrintsUOW.STOCK_CODES.Add(budget_stock_code);
                    bluePrintsUOW.SaveChanges();
                    projection.Entity.Entity.GUID_BUDGET_STOCK_CODE = budget_stock_code.GUID;
                }

                estimateItemsToSave.Add(projection);

                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            estimate_itemViewModelWrapper.BulkSave(estimateItemsToSave);
            estimate_itemViewModelWrapper.CleanUpEntitiesLoader();

            ESTIMATE estimate = this.Entities.FirstOrDefault(x => x.GUID.ToString() == parentId.ToString());
            estimate.STATUS = BaselineStatus.Live;
            MainViewModel.Save(estimate);
            MessageBoxService.ShowMessage(estimate.NAME + " approved");
            IsApproving = false;
            this.RaisePropertyChanged(x => x.IsApproving);
        }

        public override string UnifiedRowValidation(ESTIMATE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(ESTIMATE projection, string field_name, object new_value, bool isPaste)
        {
            //if (field_name == BindableBase.GetPropertyName(() => new ESTIMATE().STATUS))
            //{
            //    if(new_value != null)
            //    {
            //        object oldValue = DataUtils.GetNestedValue(field_name, projection);
            //        if (oldValue != null)
            //        {
            //            BaselineStatus oldStatus = (BaselineStatus)oldValue;
            //            BaselineStatus newStatus = (BaselineStatus)new_value;

            //            if ((oldStatus == BaselineStatus.Working || oldStatus == BaselineStatus.Superseded) && newStatus == BaselineStatus.Live)
            //                return "Please use the approve button to move estimate from working to live";
            //            //else if (oldStatus == BaselineStatus.Live)
            //            //    return "Cannot change status once it is live";
            //        }
            //    }
            //}

            return string.Empty;
        }
        #endregion
    }
}