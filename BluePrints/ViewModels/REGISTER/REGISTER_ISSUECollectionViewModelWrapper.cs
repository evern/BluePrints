using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class REGISTER_ISSUECollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_ISSUE, REGISTER_ISSUE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_ISSUECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_ISSUECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_ISSUECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        DispatcherTimer delayedRefreshTimer;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            delayedRefreshTimer = new DispatcherTimer();
            delayedRefreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedRefreshTimer.Tick += DelayedRefreshTimer_Tick;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_CHANGE, REGISTER_CHANGEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_HOLD, REGISTER_HOLDProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<REGISTER_CHANGE>, IQueryable<REGISTER_CHANGE>> REGISTER_CHANGEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<REGISTER_HOLD>, IQueryable<REGISTER_HOLD>> REGISTER_HOLDProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_ISSUE);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<REGISTER_ISSUE>, IQueryable<REGISTER_ISSUE>> specifyMainViewModelProjection()
        {
            return query => constructMainViewModelProjection(query);
        }

        private IQueryable<REGISTER_ISSUE> constructMainViewModelProjection(IQueryable<REGISTER_ISSUE> query)
        {
            List<REGISTER_ISSUE> registerIssue = query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NUMBER).ToList();
            registerIssue.ForEach(x => x.SetRegisterChange(REGISTER_CHANGECollection));
            registerIssue.ForEach(x => x.SetRegisterHold(REGISTER_HOLDCollection));
            return registerIssue.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_ISSUE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(REGISTER_CHANGE) || changedType == typeof(REGISTER_HOLD))
            {
                FullRefreshWithoutClearingUndoRedo();
                return;
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        #region Collection Call Backs
        protected override bool onBeforeEntitySavedIsContinue(REGISTER_ISSUE projection)
        {
            if (projection.GUID == Guid.Empty && projection.DATE_RAISED == null)
                projection.DATE_RAISED = DateTime.Now;

            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.onBeforeEntitySavedIsContinue(projection);
        }

        public override string UnifiedRowValidation(REGISTER_ISSUE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(REGISTER_ISSUE projection, string field_name, object new_value)
        {
            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_ISSUE().DATE_CLOSED))
            {
                DateTime? dateClosed = (DateTime?)new_value;
                if (projection.DATE_RAISED != null && dateClosed != null && ((DateTime)projection.DATE_RAISED).Date > ((DateTime)dateClosed).Date)
                    return "Date closed cannot be earlier than date raised";
            }

            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_ISSUE().DATE_RAISED))
            {
                DateTime? dateRaised = (DateTime?)new_value;
                if (projection.DATE_CLOSED != null && dateRaised != null && ((DateTime)dateRaised).Date > ((DateTime)projection.DATE_CLOSED).Date)
                    return "Date raised cannot be later than date closed";
            }

            return string.Empty;
        }
        #endregion

        #endregion

        #region IEntityNumber
        protected override string GetEntityNumberFieldName()
        {
            return BindableBase.GetPropertyName(() => new REGISTER_CHANGE().NUMBER);
        }

        protected override int DefaultNumericFieldLength()
        {
            return Int32.Parse(BluePrintsResources.Default_Register_Numeric_Length);
        }
        #endregion

        #region View Properties
        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void HoldRegister()
        {
            DocumentInfo DocumentInfo = new DocumentInfo("View_HoldRegister" + loadPROJECT.EntityKey.ToString(),
            new EntitiesParameter<PROJECT>(loadPROJECT),
                "REGISTER_HOLDCollectionView",
                "[" + loadPROJECT.NUMBER + "] Hold Register");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public bool CanSendToHoldRegister()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return DisplaySelectedEntity.RegisterChange == null && DisplaySelectedEntity.RegisterHold == null;
        }

        public bool CanSendToChangeRegister()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return DisplaySelectedEntity.RegisterChange == null && DisplaySelectedEntity.RegisterHold == null;
        }

        public void SendToChangeRegister()
        {
            if (DisplaySelectedEntity == null)
                return;

            var editingEntity = DisplaySelectedEntity;
            REGISTER_CHANGE newRegister = new REGISTER_CHANGE();
            newRegister.GUID_PROJECT = loadPROJECT.GUID;
            newRegister.GUID_AREA = DisplaySelectedEntity.GUID_AREA;
            newRegister.NUMBER = getChangeRegisterNewNumber();
            newRegister.TITLE = DisplaySelectedEntity.TITLE;
            newRegister.DESCRIPTION = DisplaySelectedEntity.DESCRIPTION;
            newRegister.SCHEDULE_IMPACT = DisplaySelectedEntity.SCHEDULE_IMPACT;
            newRegister.COST_IMPACT = DisplaySelectedEntity.COST_IMPACT;
            newRegister.IMPACT_TYPE = Register_ImpactType.Internal;
            newRegister.INTERDISC_CHECK_COMPLETE = false;
            newRegister.APPROVED = false;
            newRegister.DATE_RAISED = DateTime.Now;
            newRegister.CREATED = DateTime.Now;
            newRegister.CREATEDBY = LoginCredentials.CurrentUserGuid;
            REGISTER_CHANGEViewModel.Save(newRegister);

            editingEntity.GUID_CHANGE = newRegister.GUID;
            MainViewModel.Save(editingEntity);

            DocumentInfo DocumentInfo = new DocumentInfo("View_ChangeRegister" + loadPROJECT.EntityKey.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "REGISTER_CHANGECollectionView",
                    "[" + loadPROJECT.NUMBER + "] Change Register");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
            delayedRefreshTimer.Start();
        }

        public void SendToHoldRegister()
        {
            if (DisplaySelectedEntity == null)
                return;

            var editingEntity = DisplaySelectedEntity;
            REGISTER_HOLD newRegister = new REGISTER_HOLD();
            newRegister.GUID_PROJECT = loadPROJECT.GUID;
            newRegister.GUID_AREA = DisplaySelectedEntity.GUID_AREA;
            newRegister.NUMBER = getHoldRegisterNewNumber();
            newRegister.DESCRIPTION = DisplaySelectedEntity.DESCRIPTION;
            newRegister.DATE_RAISED = DateTime.Now;
            newRegister.CREATED = DateTime.Now;
            newRegister.CREATEDBY = LoginCredentials.CurrentUserGuid;
            REGISTER_HOLDViewModel.Save(newRegister);

            editingEntity.GUID_HOLD = newRegister.GUID;
            MainViewModel.Save(editingEntity);

            DocumentInfo DocumentInfo = new DocumentInfo("View_HoldRegister" + loadPROJECT.EntityKey.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "REGISTER_HOLDCollectionView",
                    "[" + loadPROJECT.NUMBER + "] Hold Register");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
            delayedRefreshTimer.Start();
        }


        private void DelayedRefreshTimer_Tick(object sender, EventArgs e)
        {
            delayedRefreshTimer.Stop();
            FullRefresh();
        }


        private string getChangeRegisterNewNumber()
        {
            IEnumerable<REGISTER_CHANGE> entitiesInOrder = REGISTER_CHANGEViewModel.Entities.OrderBy(x => x.EntityNumber);
            if(entitiesInOrder.Count() == 0)
                return StringFormatUtils.AppendStringWithEnumerator(string.Empty, 0, DefaultNumericFieldLength());

            REGISTER_CHANGE largestNumberEntity = entitiesInOrder.Last();
            string largestNumberString = largestNumberEntity.EntityNumber;
            int numericFieldLength = 0;
            long largestNumberValueOnly = 0;
            string largestNumberStringOnly = StringFormatUtils.ParseStringIntoComponents(largestNumberString, out numericFieldLength, out largestNumberValueOnly);
            long newRowNumber = largestNumberValueOnly + 1;
            return StringFormatUtils.AppendStringWithEnumerator(string.Empty, newRowNumber, DefaultNumericFieldLength());
        }

        private string getHoldRegisterNewNumber()
        {
            IEnumerable<REGISTER_HOLD> entitiesInOrder = REGISTER_HOLDViewModel.Entities.OrderBy(x => x.EntityNumber);
            if (entitiesInOrder.Count() == 0)
                return StringFormatUtils.AppendStringWithEnumerator(string.Empty, 0, DefaultNumericFieldLength());

            REGISTER_HOLD largestNumberEntity = entitiesInOrder.Last();
            string largestNumberString = largestNumberEntity.EntityNumber;
            int numericFieldLength = 0;
            long largestNumberValueOnly = 0;
            string largestNumberStringOnly = StringFormatUtils.ParseStringIntoComponents(largestNumberString, out numericFieldLength, out largestNumberValueOnly);
            long newRowNumber = largestNumberValueOnly + 1;
            return StringFormatUtils.AppendStringWithEnumerator(string.Empty, newRowNumber, DefaultNumericFieldLength());
        }

        public bool IsActionedOnDrawingVisibility { get; set; }
        public override void ExportToExcel()
        {
            IsActionedOnDrawingVisibility = true;
            this.RaisePropertyChanged(x => x.IsActionedOnDrawingVisibility);
            base.ExportToExcel();
            IsActionedOnDrawingVisibility = false;
            this.RaisePropertyChanged(x => x.IsActionedOnDrawingVisibility);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "REGISTER_ISSUECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "REGISTER_ISSUECollectionViewModelWrapper_v1"; }
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

        protected override string ExportExcelFilename()
        {
            return loadPROJECT.NUMBER + "_Register_Issue.xlsx";
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<REGISTER_CHANGE> REGISTER_CHANGECollection
        {
            get
            {
                var collection = GetEntities<REGISTER_CHANGE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
                return collection;
            }
        }

        public IEnumerable<REGISTER_HOLD> REGISTER_HOLDCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_HOLD>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
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

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public CollectionViewModel<REGISTER_CHANGE, REGISTER_CHANGE, Guid, IBluePrintsEntitiesUnitOfWork> REGISTER_CHANGEViewModel
        {
            get
            {
                return (CollectionViewModel<REGISTER_CHANGE, REGISTER_CHANGE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<REGISTER_CHANGE>();
            }
        }

        public CollectionViewModel<REGISTER_HOLD, REGISTER_HOLD, Guid, IBluePrintsEntitiesUnitOfWork> REGISTER_HOLDViewModel
        {
            get
            {
                return (CollectionViewModel<REGISTER_HOLD, REGISTER_HOLD, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<REGISTER_HOLD>();
            }
        }
        #endregion
    }
}