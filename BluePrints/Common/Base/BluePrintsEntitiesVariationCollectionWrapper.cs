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
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Base
{
    public abstract class BluePrintsEntitiesVariationCollectionWrapper<TMainEntity, TMainReportableEntity, TMainVariationEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : BluePrintsEntitiesCollectionWrapper<TMainEntity, TMainVariationEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>
        where TMainEntity : class, IDeliverable, new()
        where TMainReportableEntity : class, IReportable, ISupportVariation, new()
        where TMainVariationEntity : class, IBluePrintsVariationBase<TMainReportableEntity>, new()
        where TMainEntityUnitOfWork : IBluePrintsEntitiesUnitOfWork
    {
        protected abstract IDeliverableCollectionViewModelWrapper<TMainReportableEntity, TMainEntity> collectionViewModelWrapper { get; }

        protected PROJECT loadPROJECT;
        protected VARIATION loadVARIATION;
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected string baseEntityString = "Entity.Entity.Entity.";
        protected override void InitializeParameters(object parameter)
        {
            //both parameters is required because when entity is first added the associating entity (PROJECT) is not loaded
            var receiveParameter =
                (DualEntitiesParameter<PROJECT, VARIATION>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadVARIATION = receiveParameter.GetSecondEntity();

            collectionViewModelWrapper.SelectedEntities = DisplaySelectedEntities.Select(x => x.ReportableEntity);
            collectionViewModelWrapper.OnEntitiesLoadedCallBack = onViewModelWrapperLoadedCallBack;
            collectionViewModelWrapper.ApplyViewSpecificPropertiesToEntityCallBack = ApplyViewSpecificPropertiesToEntity;
            collectionViewModelWrapper.SetParentViewModel(this);
            collectionViewModelWrapper.Interface_InitializeParameters(new DualEntitiesParameter<PROJECT, BASELINE>(loadPROJECT, null));
            collectionViewModelWrapper.InitializeAndLoadEntitiesLoaderDescription();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            //MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc, x => loadVARIATION = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
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
        protected void onViewModelWrapperLoadedCallBack(IEnumerable<TMainReportableEntity> entities)
        {
            IReportableEntitiesCollection = entities;
            OnAllEntitiesCollectionLoaded();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            if (!iReportableCollectionLoaded)
                return;

            StartCreatingMainViewModel();
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected abstract void StartCreatingMainViewModel();

        //Used by variation to generate new baseline
        public Func<object> OnEntitiesLoadedParameterCallBack;
        public Action<IEnumerable<TMainVariationEntity>, object> OnEntitiesLoadedWithParameterCallBack;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainVariationEntity> entities)
        {
            if (OnEntitiesLoadedWithParameterCallBack != null)
            {
                object onLoadedParameter = OnEntitiesLoadedParameterCallBack?.Invoke();
                OnEntitiesLoadedWithParameterCallBack?.Invoke(entities, onLoadedParameter);

                //Self destruct after entities has been returned
                CleanUpEntitiesLoader();
                return;
            }

            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.CanBulkDeleteCallBack = CanBulkDeleteCallBack;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnBeforeEntityDeleteCallBack = OnBeforeEntityDeleted;
            MainViewModel.OnMappingAdditionalChangedEntitiesProperties = OnMappingAdditionalChangedEntitiesProperties;
            assign_additional_callbacks(MainViewModel);
            MainViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void ApplyViewSpecificPropertiesToEntity(TMainReportableEntity reportableEntity)
        {
            if(reportableEntity.Baseline_Guid == null)
            {
                reportableEntity.Variation_Guid = loadVARIATION.GUID;
            }
        }

        protected abstract void assign_additional_callbacks(CollectionViewModel<TMainEntity, TMainVariationEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork> mainViewModel);

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
                        affectedDisplayEntity.Update();
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
            if (loadVARIATION.SUBMITTED != null || !selectedEntities.Any(x => x.Variation_Action == VariationAction.Add))
                return false;

            return true;
        }

        /// <summary>
        /// fill down can only be performed on newly added entity
        /// </summary>
        private bool ValidateFillDownCallBack(TMainVariationEntity fillDownEntity, string fieldName, object fillValue)
        {
            if (fillDownEntity.Variation_Action != VariationAction.Add)
                return false;

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
            return loadVARIATION.SUBMITTED == null && selectedEntities != null && selectedEntities.All(x => x.VARIATION_ITEM != null && x.Variation_Action == VariationAction.Add);
        }

        #region Fundamentals

        protected void OnMappingAdditionalChangedEntitiesProperties(TMainVariationEntity existingProjectionEntity, TMainVariationEntity projectionEntity)
        {
            projectionEntity.Variation_Action = existingProjectionEntity.Variation_Action;
        }

        /// <summary>
        /// Intercept MainViewModel Saving because bulk or single selective saving is required
        /// </summary>
        public bool OnBeforeEntitySaved(TMainVariationEntity entity)
        {
            if (entity.EntityKey == Guid.Empty)
                entity.VARIATION_ITEM = new VARIATION_ITEM() { ACTION = VariationAction.Add, GUID_VARIATION = loadVARIATION.GUID, VARIATION_UNITS = entity.Variation_Units };

            if(entity.Variation_Action == VariationAction.Add)
                collectionViewModelWrapper.Save(entity.ReportableEntity);

            if (entity.ShouldSaveVariation)
                save_variation(entity);

            return false;
        }

        /// <summary>
        /// Delete variation item before entity is deleted
        /// </summary>
        /// <param name="undoRedoEntity"></param>
        public void OnBeforeEntityDeleted(TMainVariationEntity undoRedoEntity)
        {
            VARIATION_ITEMSCollectionViewModel.Delete(undoRedoEntity.VARIATION_ITEM);
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
                return (CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>) loaderCollection.GetViewModel<VARIATION_ITEM>();
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
        /// <summary>
        /// allow undo-redo behavior to be added for automated cell value changing. This behavior doesn't have to be applied on new row because AddUndo for EntityMessageType.Added is already handling this
        /// </summary>
        protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        {
            TMainVariationEntity current_row_item = (TMainVariationEntity)e.Row;
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Units))
            {
                VariationAction old_action = current_row_item.Variation_Action;

                if ((decimal)e.Value == 0)
                    current_row_item.Variation_Action = VariationAction.NoAction;
                else
                    current_row_item.Variation_Action = VariationAction.Append;

                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                MainViewModel.EntitiesUndoRedoManager.AddUndo(current_row_item, BindableBase.GetPropertyName(() => new TMainVariationEntity().Variation_Action), old_action, current_row_item.Variation_Action, EntityMessageType.Changed);
            }
            else
                collectionViewModelWrapper.Interface_CellValueExistingRowChanging(e.Column.FieldName, e.Value, current_row_item.ReportableEntity);

            base.CellValueExistingRowChanging(e);
        }

        protected override void CellValueNewRowChanging(CellValueChangedEventArgs e)
        {
            collectionViewModelWrapper.Interface_CellValueNewRowChanging(e.Column.FieldName, e.Value, ((TMainVariationEntity)e.Row).ReportableEntity);
            base.CellValueNewRowChanging(e);
        }

        /// <summary>
        /// Refresh all min max units for converter to do estimated hours validation
        /// </summary>
        public void CellValueChanged(CellValueChangedEventArgs e)
        {
            collectionViewModelWrapper.Interface_CellValueChanged(e.Column.FieldName, ((TMainVariationEntity)e.Row).ReportableEntity);
        }
        #endregion

        #region View Commands
        public bool CanDuplicateMultiple(BarEditItem barEdit) => collectionViewModelWrapper.CanDuplicateMultiple(barEdit);
        public bool CanInsertMultiple(BarEditItem barEdit) => collectionViewModelWrapper.CanInsertMultiple(barEdit);
        public bool CanDuplicate() => collectionViewModelWrapper.CanDuplicate();
        public bool CanInsert() => collectionViewModelWrapper.CanInsert();
        public bool CanAutoPopulate(object button)
        {
            if (DisplaySelectedEntity == null || DisplaySelectedEntity.Variation_Action != VariationAction.Add)
                return false;

            return collectionViewModelWrapper.CanAutoPopulate(button);
        }

        public void DuplicateMultiple(BarEditItem barEdit) => collectionViewModelWrapper.DuplicateMultiple(barEdit);
        public void InsertMultiple(BarEditItem barEdit) => collectionViewModelWrapper.InsertMultiple(barEdit);
        public void Duplicate() => collectionViewModelWrapper.Duplicate();
        public void Insert() => collectionViewModelWrapper.Insert();
        public void AutoPopulate(object button) => collectionViewModelWrapper.AutoPopulate(button);
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
        #endregion
    }
}
