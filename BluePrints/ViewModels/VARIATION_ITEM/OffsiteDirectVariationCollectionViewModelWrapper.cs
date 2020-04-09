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
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
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
        public Action<bool> SetViewCanEdit;
        public bool CanEditView { get; set; }
        protected override void resolveParameters(object parameter)
        {            
            var receiveParameter = (DualEntitiesParameter<PROJECT, VARIATION>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            loadVARIATION = receiveParameter.GetSecondEntity();
            viewType = DeliverablesViewType.Both;
            isQueryForLiveStatus = true;
            CanEditView = true;
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

        public override string ViewName => "DESIGN_VARIATION_ITEMSViewModelWrapper_v3" + getUniqueViewId();

        private string getUniqueViewId()
        {
            if (loadVARIATION == null)
                return string.Empty;

            return loadVARIATION.GUID.ToString();
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>> specifyMainViewModelProjection()
        {
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS = new List<VARIATION_ITEM>();
            if (loaderCollection != null)
            {
                VARIATION_ITEMS = loaderCollection.GetCollection<VARIATION_ITEM>();
            }
            
            return query => ProgressQueries.OffsiteDirectVariationItemTransformation(baseQueryFilter(query), loadPROJECT, livePROGRESS, PROGRESS_ITEMCollection, loadBASELINE, VARIATIONCollection, loadVARIATION, VARIATION_ITEMS, RATECollection, DOCTYPECollection, COMMODITY_CODECollection);
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
            if (loadVARIATION.SUBMITTED != null || loadVARIATION.APPROVED != null)
            {
                CanEditView = false;
                this.RaisePropertyChanged(x => x.CanEditView);
                SetViewCanEdit?.Invoke(false);
            }
            else
            {
                CanEditView = true;
                this.RaisePropertyChanged(x => x.CanEditView);
                SetViewCanEdit?.Invoke(true);
            }

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        //required to refresh row after background undo/redo operation
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if(changedType == typeof(VARIATION_ITEM))
            {
                VARIATION_ITEM variation_item = VARIATION_ITEMSCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == (Guid)key);
                if(variation_item != null)
                {
                    BASELINE_ITEMProgress projection = Entities.FirstOrDefault(x => x.GUID_ORIGINAL == variation_item.GUID_ORIBASEITEM);
                    if (projection != null)
                        projection.Update();
                }
            }

            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }

        protected override bool OnBeforeApplyingProjectionPropertiesToEntityIsContinue(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity)
        {
            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            return false;
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProgress projection, string field_name, object newValue, bool isPaste)
        {
            //budgeted hours field is disabled but just in case
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationUnits)))
            {
                if (projection.Entity.Entity.BY_DURATION && ((decimal)newValue) > 0)
                    return "Cannot set variation hours when deliverable is by duration";
                if (!projection.IsSubmitted && !projection.IsApproved)
                    return string.Empty;
            }

            if (projection.IsReadOnly)
                return "Cannot edit baseline deliverables";
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
                    if ((findPHASE != null && findDOCTYPE != null) && findDOCTYPE.IS_INDIRECT_ONLY && findPHASE.PHASE_TYPE != PhaseType.Indirect)
                        return "Selected document type is valid for indirect only, please change phase to indirect";
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_PHASE)))
            {
                if (projection.Entity.Entity.GUID_DOCTYPE != null && newValue != null)
                {
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DOCTYPE);
                    PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.GUID == (Guid)newValue);
                    if ((findPHASE != null && findDOCTYPE != null) && findDOCTYPE.IS_INDIRECT_ONLY && findPHASE.PHASE_TYPE != PhaseType.Indirect)
                        return "Selected document type is valid for indirect only, please change phase to indirect";
                }
            }

            return base.UnifiedValueValidation(projection, field_name, newValue, isPaste);
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            if (!isNew && field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationUnits))
            {
                if (projection.DisplayVariationAction != VariationAction.Add)
                {
                    VariationAction old_action = projection.DisplayVariationAction;

                    if ((decimal)new_value == 0)
                    {
                        projection.DisplayVariationAction = VariationAction.NoAction;
                    }
                    else
                    {
                        projection.DisplayVariationAction = VariationAction.Append;
                    }
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION)))
            {
                decimal oldValue = projection.Variation_Units;
                decimal newValue = 0;
                projection.DisplayVariationUnits = newValue;
                if (!isNew)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DisplayVariationUnits), oldValue, newValue, EntityMessageType.Changed);
                }
                else
                    projection.Update();
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        #region Tag saving behavior
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(BASELINE_ITEMProgress projection, out bool isNew)
        {
            //not attaching to baseline when deliverable is added through variation list
            if (projection.GUID_BASELINE == null)
                projection.GUID_VARIATION = loadVARIATION.GUID;

            isNew = false;
            //do not allow modification to existing deliverables
            if (projection.GUID != Guid.Empty && projection.DisplayVariationAction != VariationAction.Add)
            {
                //only save variation units
                if (projection.ShouldSaveVariation)
                    saveVariation(projection);

                return OperationInterceptMode.SkipOneAndAllDbSaves;
            }

            if (projection.IsReadOnly)
            {
                isNew = false;
                return OperationInterceptMode.SkipOneAndAllDbSaves;
            }

            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
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

        protected override bool canDeleteDeliverable(VARIATION_ITEM variation_item, BASELINE_ITEMProgress deliverable)
        {
            return variation_item.GUID_ORIBASEITEM == deliverable.GUID_ORIGINAL && variation_item.ACTION != VariationAction.NoAction && variation_item.GUID_VARIATION != loadVARIATION.GUID;
        }

        /// <summary>
        /// Delete variation item before entity is deleted
        /// </summary>
        protected override OperationInterceptMode OnBeforeProjectionDeleteIsContinue(BASELINE_ITEMProgress projection, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            if (projection.IsReadOnly)
            {
                errorMessages.Add(new ErrorMessage(projection.Deliverable_Name, "Cannot delete baseline deliverable"));
                return OperationInterceptMode.SkipOneAndAllDbSaves;
            }

            //don't delete the varation item just yet because user might be redoing later
            if (projection.VARIATION_ITEM != null && !MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation)
                VARIATION_ITEMSCollectionViewModel.Delete(projection.VARIATION_ITEM);

            return base.OnBeforeProjectionDeleteIsContinue(projection, out errorMessages);
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