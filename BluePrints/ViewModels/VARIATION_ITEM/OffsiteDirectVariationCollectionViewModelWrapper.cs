using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class OffsiteDirectVariationCollectionViewModelWrapper : BASELINE_ITEMCollectionViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectVariationCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectVariationCollectionViewModelWrapper());
        }

        protected VARIATION loadVARIATION;
        protected override void resolveParameters(object parameter)
        {            
            var receiveParameter = (DualEntitiesParameter<PROJECT, VARIATION>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadVARIATION = receiveParameter.GetSecondEntity();
            viewType = DeliverablesViewType.Both;
            isQueryForLiveStatus = true;
            //base.resolveParameters(parameter);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
            base.addEntitiesLoader();
        }

        private Func<IRepositoryQuery<VARIATION_ITEM>, IQueryable<VARIATION_ITEM>> VARIATION_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_VARIATION == loadVARIATION.GUID);
        }

        protected override string ViewName => "DESIGN_VARIATION_ITEMSViewModelWrapper_v3" + loadPROJECT == null ? Guid.Empty.ToString() : loadPROJECT.GUID.ToString();

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>> specifyMainViewModelProjection()
        {
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS = new List<VARIATION_ITEM>();
            if (loaderCollection != null)
            {
                VARIATION_ITEMS = loaderCollection.GetCollection<VARIATION_ITEM>();
            }
            
            return query => ProgressQueries.OffsiteDirectVariationItemTransformation(baseQueryFilter(query), loadPROJECT, livePROGRESS, PROGRESS_ITEMCollection, loadBASELINE, VARIATIONCollection, loadVARIATION, VARIATION_ITEMS, RATECollection);
        }

        protected override IQueryable<BASELINE_ITEM> baseQueryFilter(IRepositoryQuery<BASELINE_ITEM> query)
        {
            if (loadVARIATION.APPROVED == null)
                //When variation is not approved, retrieve current live deliverables and variation deliverables
                //Also x.GUID_VARIATION != loadVARIATION.GUID prevents deliverable from getting shown twice due to it not being removed from the live deliverable's list because other variation has units on it
                return query.Where(x => (x.GUID_BASELINE == load_context_guid && x.GUID_VARIATION != loadVARIATION.GUID) || (x.GUID_VARIATION == loadVARIATION.GUID && x.GUID_BASELINE == null));
            else
                //When variation is approved, retrieve deliverables from variation connected baseline
                return query.Where(x => x.GUID_BASELINE == loadVARIATION.GUID_BASELINE);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if(changedType == typeof(VARIATION_ITEM))
            {
                VARIATION_ITEM variation_item = VARIATION_ITEMSCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
                if(variation_item != null)
                {
                    BASELINE_ITEMProgress projection = DisplayEntities.FirstOrDefault(x => x.GUID_ORIGINAL == variation_item.GUID_ORIBASEITEM);
                    if (projection != null)
                        projection.Update();
                }
            }

            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }

        protected override void OnBeforeApplyProjectionPropertiesToEntity(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity)
        {
            //not attaching to baseline when deliverable is added through variation list
            if (projectionEntity.GUID_BASELINE == null)
                projectionEntity.GUID_VARIATION = loadVARIATION.GUID;

            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);

            //not calling base here because we do not want to assign baseline_guid;
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProgress projection, string field_name, object newValue)
        {
            //budgeted hours field is disabled but just in case
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Variation_Units)))
            {
                if (projection.Entity.Entity.BY_DURATION && ((decimal)newValue) > 0)
                    return "Cannot set variation hours when deliverable is by duration";
            }
            //this is not likely to happen, because variation isn't trackable yet but just in case
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION)))
            {
                if (projection.Earned_Units_Total > 0)
                    return "Cannot change deliverable tracking type when percentage is already earned";
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE)))
            {
                if(projection.Entity.Entity.GUID_PHASE != null && newValue != null)
                {
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == (Guid)newValue);
                    PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_PHASE);
                    if ((findPHASE != null && findDOCTYPE != null) && findDOCTYPE.IS_INDIRECT_ONLY && findPHASE.CHARGE_TYPE == ChargeType.Direct)
                        return "Selected document type is valid for indirect only, please change phase to indirect";
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_PHASE)))
            {
                if (projection.Entity.Entity.GUID_DOCTYPE != null && newValue != null)
                {
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DOCTYPE);
                    PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.GUID == (Guid)newValue);
                    if ((findPHASE != null && findDOCTYPE != null) && findDOCTYPE.IS_INDIRECT_ONLY && findPHASE.CHARGE_TYPE == ChargeType.Direct)
                        return "Selected document type is valid for indirect only, please change phase to indirect";
                }
            }

            return base.UnifiedValueValidation(projection, field_name, newValue);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            if (!isNew && field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationUnits))
            {
                if (projection.DisplayVariationAction != VariationAction.Add)
                {
                    VariationAction old_action = projection.DisplayVariationAction;

                    if ((decimal)new_value == 0)
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationAction), projection.DisplayVariationAction, VariationAction.NoAction,
                        EntityMessageType.Changed);

                        projection.DisplayVariationAction = VariationAction.NoAction;
                    }
                    else
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationAction), projection.DisplayVariationAction, VariationAction.Append,
                        EntityMessageType.Changed);

                        projection.DisplayVariationAction = VariationAction.Append;
                    }
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION)))
            {
                decimal oldValue = projection.Variation_Units;
                decimal newValue = 0;
                projection.DisplayVariationUnits = newValue;
                if(!isNew)
                {
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationUnits), oldValue, newValue, EntityMessageType.Changed);
                }
                else
                    projection.Update();
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        #region Tag saving behavior
        public void BulkDelete(IEnumerable<BASELINE_ITEMProgress> selectedEntities)
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to delete " + displaySelectedEntities.Count + " selected entries?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            if (!DisplaySelectedEntities.Any(x => x.DisplayVariationAction == VariationAction.Add))
            {
                MessageBoxService.ShowMessage("Selection contains deliverable(s) that aren't variation, you can only delete deliverable that is added through variation", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return;
            }

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.BaseBulkDelete(displaySelectedEntities);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public override bool OnBeforeEntitySaved(BASELINE_ITEMProgress entity)
        {
            //do not allow modification to existing deliverables
            if (entity.GUID != Guid.Empty && entity.DisplayVariationAction != VariationAction.Add)
            {
                //only save variation units
                if (entity.ShouldSaveVariation)
                    saveVariation(entity);

                return false;
            }

            return base.OnBeforeEntitySaved(entity);
        }

        public override void OnEntitiesSavedCallBack(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity, bool isNewEntity)
        {
            //copy the original entity key to projection first before saving variation
            base.OnEntitiesSavedCallBack(projectionEntity, entity, isNewEntity);

            if (isNewEntity)
                projectionEntity.DisplayVariationAction = VariationAction.Add;

            if (projectionEntity.ShouldSaveVariation)
                saveVariation(projectionEntity);
        }

        protected override void onBeforeEntitiesDuplicated(BASELINE_ITEMProgress copyEntity, BASELINE_ITEMProgress newEntity)
        {
            newEntity.DisplayVariationUnits = copyEntity.DisplayVariationUnits;
            //cannot put VariationAction.Add here because OnBeforeEntitySaved will skip this entity
            //newEntity.DisplayVariationAction = VariationAction.Add;
            newEntity.Entity.Entity.BUDGET_HOURS = 0;

            base.onBeforeEntitiesDuplicated(copyEntity, newEntity);
        }

        /// <summary>
        /// Delete variation item before entity is deleted
        /// </summary>
        /// <param name="undoRedoEntity"></param>
        protected override DeleteInterceptMode onBeforeEntitiesDeleted(BASELINE_ITEMProgress delete_entity)
        {
            if (!MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation())
                MainViewModel.EntitiesUndoRedoManager.AddUndo(delete_entity, null, null, null, EntityMessageType.Deleted);

            //don't delete the varation item just yet because user might be redoing later
            if (delete_entity.VARIATION_ITEM != null && !MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation())
                VARIATION_ITEMSCollectionViewModel.Delete(delete_entity.VARIATION_ITEM);

            return DeleteInterceptMode.Continue;
        }

        public void CancelDeliverable(BASELINE_ITEMProgress projectionEntity)
        {
            if (loadVARIATION == null || loadVARIATION.SUBMITTED != null || loadVARIATION.APPROVED != null)
                return;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            if (projectionEntity.DisplayVariationAction == VariationAction.Add)
                return;

            VariationAction newAction;
            if (projectionEntity.DisplayVariationAction == VariationAction.Cancel)
                newAction = VariationAction.NoAction;
            else
                newAction = VariationAction.Cancel;

            var oldUnits = projectionEntity.DisplayVariationUnits;
            projectionEntity.DisplayVariationUnits = 0;

            MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationUnits), oldUnits,
                projectionEntity.DisplayVariationUnits, EntityMessageType.Changed);

            var oldAction = projectionEntity.DisplayVariationAction;

            MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationAction), oldAction, newAction,
                EntityMessageType.Changed);

            projectionEntity.DisplayVariationAction = newAction;

            MainViewModel.Save(projectionEntity);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();

            projectionEntity.Update();
        }
        #endregion

        #region Variation Specific
        private void saveVariation(BASELINE_ITEMProgress projectionEntity)
        {
            VARIATION_ITEM updateVARIATION_ITEM = projectionEntity.UpdateVariationItem(loadVARIATION.GUID);
            VARIATION_ITEMSCollectionViewModel.Save(updateVARIATION_ITEM);
        }
        #endregion

        #region View Property
        protected override void OnClose(CancelEventArgs e)
        {
            Messenger.Default.Send(new EntityMessage<VARIATION, Guid>(loadVARIATION.GUID, MainViewModel.Key, EntityMessageType.Changed, this, CurrentHWID, false));
            base.OnClose(e);
        }

        public NewItemRowPosition NewItemRowPosition
        {
            get
            {
                if (loadVARIATION != null && loadVARIATION.SUBMITTED == null)
                    return NewItemRowPosition.Top;

                return NewItemRowPosition.None;
            }
        }

        private CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_ITEMSCollectionViewModel
        {
            get
            {
                return (CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<VARIATION_ITEM>();
            }
        }
        #endregion
    }
}