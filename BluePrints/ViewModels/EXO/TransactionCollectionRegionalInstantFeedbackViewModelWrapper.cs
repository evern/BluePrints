using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class TransactionCollectionRegionalInstantFeedbackViewModelWrapper : BluePrintsEntitiesCollectionWrapper<X_JOB_TRANSACTIONS_DETAIL_V4, X_JOB_TRANSACTIONS_DETAIL_V4, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TransactionCollectionRegionalInstantFeedbackViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new TransactionCollectionRegionalInstantFeedbackViewModelWrapper());
        }

        public bool CanEditWithoutApproval { get; set; }
        public bool IsCostsVisible { get; set; }
        public bool CanEditQuantity { get; set; }
        protected override string readOnlyMessage => "Cells are read only because you do not have authority to edit transactions";
        protected TransactionCollectionRegionalInstantFeedbackViewModelWrapper()
        {
            IsReadOnly = true;
            IsCostsVisible = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_ShowCosts)) == LoginCredentials.PermissionStatus.All;
            CanEditQuantity = !IsReadOnly && LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_ChangeQuantity)) == LoginCredentials.PermissionStatus.All;
            CanEditWithoutApproval = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_RequiresApproval)) != LoginCredentials.PermissionStatus.All;

            isUsePreloadMode = false;
            IsInstantFeedbackMode = true;

            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(bluePrintsUnitOfWork);
        }

        public bool IsShowDefaultColumns => !IsInstantFeedbackMode || IsReadOnly;
        public string BandHeaderName => IsYearToDate ? "WBS" : IsReadOnly ? "Editable When Authorised" : "Editable";

        bool isUsePreloadMode;
        public bool IsUsePreloadMode
        {
            get => isUsePreloadMode;
        }

        #region Database Operation
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory;
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        //main view model uses a different unit of work factory because there are derived field from view that cannot be saved, instead we manually map properties before saving on PrimeroUnitOfWork
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> mainViewModelPrimeroUnitOfWorkFactory;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        private IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        JOBCOST_HDR loadJOBCOST_HDR;
        bool isYearToDate = false;
        bool is2021Onwards = false;
        public bool IsYearToDate => isYearToDate;
        public bool Is2021Onwards => is2021Onwards;
        public int DateSortIndex => 1;
        public string officeName;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (DualEntitiesParameter<object, object>)parameter;
            IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> tempPrimeroUnitOfWorkFactory;
            IsReadOnly = true;
            isYearToDate = true;
            DatabaseLocale dbLocale = (DatabaseLocale)PROJECTParameter.GetFirstEntity();
            is2021Onwards = (bool)PROJECTParameter.GetSecondEntity();
            isUsePreloadMode = false;
            IsInstantFeedbackMode = true;

            if (dbLocale == DatabaseLocale.Perth)
                officeName = BluePrintsResources.OfficePerth;
            else if (dbLocale == DatabaseLocale.Montreal)
                officeName = BluePrintsResources.OfficeMontreal;
            else
                officeName = BluePrintsResources.OfficeUSA;

            mainViewModelPrimeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(officeName);
            tempPrimeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(officeName);
            primeroUnitOfWork = tempPrimeroUnitOfWorkFactory.CreateUnitOfWork();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(primeroUnitOfWork);
        }

        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.STOCK_ITEMS, STOCK_ITEMSProjectionFunc);
            loaderCollection.AddLoaderDescription<GLACCS, GLACCS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.GLACCS);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc, x => loadJOBCOST_HDR = x);
            loaderCollection.AddLoaderDescription<STOCK_GROUPS, STOCK_GROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUPS);
            loaderCollection.AddLoaderDescription<STOCK_GROUP2S, STOCK_GROUP2S, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_GROUP2S);
        }

        private Func<IRepositoryQuery<STOCK_ITEMS>, IQueryable<STOCK_ITEMS>> STOCK_ITEMSProjectionFunc()
        {
            return query => query;
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query;
        }

        public ObservableCollection<JOB_TRANSACTIONS> JOB_TRANSACTIONS = new ObservableCollection<JOB_TRANSACTIONS>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(mainViewModelPrimeroUnitOfWorkFactory, x => x.X_JOB_TRANSACTIONS_DETAIL_V4);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            IsPasteCellLevel = true;
        }

        protected override Func<IRepositoryQuery<X_JOB_TRANSACTIONS_DETAIL_V4>, IQueryable<X_JOB_TRANSACTIONS_DETAIL_V4>> specifyMainViewModelProjection()
        {
            if (isYearToDate)
            {
                if (Is2021Onwards)
                {
                    DateTime date2021FirstDay = new DateTime(2021, 1, 1);
                    return query => query.Where(x => x.TRANSDATE != null && ((DateTime)x.TRANSDATE) >= date2021FirstDay);
                }
                else
                    return query => query.Where(x => x.TRANSDATE != null);
            }
            else
                return query => query.Where(x => x.MASTER_JOBNO == loadJOBCOST_HDR.MASTER_JOBNO);
        }

        public override void FullRefresh()
        {
            base.FullRefresh();
            GridControlService.RefreshData();
        }
#endregion

#region View Properties
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "InstantTransactionEntryViewModelWrapper_v1"; }
        }

        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
        }
        #endregion

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, X_JOB_TRANSACTIONS_DETAIL_V4 projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new X_JOB_TRANSACTIONS_DETAIL_V4().QUANTITY))
                projection.QtyEdited = true;

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedRowValidation(X_JOB_TRANSACTIONS_DETAIL_V4 projection)
        {
            return string.Empty;
        }

        public override bool CanKeyboardPaste()
        {
            if (IsReadOnly)
                return false;

            return base.CanKeyboardPaste();
        }

        public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            if (IsReadOnly)
                return;

            base.PastingFromClipboard(e);
        }

        private InstantFeedbackCollectionViewModel<JOBCOST_HDR, int, IPrimeroEntitiesUnitOfWork> JOBCOST_HDRInstantFeedbackCollectionViewModel;
        public InstantFeedbackCollectionViewModel<JOBCOST_HDR, int, IPrimeroEntitiesUnitOfWork>.InstantFeedbackSourceViewModel JOBCOST_HDRInstantFeedbackCollection
        {
            get
            {
                if (!isYearToDate && loadJOBCOST_HDR == null)
                    return null;

                if(JOBCOST_HDRInstantFeedbackCollectionViewModel == null)
                    JOBCOST_HDRInstantFeedbackCollectionViewModel = InstantFeedbackCollectionViewModel<JOBCOST_HDR, int, IPrimeroEntitiesUnitOfWork>.CreateInstantFeedbackCollectionViewModel(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjection);

                return JOBCOST_HDRInstantFeedbackCollectionViewModel.Entities;
            }
        }

        private List<JOBCOST_HDR> JOBCOST_HDRList;
        public List<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                if (JOBCOST_HDRList == null && primeroUnitOfWork != null && loadJOBCOST_HDR != null)
                    JOBCOST_HDRList = JOBCOST_HDRProjection(primeroUnitOfWork.JOBCOST_HDR).ToList();

                return JOBCOST_HDRList;
            }
        }

        protected IQueryable<JOBCOST_HDR> JOBCOST_HDRProjection(IRepositoryQuery<JOBCOST_HDR> query)
        {
            if (isYearToDate)
                return query;
            else
                return query.Where(x => x.MASTER_JOBNO == loadJOBCOST_HDR.MASTER_JOBNO);
        }

        public IEnumerable<GLACCS> GLACCSCollection
        {
            get
            {
                return GetEntities<GLACCS>();
            }
        }

        public IEnumerable<JOBCOST_RESOURCE> JOBCOST_RESOURCECollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_RESOURCE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.RESOURCENAME);
                return collection;
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_ITEMS> STOCK_ITEMCollection
        {
            get
            {
                var collection = GetEntities<STOCK_ITEMS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.STOCKCODE);
                return collection;
            }
        }

        public IEnumerable<X_JOB_TRANSACTIONS_DETAIL_V4> X_JOB_TRANSACTIONS_DETAILCollection
        {
            get
            {
                return GetEntities<X_JOB_TRANSACTIONS_DETAIL_V4>();
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUP2S> STOCK_GROUP2SCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP2S>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GROUPNO);
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUPS> STOCK_GROUPSCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.GROUPNO);
                return collection;
            }
        }

        public CollectionViewModel<TRANSACTION_APPROVAL, TRANSACTION_APPROVAL, Guid, IBluePrintsEntitiesUnitOfWork> TRANSACTION_APPROVALViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<TRANSACTION_APPROVAL, TRANSACTION_APPROVAL, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<TRANSACTION_APPROVAL>();
            }
        }
        public override void CleanUpEntitiesLoader()
        {
            JOBCOST_HDRInstantFeedbackCollectionViewModel?.Dispose();
            base.CleanUpEntitiesLoader();
        }

        public override string UnifiedValueValidation(X_JOB_TRANSACTIONS_DETAIL_V4 projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public string SubJobFieldName => CanEditWithoutApproval ? "JOBNO" : IsInstantFeedbackMode ? "ProxyJobNo" : "JobNoChangeTracking.TrackableProperty";
        public string CostGroupFieldName => CanEditWithoutApproval ? "COST_GROUP_NO" : IsInstantFeedbackMode ? "ProxyCostGroup" : "CostGroupChangeTracking.TrackableProperty";
        public string CostTypeFieldName => CanEditWithoutApproval ? "COST_TYPE_NO" : IsInstantFeedbackMode ? "ProxyCostType" : "CostTypeChangeTracking.TrackableProperty";
        public string StockCodeFieldName => CanEditWithoutApproval ? "STOCKCODE" : IsInstantFeedbackMode ? "ProxyStockCode" : "StockCodeChangeTracking.TrackableProperty";
        public string VariationCodeFieldName => CanEditWithoutApproval ? "VARIATION_CODE" : IsInstantFeedbackMode ? "ProxyVariationCode" : "VariationCodeChangeTracking.TrackableProperty";
        public bool IsEditableDescriptionColumnVisible => CanEditWithoutApproval;
    }
}

