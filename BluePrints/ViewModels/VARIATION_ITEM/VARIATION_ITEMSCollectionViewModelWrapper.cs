using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
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
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class VARIATION_ITEMSCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        //Used by view to show hidden workpack columns
        public Action ShowWORKPACKInternalName1;
        public Action ShowWORKPACKInternalName2;


        //Used by variation to generate new baseline
        public Func<object> OnEntitiesLoadedParameterCallBack;
        public Action<IEnumerable<BASELINE_ITEMVariation>, object> OnEntitiesLoadedWithParameterCallBack;

        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATION_ITEMSCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new VARIATION_ITEMSCollectionViewModelWrapper());
        }

        #region Database Operation
        public bool IsPhaseVisible
        {
            get
            {
                if (loadPROJECT == null)
                    return false;

                return !loadPROJECT.USELEGACYWORKPACK;
            }
        }

        private PROJECT loadPROJECT;
        private PROGRESS loadPROGRESS;
        private BASELINE loadBASELINE;
        private VARIATION loadVARIATION;
        private string baseEntityString = "Entity.Entity.Entity.";
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();


        protected override void InitializeParameters(object parameter)
        {
            //both parameters is required because when entity is first added the associating entity (PROJECT) is not loaded
            var receiveParameter =
                (OptionalEntitiesParameter<PROJECT, VARIATION>) parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadVARIATION = receiveParameter.GetSecondEntity();
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

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            //MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => loadBASELINE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc, x => loadVARIATION = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => loadPROGRESS = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadVARIATION.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        private Func<IRepositoryQuery<VARIATION_ITEM>, IQueryable<VARIATION_ITEM>> VARIATION_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_VARIATION == loadVARIATION.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMVariation>>
            ConstructMainViewModelProjection()
        {
            var BASELINE = loaderCollection.GetObject<BASELINE>();
            var PROGRESS = loaderCollection.GetObject<PROGRESS>();
            var VARIATION = loaderCollection.GetObject<VARIATION>();
            var PROGRESS_ITEMS = loaderCollection.GetCollection<PROGRESS_ITEM>();
            var VARIATION_ITEMS = loaderCollection.GetCollection<VARIATION_ITEM>();
            var RATES = loaderCollection.GetCollection<RATE>();
            var DELIVERABLES_STATUSES = loaderCollection.GetCollection<DELIVERABLES_STATUS>();

            return
                query =>
                    Baseline_ItemVariationQuery.OffsiteDirectVariationItemTransformation(query, loadPROJECT, loadPROGRESS, PROGRESS_ITEMS, 
                    loadBASELINE, VARIATION, VARIATION_ITEMS, RATES);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMVariation> entities)
        {
            if(OnEntitiesLoadedWithParameterCallBack != null)
            {
                object onLoadedParameter = OnEntitiesLoadedParameterCallBack?.Invoke();
                OnEntitiesLoadedWithParameterCallBack?.Invoke(entities, onLoadedParameter);

                //Self destruct after entities has been returned
                CleanUpEntitiesLoader();
                return;
            }

            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.ValidateBulkEditCallBack = ValidateBulkEditCallBack;
            MainViewModel.CanBulkDeleteCallBack = CanBulkDeleteCallBack;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = NewProjectionInitializeCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingProjectionEditCallBack;
            MainViewModel.IsContinueSaveCallBack = MainEntityPreSaveWithNewEntityDetection;
            //MainViewModel.OnAfterEntitySavedCallBack = MainEntitySaveVariation;
            MainViewModel.IsContinueNewRowFromViewCallBack = AddVARIATION_ITEMCallBack;
            MainViewModel.OnBeforeBulkEditSaveCallBack = OnBeforeBulkEditSaveCallBack;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = ApplyEntityPropertiesToProjectionCallBack;
            MainViewModel.OnBeforeEntityDeleteCallBack = EntityBeforeDeletionCallBack;
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = CreateNewProjectionFromNewEntity;
            MainViewModel.AdditionalValidateCellCallBack = AdditionalValidateCellCallBack;
            MainViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            //Latest Update: need to remove this line because VARIATION_ITEM save can be invoked directly through Undo/Redo operation
            //Don't refresh on local update because every updates invoke baseline_item and variation save
            //if (sender == VARIATION_ITEMSCollectionViewModel)
            //    return true;

            if (changedType == typeof(VARIATION_ITEM))
            {
                BASELINE_ITEMVariation mainEntity = MainViewModel.Entities.Where(x => x.VARIATION_ITEM != null).FirstOrDefault(x => x.VARIATION_ITEM.GUID.ToString() == key.ToString());
                if (mainEntity != null)
                {
                    //got to make sure sender is not MainViewModel or else it'll not be refreshed
                    mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(mainEntity.EntityKey, EntityMessageType.Changed, this))));
                    return true;
                }
            }

            return false;
        }

        #region CallBacks
        public BASELINE_ITEMVariation CreateNewProjectionFromNewEntity()
        {
            var newVARIATION_ITEM = new BASELINE_ITEMVariation();
            newVARIATION_ITEM.VARIATION_ITEM.ACTION = VariationAction.Add;
            return newVARIATION_ITEM;
        }

        public bool CanFillDownCallBack(IEnumerable<BASELINE_ITEMVariation> selectedEntities, GridMenuInfo info)
        {
            if (loadVARIATION.SUBMITTED != null ||
                !selectedEntities.Any(x => x.VARIATION_ITEM.ACTION == VariationAction.Add))
                return false;

            return true;
        }

        public bool ValidateFillDownCallBack(BASELINE_ITEMVariation fillDownEntity, string fieldName, object fillValue)
        {
            if (fillDownEntity.VARIATION_ITEM.ACTION != VariationAction.Add)
                return false;

            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().Entity)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                var errorMessage = string.Empty;
                MainViewModel.IsValidEntityCellValue(fillDownEntity, fieldName, fillValue, ref errorMessage);
                if (errorMessage != string.Empty)
                    return false;
            }

            return true;
        }

        public bool ValidateBulkEditCallBack(BASELINE_ITEMVariation projection, string fieldName, object editValue)
        {
            if (projection.VARIATION_ITEM.ACTION == VariationAction.Add)
                return true;

            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().Entity)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                var errorMessage = string.Empty;
                MainViewModel.IsValidEntityCellValue(projection, fieldName, editValue, ref errorMessage);
                if (errorMessage != string.Empty)
                    return false;
                else
                    return true;
            }

            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS))
            {
                return true;
            }

            return false;
        }

        private void AdditionalValidateCellCallBack(GridCellValidationEventArgs e)
        {
            //estimated hours field is disabled but just in case
            if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                BASELINE_ITEMVariation validateEntity = (BASELINE_ITEMVariation)e.Row;
                if (validateEntity.Entity.Entity.Entity.BY_DURATION && ((decimal)e.Value) > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Cannot set estimated hours when deliverable is by duration";
                }
            }
            //this is not likely to happen, because variation isn't trackable yet but just in case
            else if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                BASELINE_ITEMVariation validateEntity = (BASELINE_ITEMVariation)e.Row;
                if (validateEntity.Entity.Earned_Units_Total > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Cannot change deliverable tracking type when percentage is already earned";
                }
            }
        }

        public void OnBeforeBulkEditSaveCallBack(BASELINE_ITEMVariation projection, string fieldName)
        {
            if (projection.VARIATION_ITEM.ACTION == VariationAction.Add)
                return;

            string variationItemUnitsFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS);

            string variationItemActionFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().ACTION);

            if (fieldName == variationItemUnitsFieldName)
            {
                if(projection.VARIATION_ITEM.VARIATION_UNITS > 0 && 
                    (projection.VARIATION_ITEM.ACTION == VariationAction.NoAction || projection.VARIATION_ITEM.ACTION == VariationAction.Cancel))
                {
                    VariationAction oldValue = projection.VARIATION_ITEM.ACTION;

                    projection.VARIATION_ITEM.ACTION = VariationAction.Append;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, variationItemActionFieldName, oldValue,
                                VariationAction.Append, EntityMessageType.Changed);
                }
                else if (projection.VARIATION_ITEM.VARIATION_UNITS == 0 && 
                    projection.VARIATION_ITEM.ACTION == VariationAction.Append)
                {
                    projection.VARIATION_ITEM.ACTION = VariationAction.NoAction;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, variationItemActionFieldName, VariationAction.Append,
                                VariationAction.NoAction, EntityMessageType.Changed);
                }
            }
        }

        public bool CanBulkDeleteCallBack(IEnumerable<BASELINE_ITEMVariation> selectedEntities)
        {
            return loadVARIATION.SUBMITTED == null && selectedEntities != null && selectedEntities.All(x => x.VARIATION_ITEM != null && x.VARIATION_ITEM.ACTION == VariationAction.Add);
        }

        public bool AddVARIATION_ITEMCallBack(RowEventArgs e, BASELINE_ITEMVariation projectionEntity)
        {
            projectionEntity.VARIATION_ITEM.ACTION = VariationAction.Add;
            return true;
        }


        public bool NewProjectionInitializeCallBack(BASELINE_ITEMVariation projectionEntity)
        {
            projectionEntity.VARIATION_ITEM.GUID_VARIATION = loadVARIATION.GUID;
            return true;
            //projectionEntity.VARIATION_ITEM.ACTION = VariationAction.Add;
        }

        /// <summary>
        /// Used to determine variation action
        /// </summary>
        public bool ExistingProjectionEditCallBack(BASELINE_ITEMVariation projectionEntity,
            CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName !=
                BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM) + "." +
                BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS))
                return true;

            if (projectionEntity.VARIATION_ITEM.ACTION == VariationAction.Add)
                return true;

            var oldAction = projectionEntity.VARIATION_ITEM.ACTION;

            if (projectionEntity.VARIATION_ITEM.VARIATION_UNITS == 0)
                projectionEntity.VARIATION_ITEM.ACTION = VariationAction.NoAction;
            else
                projectionEntity.VARIATION_ITEM.ACTION = VariationAction.Append;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM) + "." +
                BindableBase.GetPropertyName(() => new VARIATION_ITEM().ACTION), oldAction,
                projectionEntity.VARIATION_ITEM.ACTION, EntityMessageType.Changed);

            return true;
        }

        private bool MainEntityPreSaveWithNewEntityDetection(BASELINE_ITEMVariation projectionEntity, bool isNewEntity)
        {
            if (projectionEntity.VARIATION_ITEM.ACTION == VariationAction.Add)
            {
                if (isNewEntity)
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity, null, null, null,
                        EntityMessageType.Added);
                return true;
            }
            else
            {
                MainEntitySaveVariation(projectionEntity, false);
            }

            return false;
        }

        private void MainEntitySaveVariation(BASELINE_ITEMVariation projectionEntity, bool isNewEntity)
        {
            var saveVARIATION_ITEM = projectionEntity.VARIATION_ITEM;
            saveVARIATION_ITEM.GUID_VARIATION = loadVARIATION.EntityKey;
            saveVARIATION_ITEM.GUID_ORIBASEITEM = projectionEntity.Entity.Entity.OriginalEntityKey;
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (saveVARIATION_ITEM.CREATED.Date.Year == 1)
                saveVARIATION_ITEM.CREATED = DateTime.Now;

            VARIATION_ITEMSCollectionViewModel.Save(saveVARIATION_ITEM);
        }

        protected override void OnBeforeApplyProjectionPropertiesToEntity(BASELINE_ITEMVariation projectionEntity, BASELINE_ITEM entity)
        {
            projectionEntity.Entity.Entity.Entity.GUID_VARIATION = loadVARIATION.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity.Entity);
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        public void ApplyEntityPropertiesToProjectionCallBack(Guid primaryKey, BASELINE_ITEMVariation projectionEntity,
            BASELINE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.Entity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
            MainEntitySaveVariation(projectionEntity, isNewEntity);
        }

        public void EntityBeforeDeletionCallBack(BASELINE_ITEMVariation undoRedoEntity)
        {
            VARIATION_ITEMSCollectionViewModel.Delete(undoRedoEntity.VARIATION_ITEM);
        }
        #endregion

        #endregion

        #region View Behavior
        public void CancelBASELINE_ITEM(BASELINE_ITEMVariation projectionEntity)
        {
            if (loadVARIATION == null || loadVARIATION.SUBMITTED != null || loadVARIATION.APPROVED != null)
                return;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            if (projectionEntity.VARIATION_ITEM.ACTION == VariationAction.Add)
                return;

            VariationAction newAction;

            if (projectionEntity.VARIATION_ITEM.ACTION == VariationAction.Cancel)
                newAction = VariationAction.NoAction;
            else
                newAction = VariationAction.Cancel;

            var oldUnits = projectionEntity.VARIATION_ITEM.VARIATION_UNITS;
            projectionEntity.VARIATION_ITEM.VARIATION_UNITS = 0;

            MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS), oldUnits,
                projectionEntity.VARIATION_ITEM.VARIATION_UNITS, EntityMessageType.Changed);

            var oldAction = projectionEntity.VARIATION_ITEM.ACTION;

            MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().ACTION), oldAction, newAction,
                EntityMessageType.Changed);

            projectionEntity.VARIATION_ITEM.ACTION = newAction;

            MainViewModel.Save(projectionEntity);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            //RefreshSelectedEntity();
            //RefreshView();
            FullRefresh();
        }

        /// <summary>
        /// Allow undo-redo behavior to be added for automated cell value changing. This behavior doesn't have to be applied on new row because AddUndo for EntityMessageType.Added is already handling this
        /// </summary>
        protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        {
            var activeVARIATION_ITEM = (BASELINE_ITEMVariation)e.Row;
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if ((bool)e.Value)
                {
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    string fieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM) + "." +
                                BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS);

                    decimal oldValue = activeVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                    if (oldValue > 0)
                    {
                        activeVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS = 0;
                        MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activeVARIATION_ITEM, fieldName, oldValue, 0, EntityMessageType.Changed);
                    }
                }
            }
            else if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if ((bool)e.Value)
                {
                    decimal oldEstimatedHours = activeVARIATION_ITEM.Entity.Entity.Entity.ESTIMATED_HOURS;
                    decimal oldVariationUnits = activeVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                    if (oldEstimatedHours > 0)
                    {
                        decimal newValue = 0;
                        string estimatedHoursFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().Entity) + "." +
                        BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                        BindableBase.GetPropertyName(() => new BASELINE_ITEM().ESTIMATED_HOURS);
                        activeVARIATION_ITEM.Entity.Entity.Entity.ESTIMATED_HOURS = newValue;
                        MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activeVARIATION_ITEM, estimatedHoursFieldName, oldEstimatedHours, newValue, EntityMessageType.Changed);
                    }

                    if (oldVariationUnits > 0)
                    {
                        decimal newValue = 0;
                        string variationHoursFieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM) + "." +
                        BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS);
                        activeVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS = newValue;
                        MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(activeVARIATION_ITEM, variationHoursFieldName, oldVariationUnits, newValue, EntityMessageType.Changed);
                    }
                }
            }
            else if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA))
            {
                Guid? oldValue = activeVARIATION_ITEM.Entity.Entity.Entity.GUID_SUBAREA;
                if (oldValue != null)
                {
                    Guid? newValue = (Guid?)null;
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBAREA);
                    activeVARIATION_ITEM.Entity.Entity.Entity.GUID_SUBAREA = newValue;
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(activeVARIATION_ITEM, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
            }

            base.CellValueExistingRowChanging(e);
        }

        protected override void CellValueNewRowChanging(CellValueChangedEventArgs e)
        {
            var activeVARIATION_ITEM = (BASELINE_ITEMVariation)e.Row;
            if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA))
            {
                if (e.Value != null)
                {
                    activeVARIATION_ITEM.Entity.Entity.Entity.GUID_AREA = (Guid)e.Value;
                    //Area is required immediately
                    activeVARIATION_ITEM.Entity.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
                    activeVARIATION_ITEM.Update();
                }

                if (activeVARIATION_ITEM.Entity.Entity.Entity.GUID_SUBAREA != null)
                {
                    activeVARIATION_ITEM.Entity.Entity.Entity.GUID_SUBAREA = null;
                }
            }

            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM) + "." + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS))
            {
                MessageBoxService.ShowMessage(BluePrintsResources.Notify_AddBASELINE_ITEMBeforeVARIATION_UNITS);
                e.Handled = true;
                return;
            }

            var activeItem = (BASELINE_ITEMVariation)e.Row;
            if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK))
            {
                var chosenWORKPACK = WORKPACKCollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenWORKPACK != null)
                {
                    activeItem.Entity.Entity = new BASELINE_ITEMProjection();
                    activeItem.Entity.Entity.Entity = new BASELINE_ITEM();
                    activeItem.Entity.Entity.Entity.GUID_AREA = chosenWORKPACK.GUID_DAREA;
                    //Area is required immediately for subarea selection
                    activeItem.Entity.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DAREA);
                    activeItem.Entity.Entity.Entity.GUID_SUBAREA = chosenWORKPACK.GUID_DSUBAREA;
                    activeItem.Entity.Entity.Entity.GUID_DOCTYPE = chosenWORKPACK.GUID_DDOCTYPE;
                    activeItem.Entity.Entity.Entity.GUID_DEPARTMENT = chosenWORKPACK.GUID_DDEPARTMENT;
                    activeItem.Entity.Entity.Entity.GUID_DISCIPLINE = chosenWORKPACK.GUID_DDISCIPLINE;
                    activeItem.Entity.Entity.Entity.GUID_PHASE = chosenWORKPACK.PHASE != null
                        ? chosenWORKPACK.GUID_DPHASE
                        : null;
                    var SelectedAREA = AREACollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DAREA);
                    var SelectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDOCTYPE);
                    var SelectedDISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDISCIPLINE);
                    var BASELINE_ITEMJoinRATES = MainViewModel.Entities.Select(x => x.Entity.Entity).AsEnumerable();

                    activeItem.Entity.Entity.Entity.INTERNAL_NUM = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, BASELINE_ITEMJoinRATES, SelectedAREA, SelectedDISCIPLINE, SelectedDOCTYPE);
                    activeItem.Update();
                }
            }
            else if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
                {
                    if (chosenDOCTYPE.GUID_DDEPARTMENT != null)
                        activeItem.Entity.Entity.Entity.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;

                    //Baseline and Department is required immediately for deliverables status selection
                    activeItem.Entity.Entity.Entity.BASELINE = loadBASELINE;
                    activeItem.Entity.Entity.Entity.DOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
                    activeItem.Update();
                }
            }

            base.CellValueNewRowChanging(e);
        }

        public bool CanDuplicate()
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void Duplicate()
        {
            if (!isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            var BASELINE_ITEMS = MainViewModel.Entities.Select(x => x.Entity.Entity.Entity);
            foreach (var selectedEntity in DisplaySelectedEntities)
            {
                var newProjection = new BASELINE_ITEMVariation();
                DataUtils.ShallowCopy(newProjection.Entity.Entity.Entity, selectedEntity.Entity.Entity.Entity);
                newProjection.Entity.Entity.Entity.GUID = Guid.Empty;
                newProjection.Entity.Entity.Entity.GUID_BASELINE = null;
                newProjection.Entity.Entity.Entity.GUID_ORIGINAL = Guid.Empty;
                newProjection.Entity.Entity.Entity.ESTIMATED_HOURS = 0;
                newProjection.VARIATION_ITEM.ACTION = VariationAction.Add;

                var selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.Entity.GUID_AREA);
                var selectedDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.Entity.GUID_DISCIPLINE);
                var selectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.Entity.GUID_DOCTYPE);
                newProjection.Entity.Entity.Entity.INTERNAL_NUM = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, BASELINE_ITEMS, selectedAREA, selectedDISCIPLINE, selectedDOCTYPE, newProjection.EntityKey);
                MainViewModel.Save(newProjection);
            }

            if (!isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        private bool isProcessingMultipleDuplicates;

        /// <summary>
        /// Paste clipboard data multiple times
        /// </summary>
        public void DuplicateMultiple(BarEditItem barEdit)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            isProcessingMultipleDuplicates = true;
            var timesToDuplicate = 0;
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
                for (var i = 0; i < timesToDuplicate; i++)
                    Duplicate();
            isProcessingMultipleDuplicates = false;
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanAutoPopulate(object button)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0 || MainViewModel.SelectedEntities.All(x => x.VARIATION_ITEM.ACTION != VariationAction.Add))
                return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;

            var departmentFieldName = "Entity.Entity.Entity.GUID_DEPARTMENT";
            var disciplineFieldName = "Entity.Entity.Entity.GUID_DISCIPLINE";
            var docTypeFieldName = "Entity.Entity.Entity.GUID_DOCTYPE";
            var areaFieldName = "Entity.Entity.Entity.GUID_AREA";
            var subAreaFieldName = "Entity.Entity.Entity.SubAreaGuid";
            var workpackFieldName = "Entity.Entity.Entity.GUID_WORKPACK";
            var internalNumberFieldName = "Entity.Entity.Entity.INTERNAL_NUM";

            var entitiesToSave = new List<BASELINE_ITEMVariation>();
            if (info.Column.FieldName == internalNumberFieldName)
                foreach (var entity in MainViewModel.SelectedEntities)
                    entity.Entity.Entity.Entity.INTERNAL_NUM = string.Empty;

            var BASELINE_ITEMS = MainViewModel.Entities.Select(x => x.Entity.Entity.Entity);
            foreach (var entity in MainViewModel.SelectedEntities)
            {
                if (entity.VARIATION_ITEM.ACTION != VariationAction.Add)
                    continue;

                var entityWORKPACK =
                    WORKPACKCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.Entity.GUID_WORKPACK);
                if (info.Column.FieldName == internalNumberFieldName)
                {
                    var internalNum = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, BASELINE_ITEMS, entity.Entity.Entity.Entity.AREA, entity.Entity.Entity.Entity.DISCIPLINE, entity.Entity.Entity.Entity.DOCTYPE, entity.EntityKey);
                    MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, internalNum);
                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == departmentFieldName || info.Column.FieldName == disciplineFieldName ||
                         info.Column.FieldName == docTypeFieldName || info.Column.FieldName == areaFieldName || info.Column.FieldName == subAreaFieldName) 
                {
                    if (entityWORKPACK == null)
                        continue;

                    if (info.Column.FieldName == departmentFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDEPARTMENT);
                    else if (info.Column.FieldName == disciplineFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDISCIPLINE);
                    else if (info.Column.FieldName == docTypeFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDOCTYPE);
                    else if (info.Column.FieldName == areaFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DAREA);
                    else if (info.Column.FieldName == subAreaFieldName && IsPhaseVisible)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DSUBAREA);

                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == workpackFieldName)
                {
                    if (entity.Entity.Entity.Entity.GUID_AREA == Guid.Empty || entity.Entity.Entity.Entity.GUID_DISCIPLINE == Guid.Empty)
                        continue;

                    string internalName = BluePrintsDataUtils.WORKPACK_Generate_InternalNumber2(entity.Entity.Entity.Entity.GUID_AREA, entity.Entity.Entity.Entity.GUID_SUBAREA,
                        loadPROJECT, AREACollection, SUBAREACollection, entity.Entity.Entity.Entity.GUID_PHASE, PHASECollection);

                    if (internalName == string.Empty)
                        return;

                    var findWORKPACK =
                        WORKPACKCollection.FirstOrDefault(
                            x =>
                                x.INTERNAL_NAME1 == internalName);

                    if (findWORKPACK == null)
                    {
                        var newWORKPACK = new WORKPACK();
                        AREA defaultSubArea = SUBAREACollection.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.WorkpackDefaultSubArea);

                        newWORKPACK.GUID_PROJECT = loadPROJECT.GUID;
                        newWORKPACK.GUID_DAREA = entity.Entity.Entity.Entity.GUID_AREA;
                        newWORKPACK.GUID_DSUBAREA = entity.Entity.Entity.Entity.GUID_SUBAREA == null ? defaultSubArea == null ? defaultSubArea.GUID : (Guid?)null : entity.Entity.Entity.Entity.GUID_SUBAREA;
                        newWORKPACK.GUID_DPHASE = entity.Entity.Entity.Entity.GUID_PHASE;
                        if (entity.Entity.Entity.Entity.GUID_DISCIPLINE != null)
                            newWORKPACK.GUID_DDISCIPLINE = (Guid)entity.Entity.Entity.Entity.GUID_DISCIPLINE;
                        if (entity.Entity.Entity.Entity.GUID_DEPARTMENT != null)
                            newWORKPACK.GUID_DDEPARTMENT = (Guid)entity.Entity.Entity.Entity.GUID_DEPARTMENT;
                        if (entity.Entity.Entity.Entity.GUID_DOCTYPE != null)
                            newWORKPACK.GUID_DDOCTYPE = (Guid)entity.Entity.Entity.Entity.GUID_DOCTYPE;

                        newWORKPACK.INTERNAL_NAME1 = internalName;
                        newWORKPACK.STARTDATE = DateTime.Now;
                        newWORKPACK.ENDDATE =
                            BluePrintsDataUtils.WORKPACK_Calculate_EndDate((DateTime)newWORKPACK.STARTDATE, loadPROJECT);
                        var reviewStartDate = (DateTime)newWORKPACK.STARTDATE;
                        var reviewEndDate = (DateTime)newWORKPACK.ENDDATE;
                        BluePrintsDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate,
                            loadPROJECT, false);
                        newWORKPACK.REVIEWSTARTDATE = reviewStartDate;
                        newWORKPACK.REVIEWENDDATE = reviewEndDate;
                        newWORKPACK.AUTOGENERATED = true;
                        newWORKPACK.TYPE = WorkpackType.OffsiteDirect;
                        ((CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)
                            loaderCollection.GetViewModel<WORKPACK>()).Save(newWORKPACK);

                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, newWORKPACK.GUID);
                    }
                    else
                    {
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, findWORKPACK.GUID);
                    }

                    entitiesToSave.Add(entity);
                }
            }

            MainViewModel.BulkSave(entitiesToSave);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        #endregion

        #region View Properties
        protected override string ExportExcelFilename()
        {
            return loadPROJECT.NUMBER + "_" + loadVARIATION.NAME + ".xlsx";
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "VARIATION_ITEMSViewModelWrapper"; }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
                return collection;
            }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                return GetSUBAREACollection();
            }
        }

        public IEnumerable<AREA> GetSUBAREACollection()
        {
            var collection = GetEntities<AREA>();
            if (collection != null)
                collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
            return collection;
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
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

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        private CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
            variation_itemsCollectionViewModel;

        private CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
            VARIATION_ITEMSCollectionViewModel
        {
            get
            {
                if (variation_itemsCollectionViewModel == null)
                    if (MainViewModel != null)
                        variation_itemsCollectionViewModel =
                            (CollectionViewModel<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                            loaderCollection.GetViewModel<VARIATION_ITEM>();

                return variation_itemsCollectionViewModel;
            }
        }
        #endregion

    }
}