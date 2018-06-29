using BaseModel.Data.Helpers;
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
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
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
        public DAYWORK EditingEntity { get; set; }
        private bool isCompletelyLoaded { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var param = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = param.GetEntity();
            DataDate = DateTime.Now.Date;
            isCompletelyLoaded = false;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_STAFF_ROLES, DAYWORK_STAFF_ROLEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_EQUIPMENTS, DAYWORK_EQUIPMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_MATERIALS, DAYWORK_MATERIALProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_LABOURS, DAYWORK_LABOURProjectionFunc);
        }

        private Func<IRepositoryQuery<DAYWORK_LABOUR>, IQueryable<DAYWORK_LABOUR>> DAYWORK_LABOURProjectionFunc()
        {
            return query => daywork_labourProjection(query);
        }

        private IQueryable<DAYWORK_LABOUR> daywork_labourProjection(IRepositoryQuery<DAYWORK_LABOUR> dayworkLabour)
        {
            List<DAYWORK_LABOUR> dayWorkLabourCollection = dayworkLabour.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).ToList();
            dayWorkLabourCollection.ForEach(x => x.SetRoles(DAYWORK_STAFF_ROLESCollection));

            return dayWorkLabourCollection.AsQueryable();
        }

        private Func<IRepositoryQuery<DAYWORK_EQUIPMENT>, IQueryable<DAYWORK_EQUIPMENT>> DAYWORK_EQUIPMENTProjectionFunc()
        {
            return query => daywork_equipmentProjection(query);
        }

        private IQueryable<DAYWORK_EQUIPMENT> daywork_equipmentProjection(IRepositoryQuery<DAYWORK_EQUIPMENT> daywork_equipment)
        {
            List<DAYWORK_EQUIPMENT> daywork_equipmentCollection = daywork_equipment.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).ToList();
            daywork_equipmentCollection.ForEach(x => x.SetHistory(daywork_equipmentCollection));

            return daywork_equipmentCollection.AsQueryable();
        }

        private Func<IRepositoryQuery<DAYWORK_MATERIAL>, IQueryable<DAYWORK_MATERIAL>> DAYWORK_MATERIALProjectionFunc()
        {
            return query => daywork_materialProjection(query);
        }

        private IQueryable<DAYWORK_MATERIAL> daywork_materialProjection(IRepositoryQuery<DAYWORK_MATERIAL> daywork_material)
        {
            List<DAYWORK_MATERIAL> daywork_materialCollection = daywork_material.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).ToList();
            daywork_materialCollection.ForEach(x => x.SetHistory(daywork_materialCollection));

            return daywork_materialCollection.AsQueryable();
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
            return query => daywork_labourProjection(query);
        }

        private IQueryable<DAYWORK> daywork_labourProjection(IRepositoryQuery<DAYWORK> daywork)
        {
            List<DAYWORK> dayWorkCollection = daywork.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).ToList();
            dayWorkCollection.ForEach(x => x.SetHistory(dayWorkCollection));

            return dayWorkCollection.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DAYWORK> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeWorkSaved;
            DAYWORK_EQUIPMENTSCollectionViewModel.UnifiedValueChangingCallback = equipmentUnifiedCellValueChanging;
            DAYWORK_LABOURSCollectionViewModel.UnifiedValueChangingCallback = labourUnifiedCellValueChanging;
            DAYWORK_LABOURSCollectionViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeLabourSaved;
            DAYWORK_EQUIPMENTSCollectionViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEquipmentSaved;
            DAYWORK_MATERIALSCollectionViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeMaterialSaved;
            DAYWORK_MATERIALSCollectionViewModel.UnifiedValueChangingCallback = materialUnifiedCellValueChanging;

            DAYWORK_LABOURSCollectionViewModel.SetParentViewModel(this);
            DAYWORK_EQUIPMENTSCollectionViewModel.SetParentViewModel(this);
            DAYWORK_MATERIALSCollectionViewModel.SetParentViewModel(this);
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            isCompletelyLoaded = true;
            EditingEntity = MainViewModel.Entities.FirstOrDefault(x => x.WORKDATE == DataDate && x.GUID_PROJECT == loadPROJECT.GUID);
            if (EditingEntity == null)
            {
                DAYWORK newDAYWORK = new DAYWORK();
                newDAYWORK.DESCRIPTION = "New";
                newDAYWORK.REQUESTED_BY = loadPROJECT.CLIENT;
                newDAYWORK.WORKDATE = DataDate;
                newDAYWORK.GUID_PROJECT = loadPROJECT.GUID;
                MainViewModel.Save(newDAYWORK);
                EditingEntity = newDAYWORK;
            }

            EditingEntity.SetHistory(MainViewModel.Entities);
            EditingEntity.Update();
            this.RaisePropertyChanged(x => x.EditingEntity);

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
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

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, DAYWORK projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new DAYWORK().DESCRIPTION))
            {
                //role is not instantiated with commodity codes to be selected, hence initialization begins here
                if (isNew && new_value != null)
                {
                    projection.SetHistory(DAYWORKCollection);
                }

                DAYWORK daywork = DAYWORKCollection.FirstOrDefault(x => x.DESCRIPTION == new_value.ToString());
                if (daywork != null)
                    projection.REQUESTED_BY = daywork.REQUESTED_BY;

                projection.Update();
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        private void labourUnifiedCellValueChanging(string field_name, object old_value, object new_value, DAYWORK_LABOUR projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new DAYWORK_LABOUR().RESOURCE_ID))
            {
                //role is not instantiated with commodity codes to be selected, hence initialization begins here
                if (isNew && new_value != null)
                {
                    projection.RESOURCE_ID = (int)new_value;
                    projection.SetRoles(DAYWORK_STAFF_ROLESCollection);
                }
                DAYWORK_STAFF_ROLE labour = DAYWORK_STAFF_ROLESCollection.FirstOrDefault(x => x.RESOURCE_ID == (int)new_value && x.IS_DEFAULT);
                if (labour != null)
                {
                    projection.TRADE = labour.PROJECT_ROLE;
                    projection.RATE = labour.RATE;
                }
                projection.Update();
            }
        }

        private void equipmentUnifiedCellValueChanging(string field_name, object old_value, object new_value, DAYWORK_EQUIPMENT projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new DAYWORK_EQUIPMENT().ITEM))
            {
                //role is not instantiated with commodity codes to be selected, hence initialization begins here
                if (isNew && new_value != null)
                {
                    projection.SetHistory(DAYWORK_EQUIPMENTCollection);
                }

                DAYWORK_EQUIPMENT equipment = DAYWORK_EQUIPMENTCollection.FirstOrDefault(x => x.ITEM == new_value.ToString());
                if (equipment != null)
                {
                    projection.QUANTITY = equipment.QUANTITY;
                    projection.PRICE = equipment.PRICE;
                }

                projection.Update();
            }
        }

        private void materialUnifiedCellValueChanging(string field_name, object old_value, object new_value, DAYWORK_MATERIAL projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new DAYWORK_MATERIAL().ITEM))
            {
                //role is not instantiated with commodity codes to be selected, hence initialization begins here
                if (isNew && new_value != null)
                {
                    projection.SetHistory(DAYWORK_MATERIALCollection);
                }

                DAYWORK_MATERIAL material = DAYWORK_MATERIALCollection.FirstOrDefault(x => x.ITEM == new_value.ToString());
                if (material != null)
                {
                    projection.QUANTITY = material.QUANTITY;
                    projection.PRICE = material.PRICE;
                }

                projection.Update();
            }
        }

        private bool onBeforeLabourSaved(DAYWORK_LABOUR entity)
        {
            decimal labourRate = entity.RATE == null ? 0 : (decimal)entity.RATE;
            DAYWORK_STAFF_ROLE labour = DAYWORK_STAFF_ROLESCollection.FirstOrDefault(x => x.RESOURCE_ID == entity.RESOURCE_ID && x.PROJECT_ROLE == entity.TRADE);
            if (labour == null)
            {
                removeExistingDefaultLabour(entity.RESOURCE_ID);
                DAYWORK_STAFF_ROLE newLabour = new DAYWORK_STAFF_ROLE();
                newLabour.RESOURCE_ID = entity.RESOURCE_ID;
                newLabour.PROJECT_ROLE = entity.TRADE;
                newLabour.RATE = labourRate;
                newLabour.IS_DEFAULT = true;
                newLabour.GUID_PROJECT = loadPROJECT.GUID;
                DAYWORK_STAFF_ROLESCollectionViewModel.Save(newLabour);
            }
            else if (labour != null)
            {
                removeExistingDefaultLabour(entity.RESOURCE_ID);
                labour.IS_DEFAULT = true;
                labour.RATE = labourRate;
                DAYWORK_STAFF_ROLESCollectionViewModel.Save(labour);
            }

            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.WORKDATE = DataDate;
            return true;
        }

        private void removeExistingDefaultLabour(int resourceId)
        {
            DAYWORK_STAFF_ROLE existingDefaultLabour = DAYWORK_STAFF_ROLESCollection.FirstOrDefault(x => x.RESOURCE_ID == resourceId && x.IS_DEFAULT);
            if (existingDefaultLabour != null)
            {
                existingDefaultLabour.IS_DEFAULT = false;
                DAYWORK_STAFF_ROLESCollectionViewModel.Save(existingDefaultLabour);
            }
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
        public void DayworkDescription_EditValueChanging(EditValueChangingEventArgs e)
        {
            if (!isCompletelyLoaded)
                return;

            if (EditingEntity != null)
            {
                if (((BaseEdit)e.OriginalSource).Tag.ToString() == BindableBase.GetPropertyName(() => new DAYWORK().DESCRIPTION))
                {
                    DAYWORK daywork = DAYWORKCollection.FirstOrDefault(x => x.DESCRIPTION == e.NewValue.ToString());
                    if (daywork != null)
                        EditingEntity.REQUESTED_BY = daywork.REQUESTED_BY;

                    EditingEntity.Update();
                }
            }
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
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(DAYWORK_LABOUR))
                refreshLabours();

            if (changedType == typeof(DAYWORK_EQUIPMENT))
                refreshEquipments();

            if (changedType == typeof(DAYWORK_MATERIAL))
                refreshMaterials();

            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }

        private DateTime _dataDate;
        public DateTime DataDate
        {
            get => _dataDate;
            set
            {
                _dataDate = value;

                if(isCompletelyLoaded)
                {
                    this.RaisePropertyChanged(x => x.DataDate);
                    FullRefresh();
                }
            }
        }

        public string DataDateStr => DataDate.ToString("dd-MMM-yy");

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

            double multiplier = navigationType == DateNavigationType.Forward ? 1 : -1;
            DataDate = DataDate.AddDays(multiplier);
            this.RaisePropertyChanged(x => x.DataDate);
            this.RaisePropertyChanged(x => x.DataDateStr);
            FullRefresh();
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "DAYWORKCollectionViewModelWrapper"; }
        }

        private void refreshLabours()
        {
            daywork_labours = null;
            this.RaisePropertyChanged(x => x.DAYWORK_LABOURS);
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
                    foreach (DAYWORK_LABOUR daywork_labour in DAYWORK_LABOURCollection.Where(x => x.WORKDATE == DataDate))
                        daywork_labours.Add(daywork_labour);
                }

                return daywork_labours;
            }
        }

        private void refreshEquipments()
        {
            daywork_equipments = null;
            this.RaisePropertyChanged(x => x.DAYWORK_EQUIPMENTS);
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
                    foreach (DAYWORK_EQUIPMENT dayworkEquipment in DAYWORK_EQUIPMENTCollection.Where(x => x.WORKDATE == DataDate))
                        daywork_equipments.Add(dayworkEquipment);
                }

                return daywork_equipments;
            }
        }

        private void refreshMaterials()
        {
            daywork_materials = null;
            this.RaisePropertyChanged(x => x.DAYWORK_MATERIALS);
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
                    foreach (DAYWORK_MATERIAL dayworkMaterial in DAYWORK_MATERIALCollection.Where(x => x.WORKDATE == DataDate))
                        daywork_materials.Add(dayworkMaterial);
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

        public override void FullRefresh()
        {
            isCompletelyLoaded = false;
            daywork_labours = null;
            daywork_equipments = null;
            daywork_materials = null;
            displayEntities = null;
            base.FullRefresh();
        }

        ObservableCollection<DAYWORK> displayEntities;

        public override ObservableCollection<DAYWORK> DisplayEntities
        {
            get
            {
                if (displayEntities == null)
                {
                    if (MainViewModel != null)
                    {
                        displayEntities = new ObservableCollection<DAYWORK>();
                        foreach (DAYWORK entity in MainViewModel.Entities.Where(x => x.WORKDATE == DataDate))
                        {
                            displayEntities.Add(entity);
                        }
                    }
                }

                return displayEntities;
            }
        }

        public IEnumerable<DAYWORK> DAYWORKCollection
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return MainViewModel.Entities;
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


        public IEnumerable<DAYWORK_STAFF_ROLE> DAYWORK_STAFF_ROLESCollection
        {
            get
            {
                var collection = GetEntities<DAYWORK_STAFF_ROLE>();
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