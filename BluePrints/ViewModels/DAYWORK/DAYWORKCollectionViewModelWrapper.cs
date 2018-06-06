using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class DAYWORKCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <DAYWORK, DAYWORK, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of DAYWORKCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DAYWORKCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DAYWORKCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the DAYWORKCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the DAYWORKCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DAYWORKCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public PROJECT loadPROJECT { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var param = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = param.GetEntity();
            DataDate = DateTime.Now.Date;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_LABOURS, DAYWORK_LABOURProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_EQUIPMENTS, DAYWORK_EQUIPMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_MATERIALS, DAYWORK_MATERIALProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_STAFF_ROLES, DAYWORK_STAFF_ROLEProjectionFunc);
        }

        private Func<IRepositoryQuery<DAYWORK_LABOUR>, IQueryable<DAYWORK_LABOUR>> DAYWORK_LABOURProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DAYWORK_EQUIPMENT>, IQueryable<DAYWORK_EQUIPMENT>> DAYWORK_EQUIPMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DAYWORK_MATERIAL>, IQueryable<DAYWORK_MATERIAL>> DAYWORK_MATERIALProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DAYWORK_STAFF_ROLE>, IQueryable<DAYWORK_STAFF_ROLE>> DAYWORK_STAFF_ROLEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.DAYWORKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<DAYWORK>, IQueryable<DAYWORK>> specifyMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DAYWORK> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeWorkSaved;
            DAYWORK_LABOURSCollectionViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeLabourSaved;
            DAYWORK_EQUIPMENTSCollectionViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEquipmentSaved;
            DAYWORK_MATERIALSCollectionViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeMaterialSaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(DAYWORK projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(DAYWORK projection)
        {
            return string.Empty;
        }

        private bool onBeforeWorkSaved(DAYWORK entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.WORKDATE = DataDate;
            return true;
        }

        private bool onBeforeLabourSaved(DAYWORK_LABOUR entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.WORKDATE = DataDate;
            return true;
        }

        private bool onBeforeEquipmentSaved(DAYWORK_EQUIPMENT entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.WORKDATE = DataDate;
            return true;
        }


        private bool onBeforeMaterialSaved(DAYWORK_MATERIAL entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.WORKDATE = DataDate;
            return true;
        }
        #endregion

        #region View Properties

        public DateTime DataDate { get; set; }
        public string DataDateStr => DataDate.ToString("dd-MMM-yy");

        public bool CanDateBackward()
        {
            if (MainViewModel == null || MainViewModel.IsLoading)
                return false;

            return false;
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

            double multiplier = navigationType == DateNavigationType.Forward ? 1 : -1;
            DataDate = DataDate.AddDays(multiplier);
            this.RaisePropertyChanged(x => x.DataDateStr);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "DAYWORKCollectionViewModelWrapper"; }
        }

        ObservableCollection<DAYWORK_LABOUR> daywork_labours;
        public ObservableCollection<DAYWORK_LABOUR> DAYWORK_LABOURS
        {
            get
            {
                if (DAYWORK_LABOURCollection == null)
                    return null;

                if (daywork_labours == null)
                {
                    daywork_labours = new ObservableCollection<DAYWORK_LABOUR>();
                    foreach (DAYWORK_LABOUR daywork_labour in DAYWORK_LABOURCollection)
                        daywork_labours.Add(daywork_labour);
                }

                return daywork_labours;
            }
        }

        ObservableCollection<DAYWORK_EQUIPMENT> daywork_equipments;
        public ObservableCollection<DAYWORK_EQUIPMENT> DAYWORK_EQUIPMENTS
        {
            get
            {
                if (DAYWORK_MATERIALCollection == null)
                    return null;

                if (daywork_equipments == null)
                {
                    daywork_equipments = new ObservableCollection<DAYWORK_EQUIPMENT>();
                    foreach (DAYWORK_EQUIPMENT dayworkEquipment in DAYWORK_EQUIPMENTCollection)
                        daywork_equipments.Add(dayworkEquipment);
                }

                return daywork_equipments;
            }
        }


        ObservableCollection<DAYWORK_MATERIAL> daywork_materials;
        public ObservableCollection<DAYWORK_MATERIAL> DAYWORK_MATERIALS
        {
            get
            {
                if (DAYWORK_MATERIALCollection == null)
                    return null;

                if (daywork_materials == null)
                {
                    daywork_materials = new ObservableCollection<DAYWORK_MATERIAL>();
                    foreach (DAYWORK_MATERIAL dayworkMaterial in DAYWORK_MATERIALCollection)
                    {
                        daywork_materials.Add(dayworkMaterial);
                    }
                }

                return daywork_materials;
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

        public IEnumerable<DAYWORK_LABOUR> DAYWORK_LABOURCollection
        {
            get
            {
                var collection = GetEntities<DAYWORK_LABOUR>();
                return collection;
            }
        }

        public IEnumerable<DAYWORK_EQUIPMENT> DAYWORK_EQUIPMENTCollection
        {
            get
            {
                var collection = GetEntities<DAYWORK_EQUIPMENT>();
                return collection;
            }
        }

        public IEnumerable<DAYWORK_MATERIAL> DAYWORK_MATERIALCollection
        {
            get
            {
                var collection = GetEntities<DAYWORK_MATERIAL>();
                return collection;
            }
        }

        public CollectionViewModel<DAYWORK_LABOUR, DAYWORK_LABOUR, Guid, IBluePrintsEntitiesUnitOfWork> DAYWORK_LABOURSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DAYWORK_LABOUR, DAYWORK_LABOUR, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DAYWORK_LABOUR>();
            }
        }

        public CollectionViewModel<DAYWORK_EQUIPMENT, DAYWORK_EQUIPMENT, Guid, IBluePrintsEntitiesUnitOfWork> DAYWORK_EQUIPMENTSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DAYWORK_EQUIPMENT, DAYWORK_EQUIPMENT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DAYWORK_EQUIPMENT>();
            }
        }

        public CollectionViewModel<DAYWORK_MATERIAL, DAYWORK_MATERIAL, Guid, IBluePrintsEntitiesUnitOfWork> DAYWORK_MATERIALSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DAYWORK_MATERIAL, DAYWORK_MATERIAL, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DAYWORK_MATERIAL>();
            }
        }

        public CollectionViewModel<DAYWORK_STAFF_ROLE, DAYWORK_STAFF_ROLE, Guid, IBluePrintsEntitiesUnitOfWork> DAYWORK_STAFF_ROLESCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DAYWORK_STAFF_ROLE, DAYWORK_STAFF_ROLE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DAYWORK_STAFF_ROLE>();
            }
        }
        #endregion
    }
}