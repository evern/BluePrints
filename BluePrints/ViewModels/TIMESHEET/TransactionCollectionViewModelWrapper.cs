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
    public partial class TransactionCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<X_JOB_TRANSACTIONS_DETAIL_SeqNo, X_JOB_TRANSACTIONS_DETAIL_SeqNo, int, IPrimeroEntitiesUnitOfWork>
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
        protected override string readOnlyMessage => "Cells are read only because you do not have authority to edit transactions";
        protected TransactionCollectionViewModelWrapper()
        {
            IsReadOnly = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Transactions)) == LoginCredentials.PermissionStatus.ReadOnly;
            IsCostsVisible = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_ShowCosts)) == LoginCredentials.PermissionStatus.All;
        }
        
        #region Database Operation
        private Data.PROJECT loadPROJECT;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        JOBCOST_HDR loadJOBCOST_HDR;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
        }

        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc, x => loadJOBCOST_HDR = x);
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query.Where(x => x.JOBCODE.Contains(loadPROJECT.NUMBER.ToString()));
        }

        public ObservableCollection<JOB_TRANSACTIONS> JOB_TRANSACTIONS = new ObservableCollection<JOB_TRANSACTIONS>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.X_JOB_TRANSACTIONS_DETAIL_SeqNos);
        }

        protected override Func<IRepositoryQuery<X_JOB_TRANSACTIONS_DETAIL_SeqNo>, IQueryable<X_JOB_TRANSACTIONS_DETAIL_SeqNo>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.master_jobno == loadJOBCOST_HDR.JOBNO);
        }

        protected Func<IRepositoryQuery<X_JOB_TRANSACTIONS_DETAIL_SeqNo>, IQueryable<X_JOB_TRANSACTIONS_DETAIL_SeqNo>> X_JOB_TRANSACTIONS_DETAIL_SeqNoProjection()
        {
            return query => query.Where(x => x.jobcode == loadPROJECT.NUMBER);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(X_JOB_TRANSACTIONS_DETAIL_SeqNo projection, out bool isNew)
        {
            isNew = false;
            JOB_TRANSACTIONS findJOB_TRANSACTION = primeroUnitOfWork.JOB_TRANSACTIONS.FirstOrDefault(x => x.SEQNO == projection.SEQNO);
            if(findJOB_TRANSACTION != null)
            {
                findJOB_TRANSACTION.JOBNO = projection.jobno;
                findJOB_TRANSACTION.COST_GROUP = projection.COST_GROUP;
                findJOB_TRANSACTION.COST_TYPE = projection.CostType;
                findJOB_TRANSACTION.X_VARIATIONCODE = projection.X_VARIATIONCODE;
                findJOB_TRANSACTION.DESCRIPTION = projection.description;
                findJOB_TRANSACTION.STAFFNO = projection.accno;
            }

            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        protected override void OnAfterProjectionsSave(IEnumerable<X_JOB_TRANSACTIONS_DETAIL_SeqNo> projections)
        {
            primeroUnitOfWork.SaveChanges();
            base.OnAfterProjectionsSave(projections);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<X_JOB_TRANSACTIONS_DETAIL_SeqNo> entities)
        {
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.IsPasteCellLevel = true;
            MainViewModel.IsPersistentView = true;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Properties
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "TransactionEntryViewModelWrapper_v1" + view_project_specific_affix; }
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

        public override string UnifiedValueValidation(X_JOB_TRANSACTIONS_DETAIL_SeqNo projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(X_JOB_TRANSACTIONS_DETAIL_SeqNo projection)
        {
            return string.Empty;
        }

        public IEnumerable<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_HDR>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.JOBCODE);
                return collection;
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

        public IEnumerable<X_JOB_TRANSACTIONS_DETAIL_SeqNo> X_JOB_TRANSACTIONS_DETAILCollection
        {
            get
            {
                return GetEntities<X_JOB_TRANSACTIONS_DETAIL_SeqNo>();
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
    }
}

