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

        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            delayedRefreshTimer = new DispatcherTimer();
            delayedRefreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedRefreshTimer.Tick += DelayedRefreshTimer_Tick;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_CHANGE, REGISTER_CHANGEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_HOLD, REGISTER_HOLDProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
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

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_ISSUE);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<REGISTER_ISSUE>, IQueryable<REGISTER_ISSUE>> ConstructMainViewModelProjection()
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
            MainViewModel.AdditionalValidateCellCallBack = AdditionalCellValidation;
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void OnAfterAffectingEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (changedType == typeof(REGISTER_CHANGE) || changedType == typeof(REGISTER_HOLD))
            {
                FullRefreshWithoutClearingUndoRedo();
                return;
            }

            base.OnAfterAffectingEntitiesChanged(key, changedType, messageType, sender);
        }
        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(REGISTER_ISSUE entity)
        {
            if (entity.GUID == Guid.Empty && entity.DATE_RAISED == null)
                entity.DATE_RAISED = DateTime.Now;

            entity.GUID_PROJECT = loadPROJECT.GUID;
        }

        private void AdditionalCellValidation(GridCellValidationEventArgs e)
        {
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new REGISTER_ISSUE().DATE_CLOSED))
            {
                DateTime? dateClosed = (DateTime?)e.Value;
                var editingEntity = (REGISTER_ISSUE)e.Row;
                if (editingEntity.DATE_RAISED != null && dateClosed != null && 
                    editingEntity.DATE_RAISED > dateClosed)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Date closed cannot be earlier than date raised";
                }
            }

            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new REGISTER_ISSUE().DATE_RAISED))
            {
                DateTime? dateRaised = (DateTime?)e.Value;
                var editingEntity = (REGISTER_ISSUE)e.Row;
                if (editingEntity.DATE_CLOSED != null && dateRaised != null && 
                    dateRaised > editingEntity.DATE_CLOSED)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Date raised cannot be later than date closed";
                }
            }
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
            return Int32.Parse(BluePrintsResources.REGISTER_DefaultNumberFieldLength);
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

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
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

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
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

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
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

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "REGISTER_ISSUECollectionViewModelWrapper"; }
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