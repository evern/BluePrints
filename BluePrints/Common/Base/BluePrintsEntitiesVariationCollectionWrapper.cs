using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.ViewModels;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrints.Common.Base
{
    public abstract class BluePrintsEntitiesVariationCollectionWrapper<TMainEntity, TMainReportableEntity, TMainVariationEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : BluePrintsEntitiesCollectionWrapper<TMainEntity, TMainVariationEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>
        where TMainEntity : class, IDeliverable, ISupportVariation, new()
        where TMainReportableEntity : class, IReportable, ISupportVariation, new()
        where TMainVariationEntity : class, IBluePrintsVariationBase<TMainReportableEntity>, new()
        where TMainEntityUnitOfWork : IBluePrintsEntitiesUnitOfWork
    {
        protected abstract IDeliverableCollectionViewModelWrapper<TMainReportableEntity, TMainEntity> collectionViewModelWrapper { get; }

        protected PROJECT loadPROJECT;
        protected VARIATION loadVARIATION;
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected string baseEntityString = "Entity.Entity.Entity.";
        protected override void resolveParameters(object parameter)
        {
            //both parameters is required because when entity is first added the associating entity (PROJECT) is not loaded
            var receiveParameter =
                (DualEntitiesParameter<PROJECT, VARIATION>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadVARIATION = receiveParameter.GetSecondEntity();
            reinitializeCollectionViewModelWrapper();
        }

        private void reinitializeCollectionViewModelWrapper()
        {
            #region CollectionViewModelWrapper CallBacks
            collectionViewModelWrapper.InterfaceAddUndoRedoCallBack = AddUndo;
            collectionViewModelWrapper.InterfacePauseUndoRedoCallBack = PauseUndoRedo;
            collectionViewModelWrapper.InterfaceUnpauseUndoRedoCallBack = UnpauseUndoRedo;
            collectionViewModelWrapper.BaseEntityQueryCallBack = BaseEntityQueryCallBack;
            collectionViewModelWrapper.SelectedEntities = DisplaySelectedEntities.Select(x => x.Entity);
            collectionViewModelWrapper.SelectedEntityCallBack = () => DisplaySelectedEntity.Entity;
            collectionViewModelWrapper.OnReportablesLoadedCallBack = OnViewModelWrapperLoadedCallBack;
            collectionViewModelWrapper.ApplyViewSpecificPropertiesToEntityCallBack = ApplyViewSpecificPropertiesToEntityCallBack;
            collectionViewModelWrapper.SetParentViewModel(this);
            KeyValuePair<DeliverablesViewType, EstimateViewMode> valuePair = new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Both, EstimateViewMode.Budget);
            TripleEntitiesParameter<PROJECT, IAmBaseline, object> collectionViewParameter;
            if (typeof(TMainEntity) == typeof(BASELINE_ITEM))
                collectionViewParameter = new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(loadPROJECT, null, DeliverablesViewType.Both);
            else
                collectionViewParameter = new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(loadPROJECT, null, valuePair);

            collectionViewModelWrapper.OnParameterChanged(collectionViewParameter);
            #endregion
        }

        private void Add_undo_timer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddUndo(TMainReportableEntity progress_entity, string fieldName, object oldValue, object newValue, EntityMessageType messageType)
        {
            BackgroundWorker addUndoDelayedBackgroundWorker = new BackgroundWorker();
            addUndoDelayedBackgroundWorker.DoWork += addUndoDelayedBackgroundWorker_DoWork;
            addUndoDelayedBackgroundWorker.RunWorkerCompleted += addUndoDelayedBackgroundWorker_RunWorkerCompleted;
            addUndoDelayedBackgroundWorker.RunWorkerAsync(new object[] { progress_entity, fieldName, oldValue, newValue, messageType });
        }

        private void PauseUndoRedo()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.EntitiesUndoRedoManager.PauseActionId()));
        }

        private void UnpauseUndoRedo()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.EntitiesUndoRedoManager.UnpauseActionId()));
        }

        protected void addUndoDelayedBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (MainViewModel == null)
                return;

            //need to wait for entity to be queried before FirstOrDefault
            Thread.Sleep(10);
            var argumentObject = (object[])e.Argument;
            TMainReportableEntity progress_entity = (TMainReportableEntity)argumentObject[0];
            string fieldName = null;
            if (argumentObject[1] != null)
                fieldName = (string)argumentObject[1];
            object oldValue = argumentObject[2];
            object newValue = argumentObject[3];
            EntityMessageType messageType = (EntityMessageType)argumentObject[4];

            TMainVariationEntity variation_entity = MainViewModel.Entities.FirstOrDefault(x => x.Entity.EntityKey == progress_entity.EntityKey);
            if (variation_entity != null)
            {
                if (fieldName != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.EntitiesUndoRedoManager.AddUndo(variation_entity, "Entity." + fieldName, oldValue, newValue, messageType)));
                else
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.EntitiesUndoRedoManager.AddUndo(variation_entity, null, null, null, messageType)));
            }
        }

        private void addUndoDelayedBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (MainViewModel == null)
                return;

            MainViewModel.RaisePropertiesChanged();
        }

        protected abstract IQueryable<TMainEntity> BaseEntityQueryCallBack(IRepositoryQuery<TMainEntity> query);

        protected Guid load_context_guid { get { return collectionViewModelWrapper.load_context_guid; } }
        protected Guid variation_guid { get { return loadVARIATION.GUID; } }
        protected Guid? variation_baseline_guid { get { return loadVARIATION.GUID_BASELINE; } }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc, x => loadVARIATION = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadVARIATION.GUID);
        }

        private Func<IRepositoryQuery<VARIATION_ITEM>, IQueryable<VARIATION_ITEM>> VARIATION_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_VARIATION == loadVARIATION.GUID);
        }

        protected IEnumerable<TMainReportableEntity> IReportableEntitiesCollection;
        bool iReportableCollectionLoaded => IReportableEntitiesCollection != null;
        protected void OnViewModelWrapperLoadedCallBack(IEnumerable<TMainReportableEntity> entities)
        {
            IReportableEntitiesCollection = entities;
            onAuxiliaryEntitiesCollectionLoaded();
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            if (!iReportableCollectionLoaded)
                return;

            StartCreatingMainViewModel();
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected abstract void StartCreatingMainViewModel();

        //Used by variation to generate new baseline
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainVariationEntity> entities)
        {
            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.CanBulkDeleteCallBack = CanBulkDeleteCallBack;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnBeforeEntityDeletedIsContinueCallBack = OnBeforeEntityDeleted;
            MainViewModel.OnMappingAdditionalChangedEntitiesProperties = OnMappingAdditionalChangedEntitiesProperties;
            collectionViewModelWrapper.GetEditableAllEntitiesCallBack = getEditableAllEntities;
            assign_additional_callbacks(MainViewModel, entities);
            MainViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private IEnumerable<TMainReportableEntity> getEditableAllEntities()
        {
            return DisplayEntities == null ? new List<TMainReportableEntity>() : DisplayEntities.Where(x => x.Variation_Action == VariationAction.Add).Select(x => x.Entity);
        }

        private void ApplyViewSpecificPropertiesToEntityCallBack(TMainReportableEntity reportableEntity)
        {
            if (reportableEntity.Baseline_Guid == null)
            {
                reportableEntity.Variation_Guid = loadVARIATION.GUID;
            }
        }

        protected abstract void assign_additional_callbacks(CollectionViewModel<TMainEntity, TMainVariationEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork> mainViewModel, IEnumerable<TMainVariationEntity> entities);

        #region Call Backs
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(VARIATION_ITEM))
            {
                VARIATION_ITEM updated_VARIATION_ITEM = VARIATION_ITEMCollection.FirstOrDefault(x => x.GUID == (Guid)key);
                if (updated_VARIATION_ITEM != null)
                {
                    TMainVariationEntity affectedDisplayEntity = getAffectedDisplayEntity(updated_VARIATION_ITEM);
                    if (affectedDisplayEntity != null)
                    {
                        affectedDisplayEntity.Variation_Units = updated_VARIATION_ITEM.VARIATION_UNITS;
                        GridControlService.RefreshSummary();
                        affectedDisplayEntity.Update();
                    }
                }

                return true;
            }

            return false;
        }

        private TMainVariationEntity getAffectedDisplayEntity(VARIATION_ITEM updated_VARIATION_ITEM)
        {
            foreach (TMainVariationEntity entity in MainViewModel.Entities)
            {
                if (entity.OriginalEntityKey == updated_VARIATION_ITEM.GUID_ORIBASEITEM)
                {
                    entity.VARIATION_ITEM = updated_VARIATION_ITEM;
                    return entity;
                }
            }

            return null;
        }

        /// <summary>
        /// disable when variation is submitted and if selection doesn't contain any entities with variation.add status 
        /// </summary>
        public bool CanFillDownCallBack(IEnumerable<TMainVariationEntity> selectedEntities, GridMenuInfo info)
        {
            //if (loadVARIATION.SUBMITTED != null || !selectedEntities.Any(x => x.Variation_Action == VariationAction.Add))

            if (loadVARIATION == null || loadVARIATION.SUBMITTED != null)
                return false;

            return true;
        }

        /// <summary>
        /// fill down can only be performed on newly added entity
        /// </summary>
        private bool ValidateFillDownCallBack(TMainVariationEntity fillDownEntity, string fieldName, object fillValue)
        {
            //if (fillDownEntity.Variation_Action != VariationAction.Add)
            //    return false;

            return affixOtherFillDownAllowance(fillDownEntity, fieldName, fillValue);
        }

        /// <summary>
        /// specification of any other fill down allowance specific to the entity
        /// </summary>
        protected abstract bool affixOtherFillDownAllowance(TMainVariationEntity fillDownEntity, string fieldName, object fillValue);

        /// <summary>
        /// allow all bulk edit operation to be performed on all status.add, but only allow e.g. unique internal number and variation units bulk edit to be performed on other statuses
        /// </summary>
        private bool ValidateBulkEditCallBack(TMainVariationEntity projection, string fieldName, object editValue)
        {
            if (projection.Variation_Action == VariationAction.Add)
                return true;

            if (projection.Variation_Action == VariationAction.Append &&
                fieldName == BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Units))
            {
                return true;
            }

            return affixOtherBulkEditAllowance(projection, fieldName, editValue);
        }

        /// <summary>
        /// specification of any other bulk edit allowance specific to the entity
        /// </summary>
        protected abstract bool affixOtherBulkEditAllowance(TMainVariationEntity projection, string fieldName, object editValue);

        private bool CanBulkDeleteCallBack(IEnumerable<TMainVariationEntity> selectedEntities)
        {
            return loadVARIATION.SUBMITTED == null && selectedEntities != null && selectedEntities.All(x => x.Variation_Action == VariationAction.Add);
        }

        #region Fundamentals
        protected void OnMappingAdditionalChangedEntitiesProperties(TMainVariationEntity existingProjectionEntity, TMainVariationEntity projectionEntity)
        {
            projectionEntity.Variation_Action = existingProjectionEntity.Variation_Action;
        }

        public override string UnifiedValueValidation(TMainVariationEntity entity, string field_name, object newValue)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (field_name == BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Units))
            {
                decimal variation_units = (decimal)newValue;
                if (variation_units < entity.MinNegativeUnits)
                    return "Variation units cannot be lower than " + entity.MinNegativeUnits;
                else if (entity.Variation_Action != VariationAction.Add)
                {
                    VariationAction old_action = entity.Variation_Action;

                    if (variation_units == 0)
                        entity.Variation_Action = VariationAction.NoAction;
                    else
                        entity.Variation_Action = VariationAction.Append;

                    if(MainViewModel != null)
                    {
                        MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Action), old_action, entity.Variation_Action, EntityMessageType.Changed);
                    }
                }
            }
            else
            {
                if (entity.Variation_Action != VariationAction.Add)
                    return "Cannot change variation value on existing items";
            }

            return string.Empty;
        }

        /// <summary>
        /// Intercept MainViewModel Saving because bulk or single selective saving is required
        /// </summary>
        public bool OnBeforeEntitySaved(TMainVariationEntity entity)
        {
            if (entity.EntityKey == Guid.Empty)
            {
                if (!MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation())
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, null, null, null, EntityMessageType.Added);
                entity.VARIATION_ITEM = new VARIATION_ITEM() { ACTION = VariationAction.Add, GUID_VARIATION = loadVARIATION.GUID, VARIATION_UNITS = entity.Variation_Units };
            }

            if (entity.Variation_Action == VariationAction.Add)
                collectionViewModelWrapper.Save(entity.Entity);

            if (entity.ShouldSaveVariation)
                save_variation(entity);

            return true;
        }

        /// <summary>
        /// Delete variation item before entity is deleted
        /// </summary>
        /// <param name="undoRedoEntity"></param>
        public bool OnBeforeEntityDeleted(TMainVariationEntity delete_entity)
        {
            if(!MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation())
                MainViewModel.EntitiesUndoRedoManager.AddUndo(delete_entity, null, null, null, EntityMessageType.Deleted);

            collectionViewModelWrapper.Delete(delete_entity.Entity);
            if (delete_entity.VARIATION_ITEM != null)
                VARIATION_ITEMSCollectionViewModel.Delete(delete_entity.VARIATION_ITEM);

            return false;
        }
        #endregion


        #endregion

        #region Variation Specific
        private void save_variation(TMainVariationEntity projectionEntity)
        {
            if (projectionEntity.VARIATION_ITEM == null)
                projectionEntity.VARIATION_ITEM = new VARIATION_ITEM();

            projectionEntity.VARIATION_ITEM.GUID_VARIATION = loadVARIATION.EntityKey;
            projectionEntity.VARIATION_ITEM.GUID_ORIBASEITEM = projectionEntity.OriginalEntityKey;
            projectionEntity.VARIATION_ITEM.VARIATION_UNITS = projectionEntity.Variation_Units;
            projectionEntity.VARIATION_ITEM.ACTION = projectionEntity.Variation_Action;

            VARIATION_ITEMSCollectionViewModel.Save(projectionEntity.VARIATION_ITEM);
        }

        private CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_ITEMSCollectionViewModel
        {
            get
            {
                return (CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<VARIATION_ITEM>();
            }
        }
        #endregion

        #region View Behavior
        public void CancelDeliverable(TMainVariationEntity projectionEntity)
        {
            if (loadVARIATION == null || loadVARIATION.SUBMITTED != null || loadVARIATION.APPROVED != null)
                return;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            if (projectionEntity.Variation_Action == VariationAction.Add)
                return;

            VariationAction newAction;

            if (projectionEntity.Variation_Action == VariationAction.Cancel)
                newAction = VariationAction.NoAction;
            else
            {
                newAction = VariationAction.Cancel;
                if (projectionEntity.VARIATION_ITEM == null)
                    projectionEntity.VARIATION_ITEM = new VARIATION_ITEM();
            }

            var oldUnits = projectionEntity.Variation_Units;
            projectionEntity.Variation_Units = 0;

            MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Units), oldUnits,
                projectionEntity.Variation_Units, EntityMessageType.Changed);

            var oldAction = projectionEntity.Variation_Action;

            MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Action), oldAction, newAction,
                EntityMessageType.Changed);

            projectionEntity.Variation_Action = newAction;

            MainViewModel.Save(projectionEntity);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            //RefreshSelectedEntity();
            //RefreshView();
            FullRefresh();
        }
        #endregion

        #region View Events
        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, TMainVariationEntity projection, bool isNew)
        {
            if (!isNew && field_name == BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Units))
            {
                if (projection.Variation_Action != VariationAction.Add)
                {
                    VariationAction old_action = projection.Variation_Action;

                    if ((decimal)new_value == 0)
                        projection.Variation_Action = VariationAction.NoAction;
                    else
                        projection.Variation_Action = VariationAction.Append;

                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Action), old_action, projection.Variation_Action, EntityMessageType.Changed);
                }
            }

            collectionViewModelWrapper.UnifiedCellValueChanging(field_name, old_value, new_value, projection.Entity, isNew);
            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }
        #endregion

        #region View Commands
        private bool isSelectedVariationAddEntity()
        {
            if (!DisplaySelectedEntities.All(x => x.Variation_Action == VariationAction.Add))
            {
                MessageBoxService.ShowMessage("Selection contains deliverables that have other statuses than add, please revise your selection");
                return false;
            }

            return true;
        }

        private bool canEditView()
        {
            if (loadVARIATION.SUBMITTED != null || loadVARIATION.APPROVED != null)
            {
                MessageBoxService.ShowMessage("Cannot perform current operation when variation is submitted/approved");
                return false;
            }

            return true;
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit) => collectionViewModelWrapper.CanDuplicateMultiple(barEdit);
        public bool CanInsertMultiple(BarEditItem barEdit) => collectionViewModelWrapper.CanInsertMultiple(barEdit);
        public bool CanDuplicate() => collectionViewModelWrapper.CanDuplicate();
        public bool CanInsert() => collectionViewModelWrapper.CanInsert();
        public bool CanAutoPopulate(object button) => collectionViewModelWrapper.CanAutoPopulate(button);
        public bool CanFindReplace(object button) => collectionViewModelWrapper.CanFindReplace(button);

        public void DuplicateMultiple(BarEditItem barEdit) { if (canEditView()) collectionViewModelWrapper.DuplicateMultiple(barEdit); }
        public void InsertMultiple(BarEditItem barEdit) { if (canEditView()) collectionViewModelWrapper.InsertMultiple(barEdit); }
        public void Duplicate() { if (canEditView()) collectionViewModelWrapper.Duplicate(); }
        public void Insert() { if (canEditView()) collectionViewModelWrapper.Insert(); }
        public void AutoPopulate(object button) { if (isSelectedVariationAddEntity()) collectionViewModelWrapper.AutoPopulate(button); }
        public void FindReplace(object button) { if (isSelectedVariationAddEntity()) collectionViewModelWrapper.FindReplace(button); }
        #endregion

        #region View Properties
        public NewItemRowPosition NewItemRowPosition
        {
            get
            {
                if (loadVARIATION != null && loadVARIATION.SUBMITTED == null)
                    return NewItemRowPosition.Top;

                return NewItemRowPosition.None;
            }
        }


        public IEnumerable<VARIATION_ITEM> VARIATION_ITEMCollection
        {
            get
            {
                return GetEntities<VARIATION_ITEM>();
            }
        }

        protected override void onBeforeDestroy()
        {
            collectionViewModelWrapper.CleanUpEntitiesLoader();
            base.onBeforeDestroy();
        }

        public override void CleanUpEntitiesLoader()
        {
            if(InViewModelOnlyMode)
                collectionViewModelWrapper.CleanUpEntitiesLoader();

            base.CleanUpEntitiesLoader();
        }
        #endregion
    }
}
