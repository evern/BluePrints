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
using DevExpress.Xpf.Grid;
using System.Threading;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using System.Windows.Threading;
using System.Windows;
using DevExpress.Xpf.Bars;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Views;
using BluePrints.Reports;
using System.IO;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using System.ComponentModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single ESTIMATION_DIRECT object view model.
    /// </summary>
    public partial class ESTIMATION_DIRECT_ITEMSViewModelWrapper : CollectionViewModelsWrapper<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        public Action ShowWORKPACKInternalName1;
        public Action ShowWORKPACKInternalName2;
        /// <summary>
        /// Creates a new instance of ESTIMATION_DIRECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMSViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMSViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ESTIMATION_DIRECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ESTIMATION_DIRECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATION_DIRECT_ITEMSViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        PROJECT loadPROJECT;
        ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        BackgroundWorker refreshBackgroundWorker;
        BackgroundWorker displayEntitiesRefreshBackgroundWorker;
        BackgroundWorker userStateRestoreBackgroundWorker;

        public ESTIMATION_DIRECT_ITEMProjection SelectedEntity { get; set; }
        ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> selectedentities { get; set; }
        public ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        Guid RestoreSelectedEntityGuid;
        List<Guid> RestoreSelectedEntitiesGuids = new List<Guid>();
        List<Guid> RestoreExpandedGuids = new List<Guid>();
        protected override void InitializeParameters(object parameter)
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            SelectedEntities = new ObservableCollection<ESTIMATION_DIRECT_ITEMProjection>();
            userStateRestoreBackgroundWorker = new BackgroundWorker();
            userStateRestoreBackgroundWorker.DoWork += userStateRestoreBackgroundWorker_DoWork;
            userStateRestoreBackgroundWorker.WorkerSupportsCancellation = true;

            refreshBackgroundWorker = new BackgroundWorker();
            refreshBackgroundWorker.DoWork += refreshBackgroundWorker_DoWork;
            refreshBackgroundWorker.WorkerSupportsCancellation = true;

            displayEntitiesRefreshBackgroundWorker = new BackgroundWorker();
            displayEntitiesRefreshBackgroundWorker.DoWork += displayEntitiesRefreshBackgroundWorker_DoWork;
            displayEntitiesRefreshBackgroundWorker.WorkerSupportsCancellation = true;

            OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT> receiveParameter = (OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT>)parameter;
            this.loadPROJECT = receiveParameter.GetFirstEntity();
            this.loadESTIMATION_DIRECT = receiveParameter.GetSecondEntity();
        }

        bool isQueryForLiveStatus
        {
            get { return this.loadPROJECT != null; }
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<ESTIMATION_DIRECT, ESTIMATION_DIRECT, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, typeof(PROJECT), isContinueLoadingAfterESTIMATION_DIRECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, typeof(PROJECT), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECT, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjectionFunc, typeof(PROJECT), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(6, bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>(7, bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, typeof(BluePrints.Data.PROJECT));
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(8, bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROJECT), null, OnAfterEntitiesChanged);
            
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if(entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        bool isContinueLoadingAfterESTIMATION_DIRECT(IEnumerable<ESTIMATION_DIRECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "ESTIMATION_DIRECT"))));
                return false;
            }

            this.loadESTIMATION_DIRECT = entities.First();
            return true;
        }

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == this.loadESTIMATION_DIRECT.GUID_PROJECT);
        }

        Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID && x.STATUS == EstimationStatus.Live);
            else
                return query => query.Where(x => x.GUID == this.loadESTIMATION_DIRECT.GUID);
        }

        Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.Design);
        }

        Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct);
        }

        Func<IRepositoryQuery<COMMODITY_GROUP_DIRECT>, IQueryable<COMMODITY_GROUP_DIRECT>> COMMODITY_GROUP_DIRECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null || x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query;
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>> ConstructMainViewModelProjection()
        {
            Func<IQueryable<DEPARTMENT>> getDEPARTMENTSFunc = loaderCollection.GetCollectionFunc<DEPARTMENT>();
            Func<IQueryable<RATE>> getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            Func<ESTIMATION_DIRECT> getESTIMATION_DIRECTFunc = loaderCollection.GetObjectFunc<ESTIMATION_DIRECT>();
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.JoinRATESOnESTIMATION_DIRECT_ITEMS(query, getESTIMATION_DIRECTFunc, getDEPARTMENTSFunc, getRATESFunc);
        }

        #region View Refresh
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = this.CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = this.OnEntitiesSavedCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = this.ExistingProjectionEditCallBack;

            MainViewModel.SetParentViewModel(this);
            refreshBackgroundWorker.RunWorkerAsync();
        }

        List<Guid> SelectedEntitiesGuid = new List<Guid>();
        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (changedType == typeof(COMMODITY_GROUP_DIRECT) || changedType == typeof(ESTIMATION_DIRECT_ITEM))
            {
                storeViewState();
                
                if (changedType == typeof(COMMODITY_GROUP_DIRECT))
                    COMMODITY_GROUP_DIRECTCollectionViewModel.Refresh();
                else
                {
                    if (sender.ToString() != MainViewModel.ToString())
                        mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                    else
                        mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));
                }

                if (!displayEntitiesRefreshBackgroundWorker.IsBusy)
                    displayEntitiesRefreshBackgroundWorker.RunWorkerAsync();
            }

            if (sender.ToString() == MainViewModel.ToString())
                return;

            if (changedType == typeof(WORKPACK))
            {
                this.RaisePropertyChanged(x => x.WORKPACKCollection);
                return;
            }

            if (loadESTIMATION_DIRECT != null && changedType == typeof(ESTIMATION_DIRECT) && loadESTIMATION_DIRECT.GUID.ToString() == key.ToString() ||
                loadPROJECT != null && changedType == typeof(PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null || loadESTIMATION_DIRECT != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null || loadESTIMATION_DIRECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }

            return;
        }

        void refreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(500);
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        void displayEntitiesRefreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            displayEntities = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplayEntities)));
        }

        void userStateRestoreBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1);
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.restoreViewState()));
        }

        void storeViewState()
        {
            RestoreSelectedEntityGuid = Guid.Empty;
            RestoreSelectedEntitiesGuids.Clear();
            RestoreExpandedGuids.Clear();

            foreach (ESTIMATION_DIRECT_ITEMProjection selectedEntity in SelectedEntities)
            {
                RestoreSelectedEntitiesGuids.Add(new Guid(selectedEntity.GUID.ToString()));
            }

            foreach (ESTIMATION_DIRECT_ITEMProjection entity in DisplayEntities)
            {
                if (entity.ISEXPANDED)
                    RestoreExpandedGuids.Add(entity.GUID);
            }

            if (SelectedEntity != null)
                RestoreSelectedEntityGuid = SelectedEntity.GUID;
        }

        void restoreViewState()
        {
            IEnumerable<ESTIMATION_DIRECT_ITEMProjection> restoreSelectedEntities = DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_ESTIMATION_DIRECT_ITEM)).Where(x => RestoreSelectedEntitiesGuids.Any(y => y == x.GUID));
            SelectedEntities.Clear();
            if (restoreSelectedEntities.Count() > 0)
            {
                foreach (ESTIMATION_DIRECT_ITEMProjection restoreSelectedEntity in restoreSelectedEntities)
                {
                    SelectedEntities.Add(restoreSelectedEntity);
                }
            }

            foreach (Guid expandedGuid in RestoreExpandedGuids)
            {
                ESTIMATION_DIRECT_ITEMProjection restoreExpandedEntity = DisplayEntities.FirstOrDefault(x => x.GUID == expandedGuid);
                if (restoreExpandedEntity != null)
                {
                    ExpandDisplayRow(restoreExpandedEntity);
                }
            }

            if (RestoreSelectedEntityGuid != Guid.Empty)
            {
                ESTIMATION_DIRECT_ITEMProjection restoreSelectedEntity = DisplayEntities.Concat(DisplayEntities.SelectMany(x => x.CHILD_ESTIMATION_DIRECT_ITEM)).FirstOrDefault(x => x.GUID == RestoreSelectedEntityGuid);
                if (restoreSelectedEntity != null)
                    SelectedEntity = restoreSelectedEntity;
            }
        }
        #endregion

        #region Collection Call Backs
        //public void ExistingChildrenRowAddUndoAndSave(CellValueChangedEventArgs e)
        //{
        //    ESTIMATION_DIRECT_ITEMProjection editedESTIMATION_DIRECT_ITEM = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
        //    //if (e.RowHandle == GridControl.NewItemRowHandle)
        //    //{
        //    //    editedCOMMODITY.COMMODITY_GROUP.RaisePropertyChanged(x => x.ISQUANTIFIABLE);
        //    //    return;
        //    //}

        //    COMMODITY_GROUP_DIRECTCollectionViewModel.entities
        //    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
        //    MainViewModel.EntitiesUndoRedoManager.AddUndo(editedCOMMODITY, e.Column.FieldName, e.OldValue, e.Value, EntityMessageType.Changed);
        //    COMMODITY_GROUP_DIRECTProjection parentCOMMODITY = SumParentEditValue(editedCOMMODITY, e.Column.FieldName, e.Value);
        //    if (parentCOMMODITY != null)
        //    {
        //        Save(parentCOMMODITY);
        //    }

        //    MainViewModel.Save(editedCOMMODITY);
        //    MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        //}

        //private COMMODITY_GROUP_DIRECTProjection SumParentEditValue(COMMODITY_GROUP_DIRECTProjection childCOMMODITY, string fieldName, object newEditValue)
        //{
        //    if (childCOMMODITY.COMMODITY_GROUP.GUID_PARENT == Guid.Empty)
        //        return null;
        //    else
        //    {
        //        COMMODITY_GROUP_DIRECTProjection parentCOMMODITY = COMMODITY_GROUP_DIRECTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == childCOMMODITY.COMMODITY_GROUP.GUID_PARENT);
        //        if (parentCOMMODITY != null)
        //        {
        //            decimal newValue = -1;
        //            decimal? oldValue = null;
        //            if (fieldName == BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECTProjection().COMMODITY_GROUP) + "." + BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().RATE_FREIGHT))
        //            {
        //                newValue = COMMODITY_GROUP_DIRECTCollectionViewModel.Entities.Where(x => x.COMMODITY_GROUP.GUID_PARENT == parentCOMMODITY.GUID && x.COMMODITY_GROUP.RATE_FREIGHT != null && x.GUID != childCOMMODITY.GUID).Sum(x => (decimal)x.COMMODITY_GROUP.RATE_FREIGHT);
        //                newValue += (decimal)newEditValue;
        //                oldValue = parentCOMMODITY.COMMODITY_GROUP.RATE_FREIGHT;
        //                parentCOMMODITY.COMMODITY_GROUP.RATE_FREIGHT = newValue;
        //            }
        //            else if (fieldName == BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECTProjection().COMMODITY_GROUP) + "." + BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().RATE_SUPPLY))
        //            {
        //                newValue = COMMODITY_GROUP_DIRECTCollectionViewModel.Entities.Where(x => x.COMMODITY_GROUP.GUID_PARENT == parentCOMMODITY.GUID && x.COMMODITY_GROUP.RATE_SUPPLY != null && x.GUID != childCOMMODITY.GUID).Sum(x => (decimal)x.COMMODITY_GROUP.RATE_SUPPLY);
        //                newValue += (decimal)newEditValue;
        //                oldValue = parentCOMMODITY.COMMODITY_GROUP.RATE_SUPPLY;
        //                parentCOMMODITY.COMMODITY_GROUP.RATE_SUPPLY = newValue;
        //            }
        //            else if (fieldName == BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECTProjection().COMMODITY_GROUP) + "." + BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().HOURS_INSTALL))
        //            {
        //                newValue = COMMODITY_GROUP_DIRECTCollectionViewModel.Entities.Where(x => x.COMMODITY_GROUP.GUID_PARENT == parentCOMMODITY.GUID && x.COMMODITY_GROUP.HOURS_INSTALL != null && x.GUID != childCOMMODITY.GUID).Sum(x => (decimal)x.COMMODITY_GROUP.HOURS_INSTALL);
        //                newValue += (decimal)newEditValue;
        //                oldValue = parentCOMMODITY.COMMODITY_GROUP.HOURS_INSTALL;
        //                parentCOMMODITY.COMMODITY_GROUP.HOURS_INSTALL = newValue;
        //            }

        //            if (newValue != -1)
        //            {
        //                COMMODITY_GROUP_DIRECTCollectionViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY, fieldName, oldValue, newValue, EntityMessageType.Changed);
        //                return parentCOMMODITY;
        //            }
        //        }
        //        else
        //            return null;
        //    }

        //    return null;
        //}

        public void ApplyProjectionPropertiesToEntity(ESTIMATION_DIRECT_ITEMProjection projectionEntity, ESTIMATION_DIRECT_ITEM entity)
        {
            projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ESTIMATION_DIRECT = loadESTIMATION_DIRECT.GUID;

            DataUtils.ShallowCopy(entity, projectionEntity.ESTIMATION_DIRECT_ITEM);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.ESTIMATION_DIRECT_ITEM.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.ESTIMATION_DIRECT_ITEM.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, ESTIMATION_DIRECT_ITEMProjection projectionEntity, ESTIMATION_DIRECT_ITEM entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.ESTIMATION_DIRECT_ITEM.GUID = entity.GUID;
            projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL = entity.GUID_ORIGINAL;
        }

        public void ExistingProjectionEditCallBack(ESTIMATION_DIRECT_ITEMProjection projectionEntity, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName != BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().MANUAL_COMMODITY_GROUP_DIRECT) + "." + BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().HOURS_INSTALL)
             && e.Column.FieldName != BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().MANUAL_COMMODITY_GROUP_DIRECT) + "." + BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().RATE_FREIGHT)
             && e.Column.FieldName != BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().MANUAL_COMMODITY_GROUP_DIRECT) + "." + BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().RATE_SUPPLY))
                return;

            if (projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT.GUID_PROJECT != null)
            {
                COMMODITY_GROUP_DIRECT actualCOMMODITY_GROUP_DIRECT = COMMODITY_GROUP_DIRECTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT.GUID);
                if (actualCOMMODITY_GROUP_DIRECT != null)
                {
                    DataUtils.ShallowCopy(actualCOMMODITY_GROUP_DIRECT, projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT);
                    COMMODITY_GROUP_DIRECTCollectionViewModel.Save(actualCOMMODITY_GROUP_DIRECT);
                }
            }
            else
            {
                //checks COMMODITY_GROUP_DIRECT if COMMODITY_CODE already exists
                COMMODITY_GROUP_DIRECT existingCOMMODITY_GROUP_DIRECT = COMMODITY_GROUP_DIRECTCollectionViewModel.Entities.FirstOrDefault(x => x.GUID_COMMODITYCODE == projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT.GUID_COMMODITYCODE && x.GUID_PROJECT == loadPROJECT.GUID);

                if (existingCOMMODITY_GROUP_DIRECT != null || (MessageBoxService.ShowMessage(CommonResources.Estimation_Item_Direct_AddCustomCommodityGroup, CommonResources.Confirmation_Caption, MessageButton.YesNo) == MessageResult.Yes))
                {
                    Guid oldGuid;
                    Guid newGuid;

                    if (existingCOMMODITY_GROUP_DIRECT != null)
                    {
                        existingCOMMODITY_GROUP_DIRECT.HOURS_INSTALL = projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT.HOURS_INSTALL;
                        existingCOMMODITY_GROUP_DIRECT.RATE_FREIGHT = projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT.RATE_FREIGHT;
                        existingCOMMODITY_GROUP_DIRECT.RATE_SUPPLY = projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT.RATE_SUPPLY;
                        COMMODITY_GROUP_DIRECTCollectionViewModel.Save(existingCOMMODITY_GROUP_DIRECT);

                        oldGuid = projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT;
                        newGuid = existingCOMMODITY_GROUP_DIRECT.GUID;
                    }
                    else
                    {
                        COMMODITY_GROUP_DIRECT newCOMMODITY_GROUP_DIRECT = new COMMODITY_GROUP_DIRECT();
                        DataUtils.ShallowCopy(newCOMMODITY_GROUP_DIRECT, projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT);

                        newCOMMODITY_GROUP_DIRECT.GUID = Guid.Empty;
                        newCOMMODITY_GROUP_DIRECT.GUID_PROJECT = loadPROJECT.GUID;
                        COMMODITY_GROUP_DIRECTCollectionViewModel.Save(newCOMMODITY_GROUP_DIRECT);

                        oldGuid = projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT;
                        newGuid = newCOMMODITY_GROUP_DIRECT.GUID;

                        COMMODITY_GROUP_DIRECTProjection findCOMMODITY_GROUP_DIRECT = COMMODITY_GROUP_DIRECT_DisplayCollection.FirstOrDefault(x => x.GUID == oldGuid);
                        if (findCOMMODITY_GROUP_DIRECT != null && findCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP.Count > 0)
                        {
                            foreach(COMMODITY_GROUP_DIRECTProjection childCOMMODITY_GROUP_DIRECT in findCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP)
                            {
                                COMMODITY_GROUP_DIRECT newChild_COMMODITY_GROUP_DIRECT = new COMMODITY_GROUP_DIRECT();
                                DataUtils.ShallowCopy(newChild_COMMODITY_GROUP_DIRECT, childCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP);
                                newChild_COMMODITY_GROUP_DIRECT.GUID = Guid.Empty;
                                newChild_COMMODITY_GROUP_DIRECT.GUID_PARENT = newGuid;
                                newChild_COMMODITY_GROUP_DIRECT.GUID_PROJECT = loadPROJECT.GUID;
                                COMMODITY_GROUP_DIRECTCollectionViewModel.Save(newChild_COMMODITY_GROUP_DIRECT);
                            }
                        }
                    }

                    projectionEntity.MANUAL_COMMODITY_GROUP_DIRECT.GUID = newGuid;
                    projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT = newGuid;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity, BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_GROUP_DIRECT), oldGuid, newGuid, EntityMessageType.Changed);
                }
            }
        }
        #endregion
        #endregion

        #region Local Methods
        public Action<ESTIMATION_DIRECT_ITEMProjection> SetIsRowExpanded;
        #endregion

        #region View Behavior
        public ESTIMATION_DIRECT_ITEMProjection CreateNewProjectionFromNewEntityCallBack(ESTIMATION_DIRECT_ITEM entity)
        {
            return new ESTIMATION_DIRECT_ITEMProjection();
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.RowHandle != GridControl.NewItemRowHandle)
                return;

            //ESTIMATION_DIRECT_ITEMProjection activeESTIMATION_DIRECT_ITEM = (ESTIMATION_DIRECT_ITEMProjection)e.Row;
            //if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_WORKPACK))
            //{
            //    WORKPACK chosenWORKPACK = WORKPACKCollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
            //    if (chosenWORKPACK != null)
            //    {
            //        activeESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_AREA = chosenWORKPACK.GUID_DAREA;
            //        activeESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DOCTYPE = chosenWORKPACK.GUID_DDOCTYPE;
            //        activeESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DEPARTMENT = chosenWORKPACK.GUID_DDEPARTMENT;
            //        activeESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE = chosenWORKPACK.GUID_DDISCIPLINE;
            //        activeESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_PHASE = chosenWORKPACK.PHASE != null ? chosenWORKPACK.GUID_DPHASE : null;
            //        var SelectedAREA = AREACollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DAREA);
            //        var SelectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDOCTYPE);
            //        var SelectedDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == chosenWORKPACK.GUID_DDISCIPLINE);

            //        activeESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.INTERNAL_NUM = BluePrintDataUtils.ESTIMATION_DIRECTITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities, SelectedAREA, SelectedDISCIPLINE, SelectedDOCTYPE);
            //        MainViewModel.UpdateSelectedEntity();
            //    }
            //}
            //else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DOCTYPE))
            //{
            //    DOCTYPE chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
            //    if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
            //    {
            //        activeESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;
            //        MainViewModel.UpdateSelectedEntity();
            //    }
            //}
        }
        #endregion

        #region View Commands
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

            //foreach(ESTIMATION_DIRECT_ITEMProjection selectedEntity in MainViewModel.SelectedEntities)
            //{
            //    ESTIMATION_DIRECT_ITEMProjection newProjection = new ESTIMATION_DIRECT_ITEMProjection();
            //    DataUtils.ShallowCopy(newProjection.ESTIMATION_DIRECT_ITEM, selectedEntity.ESTIMATION_DIRECT_ITEM);
            //    newProjection.ESTIMATION_DIRECT_ITEM.GUID = Guid.Empty;
            //    newProjection.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL = Guid.Empty;
            //    AREA selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.ESTIMATION_DIRECT_ITEM.GUID_AREA);
            //    DISCIPLINE selectedDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE);
            //    DOCTYPE selectedDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.ESTIMATION_DIRECT_ITEM.GUID_DOCTYPE);
            //    newProjection.ESTIMATION_DIRECT_ITEM.INTERNAL_NUM = BluePrintDataUtils.ESTIMATION_DIRECTITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities, selectedAREA, selectedDISCIPLINE, selectedDOCTYPE, newProjection.GUID);
            //    MainViewModel.EntitiesUndoRedoManager.AddUndo(newProjection, null, null, null, EntityMessageType.Added);
            //    MainViewModel.Save(newProjection);
            //}

            if (!isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        bool isProcessingMultipleDuplicates;
        /// <summary>
        /// Paste clipboard data multiple times
        /// </summary>
        public void DuplicateMultiple(BarEditItem barEdit)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            isProcessingMultipleDuplicates = true;
            int timesToDuplicate = 0;
            if(Int32.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {
                for(int i=0;i < timesToDuplicate;i++)
                {
                    Duplicate();
                }
            }
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
            //var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;

            //string departmentFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DEPARTMENT);
            //string disciplineFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE);
            //string docTypeFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DOCTYPE);
            //string areaFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_AREA);
            //string workpackFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_WORKPACK);
            //string internalNumberFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().INTERNAL_NUM);

            //List<ESTIMATION_DIRECT_ITEMProjection> entitiesToSave = new List<ESTIMATION_DIRECT_ITEMProjection>();
            //if (info.Column.FieldName == internalNumberFieldName)
            //{
            //    foreach(ESTIMATION_DIRECT_ITEMProjection entity in MainViewModel.SelectedEntities)
            //    {
            //        entity.ESTIMATION_DIRECT_ITEM.INTERNAL_NUM = string.Empty;
            //    }
            //}

            //foreach(ESTIMATION_DIRECT_ITEMProjection entity in MainViewModel.SelectedEntities)
            //{
            //    WORKPACK entityWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID == entity.ESTIMATION_DIRECT_ITEM.GUID_WORKPACK);
            //    if (info.Column.FieldName == internalNumberFieldName)
            //    {
            //        string internalNum = BluePrintDataUtils.ESTIMATION_DIRECTITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities, entity.ESTIMATION_DIRECT_ITEM.AREA, entity.ESTIMATION_DIRECT_ITEM.DISCIPLINE, entity.ESTIMATION_DIRECT_ITEM.DOCTYPE, entity.GUID);
            //        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, internalNum);
            //        entitiesToSave.Add(entity);
            //    }
            //    else if(info.Column.FieldName == departmentFieldName || info.Column.FieldName == disciplineFieldName || info.Column.FieldName == docTypeFieldName || info.Column.FieldName == areaFieldName)
            //    {
            //        if(entityWORKPACK == null)
            //            continue;

            //        if (info.Column.FieldName == departmentFieldName)
            //            MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDEPARTMENT);
            //        else if (info.Column.FieldName == disciplineFieldName)
            //            MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDISCIPLINE);
            //        else if (info.Column.FieldName == docTypeFieldName)
            //            MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDOCTYPE);
            //        else if (info.Column.FieldName == areaFieldName)
            //            MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DAREA);

            //        entitiesToSave.Add(entity);
            //    }
            //    else if(info.Column.FieldName == workpackFieldName)
            //    {
            //        if (entity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE == Guid.Empty || entity.ESTIMATION_DIRECT_ITEM.GUID_DEPARTMENT == Guid.Empty ||
            //           entity.ESTIMATION_DIRECT_ITEM.GUID_DOCTYPE == Guid.Empty || entity.ESTIMATION_DIRECT_ITEM.GUID_AREA == Guid.Empty)
            //            continue;

            //        WORKPACK findWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID_DAREA == entity.ESTIMATION_DIRECT_ITEM.GUID_AREA && x.GUID_DDOCTYPE == entity.ESTIMATION_DIRECT_ITEM.GUID_DOCTYPE && x.GUID_DDISCIPLINE == entity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE);
            //        if (findWORKPACK == null)
            //        {
            //            WORKPACK newWORKPACK = new WORKPACK();
            //            newWORKPACK.GUID_PROJECT = loadPROJECT.GUID;
            //            if (entity.ESTIMATION_DIRECT_ITEM.GUID_AREA != null)
            //                newWORKPACK.GUID_DAREA = (Guid)entity.ESTIMATION_DIRECT_ITEM.GUID_AREA;
            //            if (entity.ESTIMATION_DIRECT_ITEM.GUID_PHASE != null)
            //                newWORKPACK.GUID_DPHASE = entity.ESTIMATION_DIRECT_ITEM.GUID_PHASE;
            //            if (entity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE != null)
            //                newWORKPACK.GUID_DDISCIPLINE = (Guid)entity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE;
            //            if (entity.ESTIMATION_DIRECT_ITEM.GUID_DEPARTMENT != null)
            //                newWORKPACK.GUID_DDEPARTMENT = (Guid)entity.ESTIMATION_DIRECT_ITEM.GUID_DEPARTMENT;
            //            if (entity.ESTIMATION_DIRECT_ITEM.GUID_DOCTYPE != null)
            //                newWORKPACK.GUID_DDOCTYPE = (Guid)entity.ESTIMATION_DIRECT_ITEM.GUID_DOCTYPE;

            //            newWORKPACK.INTERNAL_NAME1 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber1(loadPROJECT, newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, DOCTYPECollection);
            //            newWORKPACK.INTERNAL_NAME2 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT, newWORKPACK, WORKPACKCollection, AREACollection, DISCIPLINECollection, PHASECollection);

            //            if (newWORKPACK.INTERNAL_NAME1 == string.Empty && newWORKPACK.INTERNAL_NAME2 == string.Empty)
            //                return;

            //            newWORKPACK.STARTDATE = DateTime.Now;
            //            newWORKPACK.ENDDATE = BluePrintDataUtils.WORKPACK_Calculate_EndDate(newWORKPACK.STARTDATE, loadPROJECT);
            //            DateTime reviewStartDate = newWORKPACK.STARTDATE;
            //            DateTime reviewEndDate = newWORKPACK.ENDDATE;
            //            BluePrintDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate, loadPROJECT, false);
            //            newWORKPACK.REVIEWSTARTDATE = reviewStartDate;
            //            newWORKPACK.REVIEWENDDATE = reviewEndDate;
            //            newWORKPACK.AUTOGENERATED = true;
            //            newWORKPACK.TYPE = WorkpackType.Design;
            //            ((CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>()).Save(newWORKPACK);

            //            MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, newWORKPACK.GUID);
            //        }
            //        else
            //            MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, findWORKPACK.GUID);

            //        entitiesToSave.Add(entity);
            //    }
            //}

            //MainViewModel.BulkSave(entitiesToSave);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }


        public void MasterRowExpanded(RowEventArgs e)
        {
            ((ESTIMATION_DIRECT_ITEMProjection)e.Row).ISEXPANDED = true;
        }

        public void MasterRowCollapsed(RowEventArgs e)
        {
            ((ESTIMATION_DIRECT_ITEMProjection)e.Row).ISEXPANDED = false;
        }

        void ExpandDisplayRow(ESTIMATION_DIRECT_ITEMProjection row)
        {
            row.ISEXPANDED = true;
            if (SetIsRowExpanded != null)
                SetIsRowExpanded(row);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "ESTIMATION_DIRECT_ITEMSViewModelWrapper";
            }
        }

        public CollectionViewModel<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECT, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITY_GROUP_DIRECTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<COMMODITY_GROUP_DIRECT>();
            }
        }

        ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> displayEntities;
        public ObservableCollection<ESTIMATION_DIRECT_ITEMProjection> DisplayEntities
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (displayEntities == null && COMMODITY_GROUP_DIRECT_DisplayCollection != null)
                {
                    displayEntities = new ObservableCollection<ESTIMATION_DIRECT_ITEMProjection>();

                    foreach (ESTIMATION_DIRECT_ITEMProjection estimation_direct_item in MainViewModel.Entities)
                    {
                        ESTIMATION_DIRECT_ITEMProjection estimation_direct_item_POCO = AddPOCODisplayEntity(estimation_direct_item);
                        displayEntities.Add(estimation_direct_item_POCO);
                    }

                    if (!userStateRestoreBackgroundWorker.IsBusy)
                        userStateRestoreBackgroundWorker.RunWorkerAsync();
                }

                return displayEntities;
            }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                return collection;
            }
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

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECTCollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_GROUP_DIRECT>();
                return collection;
            }
        }

        //COMMODITY_GROUP_DIRECTCollectionViewModelWrapper commodity_group_directCollectionViewModel;
        //public COMMODITY_GROUP_DIRECTCollectionViewModelWrapper COMMODITY_GROUP_DIRECTCollectionViewModel
        //{
        //    get
        //    {
        //        if (commodity_group_directCollectionViewModel == null && this.loadPROJECT != null)
        //        {
        //            commodity_group_directCollectionViewModel = COMMODITY_GROUP_DIRECTCollectionViewModelWrapper.Create();
        //            commodity_group_directCollectionViewModel.SetParentViewModel(this);
        //            ISupportParameter baselineSupportParameterObj = commodity_group_directCollectionViewModel as ISupportParameter;
        //            baselineSupportParameterObj.Parameter = null;
        //        }

        //        return commodity_group_directCollectionViewModel;
        //    }
        //}

        ObservableCollection<COMMODITY_GROUP_DIRECTProjection> commodity_group_direct_DisplayCollection;
        public ObservableCollection<COMMODITY_GROUP_DIRECTProjection> COMMODITY_GROUP_DIRECT_DisplayCollection
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if (commodity_group_direct_DisplayCollection == null && COMMODITY_GROUP_DIRECTCollection.Count() > 0)
                {
                    commodity_group_direct_DisplayCollection = new ObservableCollection<COMMODITY_GROUP_DIRECTProjection>();
                    IEnumerable<COMMODITY_GROUP_DIRECT> parentCOMMODITY_GROUP_DIRECTS = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT == null).AsEnumerable();
                    IEnumerable<COMMODITY_GROUP_DIRECT> childCOMMODITY_GROUP_DIRECTS = COMMODITY_GROUP_DIRECTCollection.Where(x => x.GUID_PARENT != null).AsEnumerable();
                    foreach (COMMODITY_GROUP_DIRECT parentCOMMODITY_GROUP_DIRECT in parentCOMMODITY_GROUP_DIRECTS)
                    {
                        COMMODITY_GROUP_DIRECTProjection parentCOMMODITY_GROUP_DIRECTPOCO = ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTProjection());
                        parentCOMMODITY_GROUP_DIRECTPOCO.GUID = parentCOMMODITY_GROUP_DIRECT.GUID;
                        DataUtils.ShallowCopy(parentCOMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP, parentCOMMODITY_GROUP_DIRECT);
                        commodity_group_direct_DisplayCollection.Add(parentCOMMODITY_GROUP_DIRECTPOCO);
                    }

                    foreach (COMMODITY_GROUP_DIRECTProjection commodity_group_direct in commodity_group_direct_DisplayCollection)
                    {
                        IEnumerable<COMMODITY_GROUP_DIRECT> childrenCOMMODITY_GROUP_DIRECTS = childCOMMODITY_GROUP_DIRECTS.Where(y => y.GUID_PARENT == commodity_group_direct.GUID);
                        foreach (COMMODITY_GROUP_DIRECT childrenCOMMODITY_GROUP_DIRECT in childrenCOMMODITY_GROUP_DIRECTS)
                        {
                            COMMODITY_GROUP_DIRECTProjection childrenCOMMODITY_GROUP_DIRECTPOCO = ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTProjection());
                            childrenCOMMODITY_GROUP_DIRECTPOCO.GUID = childrenCOMMODITY_GROUP_DIRECT.GUID;
                            DataUtils.ShallowCopy(childrenCOMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP, childrenCOMMODITY_GROUP_DIRECT);
                            commodity_group_direct.CHILD_COMMODITY_GROUP.Add(childrenCOMMODITY_GROUP_DIRECTPOCO);
                        }
                    }
                }

                return commodity_group_direct_DisplayCollection;
            }
        }
        #endregion

        #region Reporting
        public bool CanEditReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public void EditReport()
        {
            //REPORTDesigner reportDesigner = new REPORTDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.ESTIMATION_DIRECT_Report);
            //if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    reportDesigner.Dispose();
            //else
            //    reportDesigner.Dispose();
        }

        public void ViewReport()
        {
            //XtraReportESTIMATION_DIRECT_ITEMS ESTIMATION_DIRECTReport = new XtraReportESTIMATION_DIRECT_ITEMS();
            //PROJECT_REPORT dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            //if (dbProjectReport != null)
            //{
            //    string reportString = dbProjectReport.REPORT.ToString();
            //    using (StreamWriter sw = new StreamWriter(new MemoryStream()))
            //    {
            //        sw.Write(reportString);
            //        sw.Flush();
            //        ESTIMATION_DIRECTReport.LoadLayout(sw.BaseStream);
            //    }
            //}

            ////make sure disciplines are all populated
            //PopulateNavigationalProperties();
            //ESTIMATION_DIRECTReport.AssignProperties(loadPROJECT, loadESTIMATION_DIRECT, MainViewModel.Entities);
            //DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            //previewWindow.PreviewControl.DocumentSource = ESTIMATION_DIRECTReport;
            //previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //previewWindow.WindowState = WindowState.Maximized;
            //ESTIMATION_DIRECTReport.RequestParameters = false;
            //ESTIMATION_DIRECTReport.CreateDocument(true);
            //previewWindow.ShowDialog();
        }

        /// <summary>
        /// Add Entities into Display entities for ESTIMATION_DIRECT_ITEMProjection
        /// </summary>
        /// <param name="estimation_direct_item">Entity to copy from</param>
        /// <returns>Populated parent ESTIMATION_DIRECT_ITEMPOCO</returns>
        ESTIMATION_DIRECT_ITEMProjection AddPOCODisplayEntity(ESTIMATION_DIRECT_ITEMProjection estimation_direct_item)
        {
            ESTIMATION_DIRECT_ITEMProjection parentESTIMATION_DIRECT_ITEMPOCO = ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMProjection());
            parentESTIMATION_DIRECT_ITEMPOCO.GUID = estimation_direct_item.GUID;
            DataUtils.ShallowCopy(parentESTIMATION_DIRECT_ITEMPOCO, estimation_direct_item);

            parentESTIMATION_DIRECT_ITEMPOCO.RATE = ViewModelSource.Create(() => new RATE());
            DataUtils.ShallowCopy(parentESTIMATION_DIRECT_ITEMPOCO.RATE, estimation_direct_item.RATE);

            COMMODITY_GROUP_DIRECTProjection findCOMMODITY_GROUP_DIRECT = COMMODITY_GROUP_DIRECT_DisplayCollection.FirstOrDefault(x => x.GUID == estimation_direct_item.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
            if (findCOMMODITY_GROUP_DIRECT != null)
            {
                parentESTIMATION_DIRECT_ITEMPOCO.MANUAL_COMMODITY_GROUP_DIRECT = ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECT());
                DataUtils.ShallowCopy(parentESTIMATION_DIRECT_ITEMPOCO.MANUAL_COMMODITY_GROUP_DIRECT, findCOMMODITY_GROUP_DIRECT.COMMODITY_GROUP);
                
                PopulateCOMMODITY_GROUP_DIRECTChildren(parentESTIMATION_DIRECT_ITEMPOCO, findCOMMODITY_GROUP_DIRECT);
            }

            return parentESTIMATION_DIRECT_ITEMPOCO;
        }

        void PopulateCOMMODITY_GROUP_DIRECTChildren(ESTIMATION_DIRECT_ITEMProjection parentESTIMATION_DIRECT_ITEM, COMMODITY_GROUP_DIRECTProjection parentCOMMODITY_GROUP_DIRECT)
        {
            parentESTIMATION_DIRECT_ITEM.CHILD_ESTIMATION_DIRECT_ITEM.Clear();
            if (parentCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP.Count > 0)
            {
                foreach (COMMODITY_GROUP_DIRECTProjection childCOMMODITY_GROUP_DIRECTProjection in parentCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP)
                {
                    ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEMPOCO = ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMProjection());
                    DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.ESTIMATION_DIRECT_ITEM, parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM);
                    childESTIMATION_DIRECT_ITEMPOCO.RATE = ViewModelSource.Create(() => new RATE());
                    DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.RATE, parentESTIMATION_DIRECT_ITEM.RATE);
                    childESTIMATION_DIRECT_ITEMPOCO.ESTIMATION_DIRECT_ITEM.COMMENTS = CommonResources.Estimation_Item_Direct_ReadOnly;
                    childESTIMATION_DIRECT_ITEMPOCO.MANUAL_COMMODITY_GROUP_DIRECT = ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECT());
                    DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.MANUAL_COMMODITY_GROUP_DIRECT, childCOMMODITY_GROUP_DIRECTProjection.COMMODITY_GROUP);
                    
                    parentESTIMATION_DIRECT_ITEM.CHILD_ESTIMATION_DIRECT_ITEM.Add(childESTIMATION_DIRECT_ITEMPOCO);
                }
            }
        }

        protected override void OnClose(CancelEventArgs e)
        {
            refreshBackgroundWorker.CancelAsync();
            displayEntitiesRefreshBackgroundWorker.CancelAsync();
            userStateRestoreBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }
        #endregion
    }
}
