using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
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
        public PROJECT loadPROJECT { get; set; }
        public HSEProjection EditingEntity { get; set; }
        private bool isCompletelyLoaded { get; set; }
        protected override void resolveParameters(object parameter)
        {
            isCompletelyLoaded = false;
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.HSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<HSE>, IQueryable<HSEProjection>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).Select(x => new HSEProjection() { Entity = x });
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<HSEProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }


        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            isCompletelyLoaded = true;
            EditingEntity = MainViewModel.Entities.FirstOrDefault(x => x.Entity.HSE_DATE == DataDate);
            if (EditingEntity == null)
            {
                HSEProjection newDAYWORK = new HSEProjection();
                newDAYWORK.Entity.HSE_DATE = DataDate;
                newDAYWORK.Entity.GUID_PROJECT = loadPROJECT.GUID;
                MainViewModel.Save(newDAYWORK);
                EditingEntity = newDAYWORK;
            }

            EditingEntity.Update();
            this.RaisePropertyChanged(x => x.EditingEntity);

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "HSESingleObjectViewModelWrapper"; }
        }

        #endregion

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(HSEProjection projection)
        {
            projection.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }

        public override string UnifiedRowValidation(HSEProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(HSEProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }
        #endregion

        #region Data Date
        private DateTime _dataDate;
        public DateTime DataDate
        {
            get => _dataDate;
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