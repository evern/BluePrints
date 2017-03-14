using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using BluePrints.Data.Helpers;
using BluePrints.Common;
using DevExpress.Xpf.Grid;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Common.ViewModel.UndoRedo;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using System.Windows.Threading;
using System.Windows;
using DevExpress.Xpf.Bars;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class VARIATION_ITEMSViewModelWrapper :
        CollectionViewModelsWrapper
        <BASELINE_ITEM, VARIATION_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<BASELINE_ITEM, VARIATION_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        public Action ShowWORKPACKInternalName1;
        public Action ShowWORKPACKInternalName2;

        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATION_ITEMSViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new VARIATION_ITEMSViewModelWrapper());
        }

        #region Database Operation

        private PROJECT loadPROJECT;
        private PROGRESS loadPROGRESS;
        private BASELINE loadBASELINE;
        private VARIATION loadVARIATION;

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

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0,
                bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, null,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(1,
                bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, typeof(PROJECT),
                isContinueLoadingAfterBASELINE, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork>(2,
                bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc, typeof(PROJECT),
                isContinueLoadingAfterVARIATION, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(3,
                bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, typeof(PROJECT),
                isContinueLoadingAfterPROGRESS, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(4,
                bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, typeof(PROGRESS), null, null,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<VARIATION_ITEM, VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(5,
                bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc, typeof(VARIATION),
                null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(6,
                bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(7,
                bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>(8,
                bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(9,
                bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(10,
                bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(11,
                bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(12,
                bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROJECT), null, null,
                OnAfterEntitiesChanged);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            loadPROJECT = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "BASELINE"))));
                return false;
            }

            loadBASELINE = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterVARIATION(IEnumerable<VARIATION> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "VARIATION"))));
                return false;
            }

            loadVARIATION = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterPROGRESS(IEnumerable<PROGRESS> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROGRESS"))));
                return false;
            }

            loadPROGRESS = entities.First();
            return true;
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
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<VARIATION_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            var getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            var getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            var getVARIATIONFunc = loaderCollection.GetObjectFunc<VARIATION>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getVARIATION_ITEMSFunc =
                loaderCollection.GetCollectionFunc<VARIATION_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var submittedDate = loadVARIATION.SUBMITTED;

            return
                query =>
                    VARIATION_ITEMProjectionQuery.JoinRATESAndPROGRESS_ITEMSAndVARIATION_ITEMSOnBASELINE_ITEMS(query,
                        getPROGRESSFunc, getBASELINEFunc, getVARIATIONFunc, getPROGRESS_ITEMSFunc,
                        getVARIATION_ITEMSFunc, getRATESFunc, submittedDate != null);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION_ITEMProjection> entities)
        {
            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.ValidateBulkEditCallBack = ValidateBulkEditCallBack;
            MainViewModel.CanBulkDeleteCallBack = CanBulkDeleteCallBack;
            MainViewModel.SetParentAssociationCallBack = NewProjectionInitializeCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingProjectionEditCallBack;
            MainViewModel.IsContinueSaveCallBack = MainEntityPreSaveWithNewEntityDetection;
            MainViewModel.OnAfterEntitySavedCallBack = MainEntitySaveVariation;
            MainViewModel.IsContinueNewRowFromViewCallBack = AddVARIATION_ITEMCallBack;
            //MainViewModel.BulkPreSave = this.MainEntityBulkPreSave;
            //MainViewModel.BulkPostSave = this.MainEntityBulkPostSave;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntityCallBack;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitiesSavedCallBack;
            MainViewModel.OnBeforeEntityDeleteCallBack = EntityBeforeDeletionCallBack;
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = CreateNewProjectionFromNewEntity;
            MainViewModel.SetParentViewModel(this);

            mainThreadDispatcher.BeginInvoke(new Action(() => ShowWORKPACKColumns()));
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            if (sender == MainViewModel && messageType != EntityMessageType.Added || sender == this)
                return;

            //Map the changes from PROGRESS_ITEM to BASELINE_ITEM so undo/redo operation is valid
            //if (changedType == typeof(VARIATION_ITEM) && messageType != EntityMessageType.Added)
            //{
            //    var mappedEntity =
            //        MainViewModel.Entities.FirstOrDefault(
            //            x => x.VARIATION_ITEM != null && x.VARIATION_ITEM.GUID.ToString() == key.ToString());
            //    mainThreadDispatcher.BeginInvoke(
            //        new Action(
            //            () =>
            //                Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(mappedEntity.GUID,
            //                    EntityMessageType.Changed, this))));
            //    return;
            //}

            if (loadPROGRESS != null && changedType == typeof(PROGRESS) &&
                loadPROGRESS.GUID.ToString() == key.ToString() ||
                loadBASELINE != null && changedType == typeof(BASELINE) &&
                loadBASELINE.GUID.ToString() == key.ToString() ||
                loadVARIATION != null && changedType == typeof(VARIATION) &&
                loadVARIATION.GUID.ToString() == key.ToString() ||
                loadPROJECT != null && changedType == typeof(PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored,
                        StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                        StringFormatUtils.GetEntityNameByType(changedType)));

            if (loadPROJECT != null || loadBASELINE != null || loadPROGRESS != null || loadVARIATION != null)
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));
                else if (loadPROJECT != null || loadBASELINE != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));

            base.OnAfterEntitiesChanged(key, changedType, messageType, sender);
        }

        #region CallBacks

        public VARIATION_ITEMProjection CreateNewProjectionFromNewEntity()
        {
            var newVARIATION_ITEM = new VARIATION_ITEMProjection();
            newVARIATION_ITEM.VARIATION_ITEM.ACTION = VariationAction.Add;
            return newVARIATION_ITEM;
        }

        public bool CanFillDownCallBack(IEnumerable<VARIATION_ITEMProjection> selectedEntities, GridMenuInfo info)
        {
            if (loadVARIATION.SUBMITTED != null ||
                !selectedEntities.Any(x => x.VARIATION_ITEM.ACTION == VariationAction.Add))
                return false;

            return true;
        }

        public bool ValidateFillDownCallBack(VARIATION_ITEMProjection fillDownEntity, string fieldName, object fillValue)
        {
            if (fillDownEntity.VARIATION_ITEM.ACTION != VariationAction.Add)
                return false;

            if (fieldName == BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().BASELINE_ITEMJoinRATE)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM)
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

        public bool ValidateBulkEditCallBack(VARIATION_ITEMProjection fillDownEntity, string fieldName, object editValue)
        {
            if (fillDownEntity.VARIATION_ITEM.ACTION != VariationAction.Add)
                return false;

            if (fieldName == BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().BASELINE_ITEMJoinRATE)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                var errorMessage = string.Empty;
                MainViewModel.IsValidEntityCellValue(fillDownEntity, fieldName, editValue, ref errorMessage);
                if (errorMessage != string.Empty)
                    return false;
            }

            return true;
        }

        public bool CanBulkDeleteCallBack(IEnumerable<VARIATION_ITEMProjection> selectedEntities)
        {
            return loadVARIATION.SUBMITTED == null && selectedEntities != null && selectedEntities.All(x => x.VARIATION_ITEM != null && x.VARIATION_ITEM.ACTION == VariationAction.Add);
        }

        //public bool CanBulkRestore()
        //{
        //    return loadVARIATION.SUBMITTED == null && SelectedEntities != null && SelectedEntities.Any(x => x.VARIATION_ITEM != null && x.VARIATION_ITEM.ACTION == VariationAction.Cancel);
        //}

        //public void BulkRestore()
        //{
        //    IEnumerable<VARIATION_ITEMProjection> cancelledVARIATIONS = SelectedEntities.Where(x => x.VARIATION_ITEM != null && x.VARIATION_ITEM.ACTION == VariationAction.Cancel);
        //    foreach(VARIATION_ITEMProjection cancelledVARIATION in cancelledVARIATIONS)
        //    {
        //        cancelledVARIATION.VARIATION_ITEM.ACTION = VariationAction.NoAction;
        //        MainEntitySaveVariation(cancelledVARIATION, false);
        //    }
        //}

        //public bool CanBulkDelete()
        //{
        //    return loadVARIATION.SUBMITTED == null && SelectedEntities != null;
        //}

        //public void BulkDelete()
        //{
        //    //Only newly added variation can be deleted
        //    IEnumerable<VARIATION_ITEMProjection> addedVARIATIONS = SelectedEntities.Where(x => x.VARIATION_ITEM != null && x.VARIATION_ITEM.ACTION == VariationAction.Add);
        //    //variation that is in appended or no action shall be cancelled
        //    IEnumerable<VARIATION_ITEMProjection> existingVARIATIONS = SelectedEntities.Where(x => x.VARIATION_ITEM != null && (x.VARIATION_ITEM.ACTION == VariationAction.Append || x.VARIATION_ITEM.ACTION == VariationAction.NoAction));

        //    IEnumerable<VARIATION_ITEM> deleteVARIATION_ITEM = addedVARIATIONS.Select(x => x.VARIATION_ITEM);
        //    VARIATION_ITEMSCollectionViewModel.BaseBulkDelete(deleteVARIATION_ITEM);
        //    MainViewModel.BaseBulkDelete(addedVARIATIONS);

        //    foreach(VARIATION_ITEMProjection existingVARIATION in existingVARIATIONS)
        //    {
        //        existingVARIATION.VARIATION_ITEM.VARIATION_UNITS = 0;
        //        existingVARIATION.VARIATION_ITEM.ACTION = VariationAction.Cancel;
        //        MainEntitySaveVariation(existingVARIATION, false);
        //    }
        //}
        public bool AddVARIATION_ITEMCallBack(RowEventArgs e, VARIATION_ITEMProjection projectionEntity)
        {
            projectionEntity.VARIATION_ITEM.ACTION = VariationAction.Add;
            return true;
        }


        public void NewProjectionInitializeCallBack(VARIATION_ITEMProjection projectionEntity)
        {
            projectionEntity.VARIATION_ITEM.GUID_VARIATION = loadVARIATION.GUID;
            //projectionEntity.VARIATION_ITEM.ACTION = VariationAction.Add;
        }

        public bool ExistingProjectionEditCallBack(VARIATION_ITEMProjection projectionEntity,
            CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName !=
                BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().VARIATION_ITEM) + "." +
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
                BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().VARIATION_ITEM) + "." +
                BindableBase.GetPropertyName(() => new VARIATION_ITEM().ACTION), oldAction,
                projectionEntity.VARIATION_ITEM.ACTION, EntityMessageType.Changed);
            return true;
        }

        private bool MainEntityPreSaveWithNewEntityDetection(VARIATION_ITEMProjection projectionEntity, bool isNewEntity)
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

        private void MainEntitySaveVariation(VARIATION_ITEMProjection projectionEntity, bool isNewEntity)
        {
            var saveVARIATION_ITEM = projectionEntity.VARIATION_ITEM;
            saveVARIATION_ITEM.GUID_VARIATION = loadVARIATION.GUID;
            saveVARIATION_ITEM.GUID_ORIBASEITEM = projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL;
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (saveVARIATION_ITEM.CREATED.Date.Year == 1)
                saveVARIATION_ITEM.CREATED = DateTime.Now;

            VARIATION_ITEMSCollectionViewModel.Save(saveVARIATION_ITEM);
        }

        public void ApplyProjectionPropertiesToEntityCallBack(VARIATION_ITEMProjection projectionEntity,
            BASELINE_ITEM entity)
        {
            projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_VARIATION = loadVARIATION.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, VARIATION_ITEMProjection projectionEntity,
            BASELINE_ITEM entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID = entity.GUID;
            projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }

        public void EntityBeforeDeletionCallBack(VARIATION_ITEMProjection undoRedoEntity)
        {
            VARIATION_ITEMSCollectionViewModel.Delete(undoRedoEntity.VARIATION_ITEM);
        }

        #endregion

        #endregion

        #region View Behavior

        public void CancelBASELINE_ITEM(VARIATION_ITEMProjection projectionEntity)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            if (projectionEntity.VARIATION_ITEM.ACTION == VariationAction.Add)
                return;

            if (projectionEntity.VARIATION_ITEM.ACTION == VariationAction.Cancel)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                    BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().VARIATION_ITEM)
                    + "."
                    + BindableBase.GetPropertyName(() => new VARIATION_ITEM().ACTION), VariationAction.Cancel,
                    VariationAction.NoAction, EntityMessageType.Changed);
                projectionEntity.VARIATION_ITEM.ACTION = VariationAction.NoAction;
            }
            else
            {
                var oldUnits = projectionEntity.VARIATION_ITEM.VARIATION_UNITS;
                projectionEntity.VARIATION_ITEM.VARIATION_UNITS = 0;

                MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                    BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().VARIATION_ITEM)
                    + "."
                    + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS), oldUnits,
                    projectionEntity.VARIATION_ITEM.VARIATION_UNITS, EntityMessageType.Changed);
                var oldAction = projectionEntity.VARIATION_ITEM.ACTION;
                projectionEntity.VARIATION_ITEM.ACTION = VariationAction.Cancel;
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                    BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().VARIATION_ITEM)
                    + "."
                    + BindableBase.GetPropertyName(() => new VARIATION_ITEM().ACTION), oldAction, VariationAction.Cancel,
                    EntityMessageType.Changed);
            }

            MainViewModel.Save(projectionEntity);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            RefreshSelectedEntity();
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.RowHandle != DataControlBase.NewItemRowHandle)
                return;

            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS))
            {
                MessageBoxService.ShowMessage(CommonResources.Notify_AddBASELINE_ITEMBeforeVARIATION_UNITS);
                e.Handled = true;
                return;
            }

            var activeItem = (VARIATION_ITEMProjection) e.Row;
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().BASELINE_ITEMJoinRATE)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK
                ))
            {
                var chosenWORKPACK = WORKPACKCollection.FirstOrDefault(entity => entity.GUID == (Guid) e.Value);
                if (chosenWORKPACK != null)
                {
                    activeItem.BASELINE_ITEMJoinRATE = new BASELINE_ITEMProjection();
                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM = new BASELINE_ITEM();
                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_AREA = chosenWORKPACK.GUID_DAREA;
                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DOCTYPE = chosenWORKPACK.GUID_DDOCTYPE;
                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DEPARTMENT = chosenWORKPACK.GUID_DDEPARTMENT;
                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DISCIPLINE = chosenWORKPACK.GUID_DDISCIPLINE;
                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_PHASE = chosenWORKPACK.PHASE != null
                        ? chosenWORKPACK.GUID_DPHASE
                        : null;
                    var SelectedAREA = AREACollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DAREA);
                    var SelectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDOCTYPE);
                    var SelectedDISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDISCIPLINE);
                    var BASELINE_ITEMJoinRATES =
                        MainViewModel.Entities.Select(x => x.BASELINE_ITEMJoinRATE).AsEnumerable();

                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.INTERNAL_NUM =
                        BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, BASELINE_ITEMJoinRATES,
                            SelectedAREA, SelectedDISCIPLINE, SelectedDOCTYPE);
                    RefreshSelectedEntity();
                }
            }
            else if (e.Column.FieldName ==
                     BindableBase.GetPropertyName(() => new VARIATION_ITEMProjection().BASELINE_ITEMJoinRATE)
                     + "."
                     + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM)
                     + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid) e.Value);
                if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
                {
                    activeItem.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;
                    RefreshSelectedEntity();
                }
            }
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

            var BASELINE_ITEMS =
                MainViewModel.Entities.Select(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM);
            foreach (var selectedEntity in DisplaySelectedEntities)
            {
                var newProjection = new VARIATION_ITEMProjection();
                DataUtils.ShallowCopy(newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM,
                    selectedEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM);
                newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID = Guid.Empty;
                newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_BASELINE = null;
                newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL = Guid.Empty;
                newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS = 0;
                newProjection.VARIATION_ITEM.ACTION = VariationAction.Add;

                var selectedAREA =
                    AREACollection.FirstOrDefault(
                        x => x.GUID == newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_AREA);
                var selectedDISCIPLINE =
                    DISCIPLINECollection.FirstOrDefault(
                        x => x.GUID == newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DISCIPLINE);
                var selectedDOCTYPE =
                    DOCTYPECollection.FirstOrDefault(
                        x => x.GUID == newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DOCTYPE);
                newProjection.BASELINE_ITEMJoinRATE.BASELINE_ITEM.INTERNAL_NUM =
                    BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, BASELINE_ITEMS, selectedAREA,
                        selectedDISCIPLINE, selectedDOCTYPE, newProjection.GUID);
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
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject) button) as GridMenuInfo;

            var departmentFieldName = "BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DEPARTMENT";
            var disciplineFieldName = "BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DISCIPLINE";
            var docTypeFieldName = "BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DOCTYPE";
            var areaFieldName = "BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_AREA";
            var workpackFieldName = "BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK";
            var internalNumberFieldName = "BASELINE_ITEMJoinRATE.BASELINE_ITEM.INTERNAL_NUM";

            var entitiesToSave = new List<VARIATION_ITEMProjection>();
            if (info.Column.FieldName == internalNumberFieldName)
                foreach (var entity in MainViewModel.SelectedEntities)
                    entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.INTERNAL_NUM = string.Empty;

            var BASELINE_ITEMS =
                MainViewModel.Entities.Select(x => x.BASELINE_ITEMJoinRATE.BASELINE_ITEM);
            foreach (var entity in MainViewModel.SelectedEntities)
            {
                var entityWORKPACK =
                    WORKPACKCollection.FirstOrDefault(
                        x => x.GUID == entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_WORKPACK);
                if (info.Column.FieldName == internalNumberFieldName)
                {
                    var internalNum = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT,
                        BASELINE_ITEMS, entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.AREA,
                        entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.DISCIPLINE,
                        entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.DOCTYPE, entity.GUID);
                    MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, internalNum);
                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == departmentFieldName || info.Column.FieldName == disciplineFieldName ||
                         info.Column.FieldName == docTypeFieldName || info.Column.FieldName == areaFieldName)
                {
                    if (entityWORKPACK == null)
                        continue;

                    if (info.Column.FieldName == departmentFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName,
                            entityWORKPACK.GUID_DDEPARTMENT);
                    else if (info.Column.FieldName == disciplineFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName,
                            entityWORKPACK.GUID_DDISCIPLINE);
                    else if (info.Column.FieldName == docTypeFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDOCTYPE);
                    else if (info.Column.FieldName == areaFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DAREA);

                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == workpackFieldName)
                {
                    if (entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DISCIPLINE == Guid.Empty ||
                        entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DEPARTMENT == Guid.Empty ||
                        entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DOCTYPE == Guid.Empty ||
                        entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_AREA == Guid.Empty)
                        continue;

                    var findWORKPACK =
                        WORKPACKCollection.FirstOrDefault(
                            x =>
                                x.GUID_DDEPARTMENT == entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DEPARTMENT &&
                                x.GUID_DDISCIPLINE == entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DISCIPLINE);
                    if (findWORKPACK == null)
                    {
                        var newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_PROJECT = loadPROJECT.GUID;
                        if (entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_AREA != null)
                            newWORKPACK.GUID_DAREA = (Guid) entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_AREA;
                        if (entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_PHASE != null)
                            newWORKPACK.GUID_DPHASE = entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_PHASE;
                        if (entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DISCIPLINE != null)
                            newWORKPACK.GUID_DDISCIPLINE =
                                (Guid) entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DISCIPLINE;
                        if (entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DEPARTMENT != null)
                            newWORKPACK.GUID_DDEPARTMENT =
                                (Guid) entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DEPARTMENT;
                        if (entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DOCTYPE != null)
                            newWORKPACK.GUID_DDOCTYPE = (Guid) entity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_DOCTYPE;

                        newWORKPACK.INTERNAL_NAME1 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber1(loadPROJECT,
                            newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, DOCTYPECollection);
                        newWORKPACK.INTERNAL_NAME2 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT,
                            newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, PHASECollection);

                        newWORKPACK.STARTDATE = DateTime.Now;
                        newWORKPACK.ENDDATE =
                            BluePrintDataUtils.WORKPACK_Calculate_EndDate((DateTime) newWORKPACK.STARTDATE, loadPROJECT);
                        var reviewStartDate = (DateTime) newWORKPACK.STARTDATE;
                        var reviewEndDate = (DateTime) newWORKPACK.ENDDATE;
                        BluePrintDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate,
                            loadPROJECT, false);
                        newWORKPACK.REVIEWSTARTDATE = reviewStartDate;
                        newWORKPACK.REVIEWENDDATE = reviewEndDate;
                        newWORKPACK.AUTOGENERATED = true;
                        newWORKPACK.TYPE = WorkpackType.Design;
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

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "VARIATION_ITEMSViewModelWrapper"; }
        }

        /// <summary>
        /// The workpack internal name to be used
        /// </summary>
        public string WORKPACKDisplayMember
        {
            get
            {
                if (loadPROJECT == null || loadPROJECT.USELEGACYWORKPACK)
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME1);
                else
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME2);
            }
        }

        public void ShowWORKPACKColumns()
        {
            if (ShowWORKPACKInternalName1 == null || ShowWORKPACKInternalName2 == null)
                return;

            if (loadPROJECT == null || loadPROJECT.USELEGACYWORKPACK)
                ShowWORKPACKInternalName1();
            else
                ShowWORKPACKInternalName2();
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1).OrderBy(x => x.INTERNAL_NAME2);
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
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
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