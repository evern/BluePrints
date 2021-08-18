using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class HSESingleObjectViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <HSE, HSEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of HSESingleObjectViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static HSESingleObjectViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new HSESingleObjectViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the HSESingleObjectViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the HSESingleObjectViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected HSESingleObjectViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
#if MONTREAL
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(BluePrintsResources.OfficeMontreal);
#else
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
#endif

        public PROJECT loadPROJECT { get; set; }
        public HSEProjection EditingEntity { get; set; }
        private bool isCompletelyLoaded { get; set; }
        protected override void resolveParameters(object parameter)
        {
            isCompletelyLoaded = false;
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            DataDate =  new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); ;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<STAFF, STAFF, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STAFF);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.HSE_INJURIES, hseInjuriesProjection);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.HSE_INCIDENTS, hseIncidentsProjection);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.HSES);
        }

        protected override Func<IRepositoryQuery<HSE>, IQueryable<HSEProjection>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).ToList().Select(x => new HSEProjection() { Entity = x }).AsQueryable();
        }

        private Func<IRepositoryQuery<HSE_INJURY>, IQueryable<HSE_INJURY>> hseInjuriesProjection()
        {
            return query => query.Where(x => x.GUID_HSE == Guid.Empty);
        }

        private Func<IRepositoryQuery<HSE_INCIDENT>, IQueryable<HSE_INCIDENT>> hseIncidentsProjection()
        {
            return query => EditingEntity == null ? query.Where(x => x.GUID_HSE == Guid.Empty) : query.Where(x => x.GUID_HSE == EditingEntity.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<HSEProjection> entities)
        {
            HSE_INCIDENTViewModel.OnBeforeProjectionSaveIsContinueCallBack = HSE_INCIDENTOnBeforeEntitySaved;
            HSE_INJURYViewModel.OnBeforeProjectionSaveIsContinueCallBack = HSE_INJURYOnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            HSE_INCIDENTViewModel.FuncManualRowPastingIsContinue = ManualRowPasteAction;
            HSE_INCIDENTViewModel.SetParentViewModel(this);
            HSE_INJURYViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            EditingEntity = MainViewModel.Entities.FirstOrDefault(x => x.Entity.HSE_DATE == DataDate && x.Entity.GUID_PROJECT == loadPROJECT.GUID);
            if (EditingEntity == null)
            {
                HSEProjection newDAYWORK = new HSEProjection();
                newDAYWORK.GUID = Guid.Empty;
                newDAYWORK.Entity.HSE_DATE = DataDate;
                newDAYWORK.Entity.GUID_PROJECT = loadPROJECT.GUID;
                MainViewModel.Save(newDAYWORK);
                EditingEntity = newDAYWORK;
            }

            EditingEntity.Update();
            this.RaisePropertyChanged(x => x.EditingEntity);

            HSE_INCIDENTViewModel.Refresh();
            HSE_INJURYViewModel.Refresh();
            isCompletelyLoaded = true;

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "HSESingleObjectViewModelWrapper_v2"; }
        }

        public IEnumerable<STAFF> STAFFCollection
        {
            get
            {
                var collection = GetEntities<STAFF>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public bool ManualRowPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, HSE_INCIDENT pasteEntity, bool isLastRow)
        {
            KeyValuePair<ColumnBase, string> incidentData = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new HSE_INCIDENT().CLASSIFICATION)));
            if(incidentData.Value != string.Empty)
            {
                IncidentClassification incidentClassification;
                if (Enum.TryParse<IncidentClassification>(incidentData.Value, out incidentClassification))
                    pasteEntity.CLASSIFICATION = incidentClassification;
            }
            
            return true;
        }

        public CollectionViewModel<HSE_INCIDENT, HSE_INCIDENT, Guid, IBluePrintsEntitiesUnitOfWork> HSE_INCIDENTViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<HSE_INCIDENT, HSE_INCIDENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<HSE_INCIDENT>();
            }
        }

        public CollectionViewModel<HSE_INJURY, HSE_INJURY, Guid, IBluePrintsEntitiesUnitOfWork> HSE_INJURYViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<HSE_INJURY, HSE_INJURY, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<HSE_INJURY>();
            }
        }
        #endregion

        #region View Events
        public void EditControlPreviewMouseDown(MouseButtonEventArgs e)
        {
            BaseEdit sp = e.Source as BaseEdit;
            mainThreadDispatcher.BeginInvoke((Action)(() =>
            {
                sp.SelectAll();
            }), DispatcherPriority.Background);
        }

        public void EditValueChanged(EditValueChangedEventArgs e)
        {
            if (!isCompletelyLoaded)
                return;

            if (MainViewModel == null || EditingEntity == null)
                return;

            string fieldName = ((BaseEdit)e.OriginalSource).Tag.ToString();
            DataUtils.TrySetNestedValue(fieldName, EditingEntity, e.NewValue);

            MainViewModel.Save(EditingEntity);
            EditingEntity.Update();
        }
        #endregion

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(HSEProjection projection, out bool isNew)
        {
            projection.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public OperationInterceptMode HSE_INCIDENTOnBeforeEntitySaved(HSE_INCIDENT projection, out bool isNew)
        {
            isNew = false;
            projection.GUID_HSE = EditingEntity.GUID;
            return OperationInterceptMode.Continue;
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public OperationInterceptMode HSE_INJURYOnBeforeEntitySaved(HSE_INJURY projection, out bool isNew)
        {
            isNew = false;
            projection.GUID_HSE = EditingEntity.GUID;
            return OperationInterceptMode.Continue;
        }

        public override string UnifiedRowValidation(HSEProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(HSEProjection projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region Data Date
        private DateTime? _dataDate;
        public DateTime DataDate
        {
            get
            {
                if(_dataDate == null)
                    _dataDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                return (DateTime)_dataDate;
            }
            set
            {
                var firstDayOfMonth = new DateTime(value.Year, value.Month, 1);
                _dataDate = firstDayOfMonth;
                if (isCompletelyLoaded)
                {
                    this.RaisePropertyChanged(x => x.DataDate);
                    FullRefresh();
                }
            }
        }

        public bool CanDateBackward()
        {
            if (MainViewModel == null || MainViewModel.IsLoading)
                return false;

            return true;
        }

        public bool CanDateForward()
        {
            if (MainViewModel == null || MainViewModel.IsLoading)
                return false;

            return true;
        }

        public void DateForward()
        {
            DateChange(DateNavigationType.Forward);
        }

        public void DateBackward()
        {
            DateChange(DateNavigationType.Backward);
        }

        bool isBusy = false;
        protected void DateChange(DateNavigationType navigationType)
        {
            if (isBusy)
                return;

            int multiplier = navigationType == DateNavigationType.Forward ? 1 : -1;
            
            DataDate = DataDate.AddMonths(multiplier);
            this.RaisePropertyChanged(x => x.DataDate);
            FullRefresh();
        }
        #endregion
    }
}