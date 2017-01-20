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
        BackgroundWorker userStateStoreBackgroundWorker;
        BackgroundWorker userStateRestoreBackgroundWorker;

        public bool TryUsingProjectRates { get; set; }
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

            userStateStoreBackgroundWorker = new BackgroundWorker();
            userStateStoreBackgroundWorker.DoWork += userStateStoreBackgroundWorker_DoWork;
            userStateStoreBackgroundWorker.WorkerSupportsCancellation = true;

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
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct);
        }

        Func<IRepositoryQuery<COMMODITY_GROUP_DIRECT>, IQueryable<COMMODITY_GROUP_DIRECT>> COMMODITY_GROUP_DIRECTProjectionFunc()
        {
            return query => query;
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
        List<Guid> SelectedEntitiesGuid = new List<Guid>();
        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (changedType == typeof(COMMODITY_CODE) || changedType == typeof(COMMODITY_GROUP_DIRECT) || changedType == typeof(ESTIMATION_DIRECT_ITEM))
            {
                if (!displayEntitiesRefreshBackgroundWorker.IsBusy)
                {
                    storeViewState();
                    if (changedType == typeof(COMMODITY_GROUP_DIRECT))
                        mainThreadDispatcher.BeginInvoke(new Action(() => COMMODITY_GROUP_DIRECTCollectionViewModel.Refresh()));
                    else if (changedType == typeof(COMMODITY_CODE))
                        mainThreadDispatcher.BeginInvoke(new Action(() => COMMODITY_CODECollectionViewModel.Refresh()));
                    else
                    {
                        if (sender.ToString() != MainViewModel.ToString())
                            mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                        else
                            mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));
                    }

                    displayEntitiesRefreshBackgroundWorker.RunWorkerAsync();
                }

                return;
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
            commodity_group_direct_DisplayCollection = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.COMMODITY_GROUP_DIRECT_DisplayCollection)));
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.COMMODITY_GROUP_DIRECT_NoWBSDisplayCollection)));
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplayEntities)));
        }

        void userStateStoreBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(10);
            Type changedType = (Type)e.Argument;
            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.storeViewState()));
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

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = this.CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = this.OnEntitiesSavedCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = this.ExistingProjectionEditCallBack;
            MainViewModel.NewRowAddUndoAndBeforeSaveCallBack = this.NewRowAddUndoAndBeforeSaveCallBack;
            MainViewModel.NewRowAddUndoAndAfterSaveCallBack = this.NewRowAddUndoAndAfterSaveCallBack;
            MainViewModel.EntitiesBeforeDeletionCallBack = this.EntitiesBeforeDeletion;
            COMMODITY_CODEProjectSpecificViewModel.DisplayEntities.ToList();
            MainViewModel.SetParentViewModel(this);
            refreshBackgroundWorker.RunWorkerAsync();
        }

        #region Collection Call Backs
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

        public bool NewRowAddUndoAndBeforeSaveCallBack(RowEventArgs e, ESTIMATION_DIRECT_ITEMProjection projectionEntity)
        {
            projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY = 1;
            return true;
        }

        public void NewRowAddUndoAndAfterSaveCallBack(RowEventArgs e, ESTIMATION_DIRECT_ITEMProjection projectionEntity)
        {
            NewRowAndExistingAddUndoAndSave(projectionEntity);
        }

        public void ExistingProjectionEditCallBack(ESTIMATION_DIRECT_ITEMProjection projectionEntity, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().COMMODITY_GROUP_CODE_SELECTION)
             || e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_COMMODITY_CODE))
                NewRowAndExistingAddUndoAndSave(projectionEntity);
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().ESTIMATED_QUANTITY))
                MultiplyChildrenEntityRates(projectionEntity, (decimal)e.OldValue, (decimal)e.Value);
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_INSTALLWORKPACK))
            {
                foreach (ESTIMATION_DIRECT_ITEMProjection childrenEntity in projectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
                {
                    Guid? oldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK;
                    Guid? newValue = (Guid?)e.Value;

                    childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, e.Column.FieldName, oldValue, newValue, EntityMessageType.Changed);
                    MainViewModel.Save(childrenEntity);
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_SUPPLYWORKPACK))
            {
                foreach (ESTIMATION_DIRECT_ITEMProjection childrenEntity in projectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
                {
                    Guid? oldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK;
                    Guid? newValue = (Guid?)e.Value;

                    childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, e.Column.FieldName, oldValue, newValue, EntityMessageType.Changed);
                    MainViewModel.Save(childrenEntity);
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().GUID_DISCIPLINE))
            {
                foreach (ESTIMATION_DIRECT_ITEMProjection childrenEntity in projectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
                {
                    Guid? oldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE;
                    Guid? newValue = (Guid?)e.Value;

                    childrenEntity.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, e.Column.FieldName, oldValue, newValue, EntityMessageType.Changed);
                    MainViewModel.Save(childrenEntity);
                }

            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_FREIGHT)
            || e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_SUPPLY)
            || e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().HOURS_INSTALL))
            {
                Guid? newCOMMODITY_CODEGuid = AddProjectSpecificCOMMODITY_CODE(projectionEntity);
                if (newCOMMODITY_CODEGuid != null)
                {
                    Guid? oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE;
                    Guid? newValue = newCOMMODITY_CODEGuid;
                    projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE = newCOMMODITY_CODEGuid;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity, e.Column.FieldName, oldValue, newValue, EntityMessageType.Changed);
                    MainViewModel.Save(projectionEntity);
                }
            }
        }

        private void NewRowAndExistingAddUndoAndSave(ESTIMATION_DIRECT_ITEMProjection projectionEntity)
        {
            if (projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE != null)
            {
                COMMODITY_CODE findCOMMODITY_CODE;
                //if(TryUsingProjectRates)
                //{
                //    findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID_PROJECT == loadPROJECT.GUID && x.GUID == projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE);
                //}

                //else
                    findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE);
                
                if (findCOMMODITY_CODE != null)
                {
                    object oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;
                    object newValue = projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY == 0 ? 1 : projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                    BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().ESTIMATED_QUANTITY), oldValue, newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY = 1;

                    oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT;
                    newValue = findCOMMODITY_CODE.RATE_FREIGHT;

                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                    BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_FREIGHT), oldValue, newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT = findCOMMODITY_CODE.RATE_FREIGHT;

                    oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY;
                    newValue = findCOMMODITY_CODE.RATE_SUPPLY;

                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                    BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().RATE_SUPPLY), oldValue, newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY = findCOMMODITY_CODE.RATE_SUPPLY;

                    oldValue = projectionEntity.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL;
                    newValue = findCOMMODITY_CODE.HOURS_INSTALL;

                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity,
                    BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().HOURS_INSTALL), oldValue, newValue, EntityMessageType.Changed);

                    projectionEntity.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL = findCOMMODITY_CODE.HOURS_INSTALL;
                }

                DeleteChildrenESTIMATION_DIRECT_ITEM(projectionEntity);
            }
            else if (projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT != null)
            {
                AddChildrenESTIMATION_DIRECT_ITEM(projectionEntity);
            }
        }

        private void SumParentEntityRates(ESTIMATION_DIRECT_ITEMProjection projectionEntity)
        {

        }

        private void MultiplyChildrenEntityRates(ESTIMATION_DIRECT_ITEMProjection parentProjectionEntity, decimal oldValue, decimal newValue)
        {
            if(oldValue == 0)
                return;

            foreach(ESTIMATION_DIRECT_ITEMProjection childrenEntity in parentProjectionEntity.CHILD_ESTIMATION_DIRECT_ITEM)
            {
                decimal childOldValue = childrenEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;
                decimal childNewValue = (childOldValue / oldValue) * newValue;
                childrenEntity.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY = childNewValue;
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().ESTIMATION_DIRECT_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEM().ESTIMATED_QUANTITY), childOldValue, childNewValue, EntityMessageType.Changed);
                MainViewModel.Save(childrenEntity);
            }
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletion(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            List<ESTIMATION_DIRECT_ITEMProjection> childrenEntities = new List<ESTIMATION_DIRECT_ITEMProjection>();
            List<ESTIMATION_DIRECT_ITEMProjection> parentEntitiesNotInList = new List<ESTIMATION_DIRECT_ITEMProjection>();

            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = entity.CHILD_ESTIMATION_DIRECT_ITEM;
                List<ESTIMATION_DIRECT_ITEMProjection> childrenEntitiesNotInDeletionCollection = new List<ESTIMATION_DIRECT_ITEMProjection>();
                foreach (var childrenEntityInTotal in childrenEntitiesInTotal)
                {
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);
                }

                ESTIMATION_DIRECT_ITEMProjection parentEntity = null;
                if (entity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != Guid.Empty)
                {
                    parentEntity = MainViewModel.Entities.FirstOrDefault(x => x.GUID == entity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT);
                    if (parentEntity != null)
                    {
                        if (!entities.Any(x => x.GUID == parentEntity.GUID))
                            parentEntitiesNotInList.Add(parentEntity);
                    }
                }

                childrenEntities = childrenEntities.Concat(childrenEntitiesNotInDeletionCollection).ToList();
            }

            //can't use bulk delete here due to stack overflow
            foreach (var childrenEntity in childrenEntities)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, null, null, null, EntityMessageType.Deleted);
                MainViewModel.Delete(childrenEntity);
            }
        }
        #endregion
        #endregion

        #region Local Methods
        public Action<ESTIMATION_DIRECT_ITEMProjection> SetIsRowExpanded;
        void AddChildrenESTIMATION_DIRECT_ITEM(ESTIMATION_DIRECT_ITEMProjection parentEstimation_Direct_Item)
        {
            COMMODITY_GROUP_DIRECTProjection findCOMMODITY_GROUP_DIRECT = COMMODITY_GROUP_DIRECT_DisplayCollection.FirstOrDefault(x => x.GUID == parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
            if (findCOMMODITY_GROUP_DIRECT != null)
            {
                if (findCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP.Count > 0)
                {
                    foreach (COMMODITY_GROUP_DIRECTProjection childCOMMODITY_GROUP_DIRECTProjection in findCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP)
                    {
                        ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEM = new ESTIMATION_DIRECT_ITEMProjection();
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE = childCOMMODITY_GROUP_DIRECTProjection.COMMODITY_GROUP.GUID_COMMODITYCODE;
                        if (parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE != null)
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE = parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE;
                        else
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE = childCOMMODITY_GROUP_DIRECTProjection.COMMODITY_GROUP.GUID_DISCIPLINE;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT = parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY = parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.ESTIMATED_QUANTITY;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK = parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_INSTALLWORKPACK;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK = parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_SUPPLYWORKPACK;

                        COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == childCOMMODITY_GROUP_DIRECTProjection.COMMODITY_GROUP.GUID_COMMODITYCODE);
                        if(findCOMMODITY_CODE != null)
                        {
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT = findCOMMODITY_CODE.RATE_FREIGHT;
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY = findCOMMODITY_CODE.RATE_SUPPLY;
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL = findCOMMODITY_CODE.HOURS_INSTALL;
                        }

                        MainViewModel.EntitiesUndoRedoManager.AddUndo(childESTIMATION_DIRECT_ITEM, null, null, null, EntityMessageType.Added);
                        MainViewModel.Save(childESTIMATION_DIRECT_ITEM);
                    }
                }
            }
        }

        void DeleteChildrenESTIMATION_DIRECT_ITEM(ESTIMATION_DIRECT_ITEMProjection parentEstimation_Direct_Item)
        {
            foreach (ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEM in parentEstimation_Direct_Item.CHILD_ESTIMATION_DIRECT_ITEM)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(childESTIMATION_DIRECT_ITEM, null, null, null, EntityMessageType.Deleted);
                MainViewModel.Delete(childESTIMATION_DIRECT_ITEM);
            }
        }


        DevExpress.Mvvm.IDialogService BulkColumnEditDialogService { get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); } }

        private Guid? AddProjectSpecificCOMMODITY_CODEGroup(ESTIMATION_DIRECT_ITEMProjection editedProjectionEntity)
        {
            if (editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != null)
            {
                ESTIMATION_DIRECT_ITEMProjection parentESTIMATION_DIRECT_ITEM = MainViewModel.Entities.First(x => x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL == editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT);
                IEnumerable<COMMODITY_CODE_ProjectSpecificProjection> findAllCOMMODITY_GROUP = COMMODITY_CODEProjectSpecificViewModel.DisplayEntities.Where(x => x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
                //Check whether project specific commodity code group exists
                if (findAllCOMMODITY_GROUP.Count() > 0)
                {
                    //no doubt there is only one commodity code grouping by that estimate original parent guid
                    //asks user if they want to create new or edit existing group

                    //if user wants to add
                        //find parent estimate direct item
                        //foreach parent estimate direct item children's commodity code add new group and project specific commodity code
                    //else if user wants to edit
                        //if (findAllCOMMODITY_GROUP.Count() > 1)
                            //Present commodity group selection dialog
                            //use selected commodity group
                            //foreach children in commodity group cross reference children estimation direct item for rates
                        //else
                            //Find specific commodity group using the code below
                            //COMMODITY_CODE_ProjectSpecificProjection findSpecificCOMMODITY_GROUP = COMMODITY_CODEProjectSpecificViewModel.DisplayEntities.FirstOrDefault(x => x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT && x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID == parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID);
                            //foreach children in commodity group cross reference children estimation direct item for rates
                    //end if user wants to add
                }
                //when there are no project specific commodity code group
                else
                {
                    //find parent estimate direct item
                    //foreach parent estimate direct item children's commodity code add new group and project specific commodity code
                }
            }
            //it is confirmed that current commodity code is standalone
            else
            {
                //ask if user wants to add or edit existing
                    //if user wants to add
                        //create new standalone project specific commodity code
                    //end if user wants to add
                //else if user wants to edit
                    //count number of project specific commodity code
                    //if project specific commodity code more than 1
                        //pop up dialog box for user to select standalone commodity code to edit
                    //else if project specific commodity code is 1
                        //edit current project specific commodity code
                    //end if project specific commodity code more than 1
                //end if user wants to edit
            }

            return Guid.Empty;
        }

        private Guid? AddProjectSpecificCOMMODITY_CODE(ESTIMATION_DIRECT_ITEMProjection editedProjectionEntity)
        {
            if (editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE == null)
                return null;

            COMMODITY_CODE currentCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == (Guid)editedProjectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_CODE);
            if (currentCOMMODITY_CODE != null)
            {
                if (currentCOMMODITY_CODE.GUID_PROJECT != null)
                {
                    if (MessageBoxService.ShowMessage(CommonResources.Estimation_Item_Direct_EditOrAddNewCommodityCode, CommonResources.Confirmation_Caption, MessageButton.YesNo) == MessageResult.Yes)
                    {
                        IEnumerable<COMMODITY_CODE> projectSpecific_COMMODITY_CODES = COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.FULLCODE == currentCOMMODITY_CODE.FULLCODE);
                        if (projectSpecific_COMMODITY_CODES.Count() > 1)
                        {
                            return MultipleProjectSpecificCommodityCodeSelection(currentCOMMODITY_CODE, projectSpecific_COMMODITY_CODES, editedProjectionEntity);
                        }
                        else
                        {
                            return UpdateAndSaveCOMMODITY_CODE(currentCOMMODITY_CODE, null, editedProjectionEntity, false);
                        }
                    }
                    else
                    {
                        return UpdateAndSaveCOMMODITY_CODE(new COMMODITY_CODE(), currentCOMMODITY_CODE, editedProjectionEntity, true);
                    }
                }
                else
                {
                    IEnumerable<COMMODITY_CODE> projectSpecific_COMMODITY_CODES = COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.FULLCODE == currentCOMMODITY_CODE.FULLCODE);
                    if (projectSpecific_COMMODITY_CODES.Count() > 0)
                    {
                        if (MessageBoxService.ShowMessage(CommonResources.Estimation_Item_Direct_EditOrAddNewCommodityCode, CommonResources.Confirmation_Caption, MessageButton.YesNo) == MessageResult.Yes)
                        {
                            return MultipleProjectSpecificCommodityCodeSelection(currentCOMMODITY_CODE, projectSpecific_COMMODITY_CODES, editedProjectionEntity);
                        }
                        else
                        {
                            return UpdateAndSaveCOMMODITY_CODE(new COMMODITY_CODE(), currentCOMMODITY_CODE, editedProjectionEntity, true);
                        }
                    }
                    else
                    {
                        MessageBoxService.ShowMessage(CommonResources.Estimation_Item_Direct_ProjectSpecificCommodityCodeCreated);
                        return UpdateAndSaveCOMMODITY_CODE(new COMMODITY_CODE(), currentCOMMODITY_CODE, editedProjectionEntity, true);
                    }
                }
            }

            return null;
        }

        private Guid MultipleProjectSpecificCommodityCodeSelection(COMMODITY_CODE currentCOMMODITY_CODE, IEnumerable<COMMODITY_CODE> projectSpecific_COMMODITY_CODES, ESTIMATION_DIRECT_ITEMProjection ratesESTIMATION_DIRECT_ITEM)
        {
            IEnumerable<COMMODITY_CODE> projectSpecific_COMMODITY_CODESExcludingCurrent = COMMODITY_CODECollection.Where(x => x.GUID != currentCOMMODITY_CODE.GUID && x.GUID_PROJECT == loadPROJECT.GUID && x.FULLCODE == currentCOMMODITY_CODE.FULLCODE);
            var commodity_codeSelectionViewModel = COMMODITY_CODESelectionViewModel.Create(projectSpecific_COMMODITY_CODES, currentCOMMODITY_CODE, DISCIPLINECollection);
            if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Select Item to edit", "COMMODITY_CODESelectionView", commodity_codeSelectionViewModel) == MessageResult.OK)
            {
                if (commodity_codeSelectionViewModel.SelectedItem != null)
                {
                    return UpdateAndSaveCOMMODITY_CODE((COMMODITY_CODE)commodity_codeSelectionViewModel.SelectedItem, null, ratesESTIMATION_DIRECT_ITEM, false);
                }
            }
            else
            {
                return UpdateAndSaveCOMMODITY_CODE(currentCOMMODITY_CODE, null, ratesESTIMATION_DIRECT_ITEM, false);
            }

            return Guid.Empty;
        }

        private Guid UpdateAndSaveCOMMODITY_CODE(COMMODITY_CODE newOrExistingCOMMODITY_CODE, COMMODITY_CODE creationCOMMODITY_CODE, ESTIMATION_DIRECT_ITEMProjection ratesESTIMATION_DIRECT_ITEM, bool populateSourceDescription)
        {
            if (creationCOMMODITY_CODE != null)
            {
                DataUtils.ShallowCopy(newOrExistingCOMMODITY_CODE, creationCOMMODITY_CODE);
                newOrExistingCOMMODITY_CODE.GUID = Guid.Empty;
                newOrExistingCOMMODITY_CODE.GUID_PROJECT = loadPROJECT.GUID;
                newOrExistingCOMMODITY_CODE.GUID_PARENT = Guid.Empty;
            }

            newOrExistingCOMMODITY_CODE.RATE_FREIGHT = ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT;
            newOrExistingCOMMODITY_CODE.RATE_SUPPLY = ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY;
            newOrExistingCOMMODITY_CODE.HOURS_INSTALL = ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL;

            if (ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != null)
            {
                ESTIMATION_DIRECT_ITEMProjection parentESTIMATION_DIRECT_ITEM = DisplayEntities.FirstOrDefault(x => x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL == ratesESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT);
                if (parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT != null)
                {
                    COMMODITY_GROUP_DIRECT parentCOMMODITY_GROUP_DIRECT = COMMODITY_GROUP_DIRECTCollection.FirstOrDefault(x => x.GUID == parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
                    string groupDescription = parentCOMMODITY_GROUP_DIRECT.DESCRIPTION;
                    Guid groupGuid = parentCOMMODITY_GROUP_DIRECT.GUID;

                    if (parentCOMMODITY_GROUP_DIRECT != null)
                    {
                        newOrExistingCOMMODITY_CODE.COMMODITY_GROUP_DESC = groupDescription;
                        newOrExistingCOMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT = groupGuid;
                        newOrExistingCOMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID = COMMODITY_CODEProjectSpecificViewModel.DisplayEntities.Count(x => x.COMMODITY_CODE.GUID_COMMODITY_GROUP_DIRECT == groupGuid && x.COMMODITY_CODE.COMMODITY_GROUP_DIRECT_ID != parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.COMMODITY_GROUP_DIRECT_ID);
                    }
                }
            }

            COMMODITY_CODECollectionViewModel.Save(newOrExistingCOMMODITY_CODE);
            return newOrExistingCOMMODITY_CODE.GUID;
        }

        protected override void OnClose(CancelEventArgs e)
        {
            refreshBackgroundWorker.CancelAsync();
            displayEntitiesRefreshBackgroundWorker.CancelAsync();
            userStateRestoreBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }
        #endregion

        #region View Behavior
        public ESTIMATION_DIRECT_ITEMProjection CreateNewProjectionFromNewEntityCallBack(ESTIMATION_DIRECT_ITEM entity)
        {
            return new ESTIMATION_DIRECT_ITEMProjection();
        }
        #endregion

        #region View Commands
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

        public virtual bool CanBulkDelete()
        {
            return MainViewModel != null && MainViewModel.Entities != null && MainViewModel.Entities.Count > 0 && !IsLoading && SelectedEntities.Count > 0 && SelectedEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT == null;
        }

        public void BulkDelete()
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.BaseBulkDelete(this.SelectedEntities);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
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

        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITY_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<COMMODITY_CODE>();
            }
        }

        COMMODITY_CODEProjectSpecificViewModelWrapper commodity_codeProjectSpecificViewModel;
        public COMMODITY_CODEProjectSpecificViewModelWrapper COMMODITY_CODEProjectSpecificViewModel
        {
            get
            {
                if (commodity_codeProjectSpecificViewModel == null && this.loadPROJECT != null)
                {
                    commodity_codeProjectSpecificViewModel = COMMODITY_CODEProjectSpecificViewModelWrapper.Create();
                    commodity_codeProjectSpecificViewModel.SetParentViewModel(this);
                    ISupportParameter baselineSupportParameterObj = commodity_codeProjectSpecificViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(this.loadPROJECT, new CommodityCodeTypeClass(CommodityCodeType.Direct));
                }

                return commodity_codeProjectSpecificViewModel;
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
                    IEnumerable<ESTIMATION_DIRECT_ITEMProjection> parentESTIMATION_DIRECT_ITEMS = MainViewModel.Entities.Where(x => x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT == null).AsEnumerable();
                    IEnumerable<ESTIMATION_DIRECT_ITEMProjection> AllChildESTIMATION_DIRECT_ITEMS = MainViewModel.Entities.Where(x => x.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT != null).AsEnumerable();
                    foreach (ESTIMATION_DIRECT_ITEMProjection parentESTIMATION_DIRECT_ITEM in parentESTIMATION_DIRECT_ITEMS)
                    {
                        ESTIMATION_DIRECT_ITEMProjection parentESTIMATION_DIRECT_ITEMSPOCO = ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMProjection());
                        parentESTIMATION_DIRECT_ITEMSPOCO.GUID = parentESTIMATION_DIRECT_ITEM.GUID;
                        DataUtils.ShallowCopy(parentESTIMATION_DIRECT_ITEMSPOCO.ESTIMATION_DIRECT_ITEM, parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM);
                        parentESTIMATION_DIRECT_ITEM.RATE = new RATE();
                        DataUtils.ShallowCopy(parentESTIMATION_DIRECT_ITEMSPOCO.RATE, parentESTIMATION_DIRECT_ITEM.RATE);
                        displayEntities.Add(parentESTIMATION_DIRECT_ITEMSPOCO);
                    }

                    foreach (ESTIMATION_DIRECT_ITEMProjection displayEntity in displayEntities)
                    {
                        IEnumerable<ESTIMATION_DIRECT_ITEMProjection> childESTIMATION_DIRECT_ITEMS = AllChildESTIMATION_DIRECT_ITEMS.Where(y => y.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT == displayEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL);
                        if(childESTIMATION_DIRECT_ITEMS.Count() > 0)
                        {
                            displayEntity.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT = childESTIMATION_DIRECT_ITEMS.Sum(x => x.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT);
                            displayEntity.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY = childESTIMATION_DIRECT_ITEMS.Sum(x => x.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY);
                            displayEntity.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL = childESTIMATION_DIRECT_ITEMS.Sum(x => x.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL);
                            displayEntity.RATE = new RATE();
                            displayEntity.RATE.RATE1 = childESTIMATION_DIRECT_ITEMS.Where(x => x.RATE != null).Sum(y => y.RATE.RATE1);

                            foreach (ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEM in childESTIMATION_DIRECT_ITEMS)
                            {
                                ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEMPOCO = ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMProjection());
                                childESTIMATION_DIRECT_ITEMPOCO.GUID = childESTIMATION_DIRECT_ITEM.GUID;
                                DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.ESTIMATION_DIRECT_ITEM, childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM);
                                childESTIMATION_DIRECT_ITEMPOCO.RATE = new RATE();
                                DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.RATE, childESTIMATION_DIRECT_ITEM.RATE);
                                displayEntity.CHILD_ESTIMATION_DIRECT_ITEM.Add(childESTIMATION_DIRECT_ITEMPOCO);
                            }
                        }
                    }

                    if (!userStateRestoreBackgroundWorker.IsBusy)
                        userStateRestoreBackgroundWorker.RunWorkerAsync();
                }

                return displayEntities;
            }
        }


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

                    foreach (COMMODITY_CODE commodity_code in COMMODITY_CODECollection)
                    {
                        COMMODITY_GROUP_DIRECTProjection COMMODITY_GROUP_DIRECTPOCO = ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTProjection());
                        COMMODITY_GROUP_DIRECTPOCO.GUID = commodity_code.GUID;
                        COMMODITY_GROUP_DIRECTPOCO.GUID_PROJECT = commodity_code.GUID_PROJECT;
                        COMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP.DESCRIPTION = commodity_code.FULLCODE;
                        COMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP.GUID = Guid.Empty;
                        COMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP.GUID_COMMODITYCODE = commodity_code.GUID;
                        COMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP.GUID_DISCIPLINE = commodity_code.GUID_DISCIPLINE;
                        COMMODITY_GROUP_DIRECTPOCO.COMMODITY_GROUP.GUID_PARENT = Guid.Empty;

                        commodity_group_direct_DisplayCollection.Add(COMMODITY_GROUP_DIRECTPOCO);
                    }
                }

                return commodity_group_direct_DisplayCollection;
            }
        }

        public IEnumerable<COMMODITY_GROUP_DIRECTProjection> COMMODITY_GROUP_DIRECT_NoWBSDisplayCollection
        {
            get
            {
                return COMMODITY_GROUP_DIRECT_DisplayCollection.Where(x => x.COMMODITY_GROUP.GUID_COMMODITYCODE != null);
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
                    collection = collection.OrderBy(x => x.GUID_PROJECT).OrderBy(x => x.CODE);
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
        #endregion
    }
}
