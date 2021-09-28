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
using BluePrints.Common.ViewModel.Utils;
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
    public partial class TransactionCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<X_JOB_TRANSACTIONS_DETAIL_V3, X_JOB_TRANSACTIONS_DETAIL_V3, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TransactionCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new TransactionCollectionViewModelWrapper());
        }

        public bool IsCostsVisible { get; set; }
        public bool CanEditQuantity { get; set; }
        protected override string readOnlyMessage => "Cells are read only because you do not have authority to edit transactions";
        protected TransactionCollectionViewModelWrapper()
        {
            IsReadOnly = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Transactions)) == LoginCredentials.PermissionStatus.ReadOnly;
            IsCostsVisible = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_ShowCosts)) == LoginCredentials.PermissionStatus.All;
            CanEditQuantity = !IsReadOnly && LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_ChangeQuantity)) == LoginCredentials.PermissionStatus.All;

            bool? isUsePreloadModePreference = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.EXO_PreloadTransactions));
            isUsePreloadMode = isUsePreloadModePreference == null ? false : (bool)isUsePreloadModePreference;
            IsInstantFeedbackMode = !IsUsePreloadMode;
        }

        public bool IsShowDefaultColumns => !IsInstantFeedbackMode || IsReadOnly;
        public string BandHeaderName => IsYearToDate ? "WBS" : IsReadOnly ? "Editable When Authorised" : "Editable";

        bool isUsePreloadMode;
        public bool IsUsePreloadMode
        {
            get => isUsePreloadMode;
            set
            {
                isUsePreloadMode = value;
                BluePrintsDataUtils.SaveUserPreference(DataUtils.GetNameOf(() => UserPreferences.EXO_PreloadTransactions), value ? UserPreferences.PreferenceTrueValue : UserPreferences.PreferenceFalseValue);
                string uniqueNavKeyFormat = DataUtils.FormatNavigationKey(loadPROJECT.GUID.ToString());

                Messenger.Default.Send(new NavigateMessage(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Transactions) + uniqueNavKeyFormat));
            }
        }

        #region Database Operation
        private Data.PROJECT loadPROJECT;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        JOBCOST_HDR loadJOBCOST_HDR;
        bool isYearToDate = false;
        bool is2020Onwards = false;
        public bool IsYearToDate => isYearToDate;
        public bool Is2020Onwards => is2020Onwards;
        public int DateSortIndex => 1;
        public string officeName;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (TripleEntitiesParameter<Data.PROJECT, object, object>)parameter;
            loadPROJECT = PROJECTParameter.GetFirstEntity();

            if(loadPROJECT == null)
            {
                IsReadOnly = true;
                isYearToDate = true;
                is2020Onwards = (bool)PROJECTParameter.GetSecondEntity();
                isUsePreloadMode = false;

                DatabaseLocale dbLocale = (DatabaseLocale)PROJECTParameter.GetThirdEntity();
                if (dbLocale == DatabaseLocale.Perth)
                    officeName = BluePrintsResources.OfficePerth;
                else if (dbLocale == DatabaseLocale.Montreal)
                    officeName = BluePrintsResources.OfficeMontreal;
                else
                    officeName = BluePrintsResources.OfficeUSA;

                primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(officeName);
            }
            else
                primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo);

            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
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
        }

        private Func<IRepositoryQuery<STOCK_ITEMS>, IQueryable<STOCK_ITEMS>> STOCK_ITEMSProjectionFunc()
        {
            return query => query;
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            if (isYearToDate)
                return query => query;
            else
                return query => query.Where(x => x.JOBCODE == loadPROJECT.NUMBER.ToString());
        }

        public ObservableCollection<JOB_TRANSACTIONS> JOB_TRANSACTIONS = new ObservableCollection<JOB_TRANSACTIONS>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.X_JOB_TRANSACTIONS_DETAIL_V3);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            IsPasteCellLevel = true;
        }

        protected override Func<IRepositoryQuery<X_JOB_TRANSACTIONS_DETAIL_V3>, IQueryable<X_JOB_TRANSACTIONS_DETAIL_V3>> specifyMainViewModelProjection()
        {
            if (isYearToDate)
            {
                if(Is2020Onwards)
                {
                    DateTime date2020FirstDay = new DateTime(2020, 1, 1);
                    return query => query.Where(x => x.TRANSDATE != null && ((DateTime)x.TRANSDATE) >= date2020FirstDay);
                }
                else
                    return query => query.Where(x => x.TRANSDATE != null);
            }
            else
                return query => query.Where(x => x.MASTER_JOBNO == loadJOBCOST_HDR.MASTER_JOBNO);
        }

        protected override void InstantFeedbackOtherUnitOfWorkSaveChanges()
        {
            primeroUnitOfWork.SaveChanges();
            base.InstantFeedbackOtherUnitOfWorkSaveChanges();
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(X_JOB_TRANSACTIONS_DETAIL_V3 projection, out bool isNew)
        {
            isNew = false;
            ApplyInstantFeedbackEntityPropertiesToOtherUnitOfWorkEntity(projection);
            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        protected override void OnAfterProjectionsSave(IEnumerable<X_JOB_TRANSACTIONS_DETAIL_V3> projections)
        {
            primeroUnitOfWork.SaveChanges();
            base.OnAfterProjectionsSave(projections);
        }

        protected override void ApplyInstantFeedbackEntityPropertiesToOtherUnitOfWorkEntity(X_JOB_TRANSACTIONS_DETAIL_V3 projection)
        {
            JOB_TRANSACTIONS findJOB_TRANSACTION = primeroUnitOfWork.JOB_TRANSACTIONS.FirstOrDefault(x => x.SEQNO == projection.SEQNO);
            if(findJOB_TRANSACTION != null)
            {
                findJOB_TRANSACTION.JOBNO = projection.JOBNO;
                findJOB_TRANSACTION.COST_GROUP = projection.COST_GROUP_NO;
                findJOB_TRANSACTION.COST_TYPE = projection.COST_TYPE_NO;
                findJOB_TRANSACTION.STOCKCODE = projection.STOCKCODE;
                findJOB_TRANSACTION.X_VARIATIONCODE = projection.VARIATION_CODE;
                findJOB_TRANSACTION.DESCRIPTION = projection.DESCRIPTION;
                findJOB_TRANSACTION.STAFFNO = projection.ACCNO;
                findJOB_TRANSACTION.QUANTITY = projection.QUANTITY;
                findJOB_TRANSACTION.STOCKCODE = projection.STOCKCODE;

                if(projection.QtyEdited && CanEditQuantity)
                {
                    if (findJOB_TRANSACTION.QUANTITY != null)
                    {
                        if (findJOB_TRANSACTION.UNITCOST != null)
                            findJOB_TRANSACTION.LINECOST = findJOB_TRANSACTION.UNITCOST * findJOB_TRANSACTION.QUANTITY;

                        if (findJOB_TRANSACTION.UNITPRICE != null)
                        {
                            findJOB_TRANSACTION.LINECHARGE = findJOB_TRANSACTION.UNITPRICE * findJOB_TRANSACTION.QUANTITY;
                            findJOB_TRANSACTION.LINETOTAL = findJOB_TRANSACTION.LINECHARGE;
                        }

                        findJOB_TRANSACTION.LINETOTAL_TAX = (double)((findJOB_TRANSACTION.LINETOTAL * findJOB_TRANSACTION.TAXRATE) / 100);
                        findJOB_TRANSACTION.LINE_TAX = findJOB_TRANSACTION.LINETOTAL_TAX;

                        findJOB_TRANSACTION.LINETOTAL_INCTAX = findJOB_TRANSACTION.LINETOTAL + findJOB_TRANSACTION.LINETOTAL_TAX;
                        projection.LINECOST = findJOB_TRANSACTION.LINECOST;
                        projection.Update();
                    }
                }
            }

            projection.QtyEdited = false;
        }

        public override void FullRefresh()
        {
            base.FullRefresh();
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
            get { return "TransactionEntryViewModelWrapper_v2" + view_project_specific_affix + officeName; }
        }

        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
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
        #endregion

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, X_JOB_TRANSACTIONS_DETAIL_V3 projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new X_JOB_TRANSACTIONS_DETAIL_V3().QUANTITY))
                projection.QtyEdited = true;

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedValueValidation(X_JOB_TRANSACTIONS_DETAIL_V3 projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(X_JOB_TRANSACTIONS_DETAIL_V3 projection)
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

        public IEnumerable<X_JOB_TRANSACTIONS_DETAIL_V3> X_JOB_TRANSACTIONS_DETAILCollection
        {
            get
            {
                return GetEntities<X_JOB_TRANSACTIONS_DETAIL_V3>();
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

        public override void CleanUpEntitiesLoader()
        {
            JOBCOST_HDRInstantFeedbackCollectionViewModel?.Dispose();
            base.CleanUpEntitiesLoader();
        }
    }
}

