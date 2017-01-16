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
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = this.CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = this.OnEntitiesSavedCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = this.ExistingProjectionEditCallBack;
            MainViewModel.EntitiesBeforeDeletionCallBack = this.EntitiesBeforeDeletion;
            //MainViewModel.PostSave = this.PostSave;

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
            if (e.Column.FieldName != BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMProjection().COMMODITY_GROUP_CODE_SELECTION))
                return;

            if (projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT != null)
            {
                AddChildrenESTIMATION_DIRECT_ITEM(projectionEntity);
            }
            else
            {
                DeleteChildrenESTIMATION_DIRECT_ITEM(projectionEntity);
            }
        }


        private void PostSave(ESTIMATION_DIRECT_ITEMProjection projectionEntity, bool isNewEntity)
        {
            if(isNewEntity)
            {
                if (projectionEntity.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT != null)
                {
                    AddChildrenESTIMATION_DIRECT_ITEM(projectionEntity);
                }
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
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_DISCIPLINE = childCOMMODITY_GROUP_DIRECTProjection.COMMODITY_GROUP.GUID_DISCIPLINE;
                        childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT = parentEstimation_Direct_Item.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL;

                        COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == childCOMMODITY_GROUP_DIRECTProjection.COMMODITY_GROUP.GUID_COMMODITYCODE);
                        if(findCOMMODITY_CODE != null)
                        {
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_FREIGHT = findCOMMODITY_CODE.RATE_FREIGHT;
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.RATE_SUPPLY = findCOMMODITY_CODE.RATE_SUPPLY;
                            childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.HOURS_INSTALL = findCOMMODITY_CODE.HOURS_INSTALL;
                        }

                        MainViewModel.Save(childESTIMATION_DIRECT_ITEM);
                    }
                }
            }
        }

        void DeleteChildrenESTIMATION_DIRECT_ITEM(ESTIMATION_DIRECT_ITEMProjection parentEstimation_Direct_Item)
        {
            foreach (ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEM in parentEstimation_Direct_Item.CHILD_ESTIMATION_DIRECT_ITEM)
            {
                MainViewModel.Delete(childESTIMATION_DIRECT_ITEM);
            }
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

        void AddCOMMODITY_GROUP_DIRECTChildren(ESTIMATION_DIRECT_ITEMProjection parentESTIMATION_DIRECT_ITEM)
        {
            COMMODITY_GROUP_DIRECTProjection findCOMMODITY_GROUP_DIRECT = COMMODITY_GROUP_DIRECT_DisplayCollection.FirstOrDefault(x => x.GUID == parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT);
            if (findCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP.Count > 0)
            {
                foreach (COMMODITY_GROUP_DIRECTProjection childCOMMODITY_GROUP_DIRECTProjection in findCOMMODITY_GROUP_DIRECT.CHILD_COMMODITY_GROUP)
                {
                    ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEM = new ESTIMATION_DIRECT_ITEMProjection();
                    DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM, parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM);
                    childESTIMATION_DIRECT_ITEM.GUID = Guid.Empty;
                    childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID = Guid.Empty;
                    childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT = parentESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL;
                    childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM.GUID_COMMODITY_GROUP_DIRECT = childCOMMODITY_GROUP_DIRECTProjection.COMMODITY_GROUP.GUID;
                    MainViewModel.Save(childESTIMATION_DIRECT_ITEM);
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
            return MainViewModel != null && MainViewModel.Entities != null && MainViewModel.Entities.Count > 0 && !IsLoading && SelectedEntities.Count > 0;
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
                        displayEntities.Add(parentESTIMATION_DIRECT_ITEMSPOCO);
                    }

                    foreach (ESTIMATION_DIRECT_ITEMProjection displayEntity in displayEntities)
                    {
                        IEnumerable<ESTIMATION_DIRECT_ITEMProjection> childESTIMATION_DIRECT_ITEMS = AllChildESTIMATION_DIRECT_ITEMS.Where(y => y.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL_PARENT == displayEntity.ESTIMATION_DIRECT_ITEM.GUID_ORIGINAL);
                        foreach (ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEM in childESTIMATION_DIRECT_ITEMS)
                        {
                            ESTIMATION_DIRECT_ITEMProjection childESTIMATION_DIRECT_ITEMPOCO = ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMProjection());
                            childESTIMATION_DIRECT_ITEMPOCO.GUID = childESTIMATION_DIRECT_ITEM.GUID;
                            DataUtils.ShallowCopy(childESTIMATION_DIRECT_ITEMPOCO.ESTIMATION_DIRECT_ITEM, childESTIMATION_DIRECT_ITEM.ESTIMATION_DIRECT_ITEM);
                            displayEntity.CHILD_ESTIMATION_DIRECT_ITEM.Add(childESTIMATION_DIRECT_ITEMPOCO);
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
        #endregion
    }
}
