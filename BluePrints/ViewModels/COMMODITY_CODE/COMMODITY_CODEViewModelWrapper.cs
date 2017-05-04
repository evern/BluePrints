using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITY_CODES collection view model.
    /// </summary>
    public partial class COMMODITY_CODESViewModelWrapper :
        CollectionViewModelsWrapper
        <COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODESCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODESViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODESViewModelWrapper(unitOfWorkFactory));
        }

        private DispatcherTimer delayedCOMMODITY_CODEPopulateDispatcher;

        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODESCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODESCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODESViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private CommodityCodeType loadCommodityCodeType;
        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private BackgroundWorker refreshBackgroundWorker;

        protected override void InitializeParameters(object parameter)
        {
            refreshBackgroundWorker = new BackgroundWorker();
            refreshBackgroundWorker.DoWork += refreshBackgroundWorker_DoWork;
            refreshBackgroundWorker.WorkerSupportsCancellation = true;

            var receiveParameter =
                (OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>) parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            var loadCommodityCodeTypeClass = receiveParameter.GetSecondEntity();

            loadCommodityCodeType = loadCommodityCodeTypeClass.commodityCodeType;
            delayedCOMMODITY_CODEPopulateDispatcher = new DispatcherTimer();
            delayedCOMMODITY_CODEPopulateDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedCOMMODITY_CODEPopulateDispatcher.Tick += delayedCOMMODITY_CODEPopulateDispatcher_Tick;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, null, x => { });
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES, null, x => { });
            loaderCollection.AddLoaderDescription<INDIRECT_TYPE, INDIRECT_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.INDIRECT_TYPES);
            loaderCollection.AddLoaderDescription<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isPROJECTSpecific
        {
            get { return loadPROJECT != null; }
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isPROJECTSpecific)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == Guid.Empty);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>>
            ConstructMainViewModelProjection()
        {
            if (isPROJECTSpecific)
                return
                    query =>
                        query.Where(
                            x => x.GUID_PROJECT == loadPROJECT.GUID && x.COMMODITYCODETYPE == loadCommodityCodeType);
            else
                return query => query.Where(x => x.GUID_PROJECT == null && x.COMMODITYCODETYPE == loadCommodityCodeType);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_CODE> entities)
        {
            MainViewModel.OnAfterEntitiesDeletedCallBack = EntitiesAfterDeletion;
            MainViewModel.OnBeforeEntitiesDeleteCallBack = EntitiesBeforeDeletion;
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitiesSaved;
            MainViewModel.OnAfterTreelistExistingRowAddUndoAndSaveCallBack = PostTreeListExistingRowAddUndoAndSave;

            MainViewModel.SetParentViewModel(this);
            if (loadCommodityCodeType == CommodityCodeType.Design)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => ShowDEPARTMENT()));
            }
            else if (loadCommodityCodeType == CommodityCodeType.Direct)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => ShowDISCIPLINE()));
                mainThreadDispatcher.BeginInvoke(new Action(() => ShowDIRECT_RATES()));
            }
            else
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => ShowINDIRECT_TYPE()));
                mainThreadDispatcher.BeginInvoke(new Action(() => ShowINDIRECT_RATES()));
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            mainThreadDispatcher.BeginInvoke(new Action(() => delayedCOMMODITY_CODEPopulateDispatcher.Start()));
        }

        private void refreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.RefreshWithoutClearingUndoManager()));
        }

        private void delayedCOMMODITY_CODEPopulateDispatcher_Tick(object sender, EventArgs e)
        {
            delayedCOMMODITY_CODEPopulateDispatcher.Stop();

            if (loadCommodityCodeType == CommodityCodeType.Design)
            {
                var populateDEPARTMENTS =
                    DEPARTMENTCollection.Where(x => x.ISDESIGNCOMMODITY == true);
                foreach (var populateDEPARTMENT in populateDEPARTMENTS)
                    if (
                        !MainViewModel.Entities.Any(
                            x => x.GUID_PARENT == Guid.Empty && x.GUID_DEPARTMENT == populateDEPARTMENT.GUID))
                    {
                        var newCOMMODITY_CODE = new COMMODITY_CODE();
                        newCOMMODITY_CODE.CODE = populateDEPARTMENT.CODE;
                        newCOMMODITY_CODE.FULLCODE = populateDEPARTMENT.CODE;
                        newCOMMODITY_CODE.NAME = populateDEPARTMENT.NAME;
                        newCOMMODITY_CODE.GUID_DEPARTMENT = populateDEPARTMENT.GUID;
                        MainViewModel.Save(newCOMMODITY_CODE);
                    }
            }
            else if (loadCommodityCodeType == CommodityCodeType.Direct)
            {
                if (!isPROJECTSpecific)
                    foreach (var populateDISCIPLINE in DISCIPLINECollection)
                        if (
                            !MainViewModel.Entities.Any(
                                x => x.GUID_PARENT == Guid.Empty && x.GUID_DISCIPLINE == populateDISCIPLINE.GUID))
                        {
                            var newCOMMODITY_CODE = new COMMODITY_CODE();
                            newCOMMODITY_CODE.CODE = populateDISCIPLINE.CODE;
                            newCOMMODITY_CODE.FULLCODE = populateDISCIPLINE.CODE;
                            newCOMMODITY_CODE.NAME = populateDISCIPLINE.NAME;
                            newCOMMODITY_CODE.GUID_DISCIPLINE = populateDISCIPLINE.GUID;
                            MainViewModel.Save(newCOMMODITY_CODE);
                        }
            }
            else if (loadCommodityCodeType == CommodityCodeType.Indirect)
            {
                foreach (var populateINDIRECT_TYPE in INDIRECT_TYPECollection)
                    if (
                        !MainViewModel.Entities.Any(
                            x => x.GUID_PARENT == Guid.Empty && x.GUID_INDIRECTTYPE == populateINDIRECT_TYPE.GUID))
                    {
                        var newCOMMODITY_CODE = new COMMODITY_CODE();
                        newCOMMODITY_CODE.CODE = populateINDIRECT_TYPE.CODE;
                        newCOMMODITY_CODE.FULLCODE = populateINDIRECT_TYPE.CODE;
                        newCOMMODITY_CODE.NAME = populateINDIRECT_TYPE.NAME;
                        newCOMMODITY_CODE.GUID_INDIRECTTYPE = populateINDIRECT_TYPE.GUID;
                        MainViewModel.Save(newCOMMODITY_CODE);
                    }
            }
        }

        #region Collection Call Backs

        public void PostTreeListExistingRowAddUndoAndSave(TreeListCellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new COMMODITY_CODE().CODE))
            {
                MainViewModel.EntitiesUndoRedoManager.RewindActionId(1);
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                var editedCOMMODITY_CODE = (COMMODITY_CODE) e.Row;
                AddUndoOnFULLCODEChanges(editedCOMMODITY_CODE);
                RecurseRenameChildrenFULLCODE(editedCOMMODITY_CODE.GUID);
                var childrenCOMMODITY_CODES = RecurseFindChildren(editedCOMMODITY_CODE,
                    MainViewModel.Entities);
                var childrenCOMMODITY_CODESList = new List<COMMODITY_CODE>(childrenCOMMODITY_CODES);
                MainViewModel.BulkSave(childrenCOMMODITY_CODESList);
            }
        }

        private void OnBeforeEntitiesSaved(COMMODITY_CODE entity)
        {
            entity.COMMODITYCODETYPE = loadCommodityCodeType;
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletion(IEnumerable<COMMODITY_CODE> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            var childrenEntities = new List<COMMODITY_CODE>();
            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = RecurseFindChildren(entity, MainViewModel.Entities);
                var childrenEntitiesNotInDeletionCollection = new List<COMMODITY_CODE>();
                foreach (var childrenEntityInTotal in childrenEntitiesInTotal)
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);

                childrenEntities = childrenEntities.Concat(childrenEntitiesNotInDeletionCollection).ToList();
            }

            uniqueGUID_PARENTS = new List<Guid>();
            //can't use bulk delete here due to stack overflow
            foreach (var childrenEntity in childrenEntities)
            {
                if (!uniqueGUID_PARENTS.Any(x => x == childrenEntity.GUID_PARENT))
                    uniqueGUID_PARENTS.Add(childrenEntity.GUID_PARENT);

                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, null, null, null,
                    EntityMessageType.Deleted);
                MainViewModel.Delete(childrenEntity);
            }
        }

        //Reorder tree after deletion
        private void EntitiesAfterDeletion(IEnumerable<COMMODITY_CODE> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            //uniqueGUID_PARENTS is initialized in EntitiesBeforeDeletion
            foreach (var entity in entities)
                if (!uniqueGUID_PARENTS.Any(x => x == entity.GUID_PARENT))
                    uniqueGUID_PARENTS.Add(entity.GUID_PARENT);

            MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
            ReorderAndSave(uniqueGUID_PARENTS);
        }

        #endregion

        #endregion

        #region View Behavior

        public Action ShowDISCIPLINE;
        public Action ShowDEPARTMENT;
        public Action ShowINDIRECT_TYPE;
        public Action ShowHOURSAWEEK;
        public Action ShowDIRECT_RATES;
        public Action ShowINDIRECT_RATES;

        private Guid GUID_PARENTOldValue;
        private List<Guid> uniqueGUID_PARENTS; //stores dropping entity parent guid before it gets reassigned

        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            uniqueGUID_PARENTS = new List<Guid>();
            GUID_PARENTOldValue = Guid.Empty;

            if (e.TargetNode != null)
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
                foreach (var obj in e.DraggedRows)
                {
                    var editCommodityCode = e.SourceManager.GetObject(obj) as COMMODITY_CODE;

                    GUID_PARENTOldValue = editCommodityCode.GUID_PARENT;
                    if (!uniqueGUID_PARENTS.Any(x => x == GUID_PARENTOldValue))
                        uniqueGUID_PARENTS.Add(GUID_PARENTOldValue);
                }
            }
        }

        public void dragDropManager_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDroppedEventArgs e)
        {
            var newParentGuid = Guid.Empty;
            if (e.TargetNode != null)
            {
                foreach (TreeListNode obj in e.DraggedRows)
                {
                    var droppedCOMMODITY_CODE = obj.Content as COMMODITY_CODE;
                    var targetCommodityCode = e.TargetNode.Content as COMMODITY_CODE;

                    if (e.DropTargetType == DropTargetType.InsertRowsAfter)
                    {
                        droppedCOMMODITY_CODE.SORTORDER = targetCommodityCode.SORTORDER + 1;
                    }
                    else if (e.DropTargetType == DropTargetType.InsertRowsBefore)
                    {
                        droppedCOMMODITY_CODE.SORTORDER = targetCommodityCode.SORTORDER - 1;
                    }
                    else
                    {
                        var targetCommodityCodeChild =
                            MainViewModel.Entities.Where(x => x.GUID_PARENT == targetCommodityCode.GUID);

                        var maxTargetChildrenOrder = 0;
                        if (targetCommodityCodeChild.Count() > 0)
                            maxTargetChildrenOrder = targetCommodityCodeChild.Max(x => x.SORTORDER);

                        maxTargetChildrenOrder += 1;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedCOMMODITY_CODE,
                            BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_DISCIPLINE),
                            droppedCOMMODITY_CODE.GUID_DISCIPLINE, targetCommodityCode.GUID_DISCIPLINE,
                            EntityMessageType.Changed);
                        droppedCOMMODITY_CODE.GUID_DISCIPLINE = targetCommodityCode.GUID_DISCIPLINE;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedCOMMODITY_CODE,
                            BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_DEPARTMENT),
                            droppedCOMMODITY_CODE.GUID_DEPARTMENT, targetCommodityCode.GUID_DEPARTMENT,
                            EntityMessageType.Changed);
                        droppedCOMMODITY_CODE.GUID_DEPARTMENT = targetCommodityCode.GUID_DEPARTMENT;

                        droppedCOMMODITY_CODE.SORTORDER = maxTargetChildrenOrder;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedCOMMODITY_CODE,
                            BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_PARENT), GUID_PARENTOldValue,
                            droppedCOMMODITY_CODE.GUID_PARENT, EntityMessageType.Changed);
                    }

                    newParentGuid = droppedCOMMODITY_CODE.GUID_PARENT;
                }

                if (!uniqueGUID_PARENTS.Any(x => x == newParentGuid))
                    uniqueGUID_PARENTS.Add(newParentGuid);

                ReorderAndSave(uniqueGUID_PARENTS);
            }
        }

        private IEnumerable<COMMODITY_CODE> ReorderAndSave(Guid guid_parent, bool dontSave = false)
        {
            IEnumerable<COMMODITY_CODE> childCommodityCodes =
                MainViewModel.Entities.Where(x => x.GUID_PARENT == guid_parent).OrderBy(x => x.SORTORDER).ToList();
            var childCommodityCodesList = new List<COMMODITY_CODE>(childCommodityCodes);

            var commodityCodeOrderCount = 10;
            foreach (var childCommodityCode in childCommodityCodesList)
            {
                if (childCommodityCode.SORTORDER != commodityCodeOrderCount)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childCommodityCode,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().SORTORDER),
                        Convert.ToInt32(Math.Round(Convert.ToDecimal(childCommodityCode.SORTORDER))),
                        commodityCodeOrderCount, EntityMessageType.Changed);
                    childCommodityCode.SORTORDER = commodityCodeOrderCount;
                    int tryParseInt;
                    if (childCommodityCode.CODE == "temp")
                        childCommodityCode.CODE = GenerateOrderString(childCommodityCode.SORTORDER);
                    else
                        childCommodityCode.CODE = int.TryParse(childCommodityCode.CODE, out tryParseInt)
                            ? AddUndoOnCODEChanges(childCommodityCode, GenerateOrderString(childCommodityCode.SORTORDER))
                            : childCommodityCode.CODE;
                }

                if (!MainViewModel.Entities.Any(x => x.GUID_PARENT == childCommodityCode.GUID))
                {
                    var oldBoolValue = childCommodityCode.ISQUANTIFIABLE;
                    var newBoolValue = true;
                    childCommodityCode.ISQUANTIFIABLE = newBoolValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childCommodityCode,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().ISQUANTIFIABLE), oldBoolValue,
                        newBoolValue, EntityMessageType.Changed);
                }
                else
                {
                    var oldBoolValue = childCommodityCode.ISQUANTIFIABLE;
                    var newBoolValue = false;
                    childCommodityCode.ISQUANTIFIABLE = newBoolValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childCommodityCode,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().ISQUANTIFIABLE), oldBoolValue,
                        newBoolValue, EntityMessageType.Changed);
                }

                commodityCodeOrderCount += 10;
            }

            RecurseRenameChildrenFULLCODE(guid_parent);

            var parentCOMMODITY_CODE = MainViewModel.Entities.FirstOrDefault(x => x.GUID == guid_parent);
            if (parentCOMMODITY_CODE != null)
            {
                decimal? newValue = null;
                decimal? oldValue = null;

                if (parentCOMMODITY_CODE.RATE_FREIGHT != null)
                {
                    oldValue = parentCOMMODITY_CODE.RATE_FREIGHT;
                    parentCOMMODITY_CODE.RATE_FREIGHT = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY_CODE,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().RATE_FREIGHT), oldValue, newValue,
                        EntityMessageType.Changed);
                }

                if (parentCOMMODITY_CODE.RATE_SUPPLY != null)
                {
                    oldValue = parentCOMMODITY_CODE.RATE_SUPPLY;
                    parentCOMMODITY_CODE.RATE_SUPPLY = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY_CODE,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().RATE_SUPPLY), oldValue, newValue,
                        EntityMessageType.Changed);
                }

                if (parentCOMMODITY_CODE.HOURS_INSTALL != null)
                {
                    oldValue = parentCOMMODITY_CODE.HOURS_INSTALL;
                    parentCOMMODITY_CODE.HOURS_INSTALL = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY_CODE,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().HOURS_INSTALL), oldValue, newValue,
                        EntityMessageType.Changed);
                }

                if (parentCOMMODITY_CODE.RATE_PLANT != null)
                {
                    oldValue = parentCOMMODITY_CODE.RATE_PLANT;
                    parentCOMMODITY_CODE.RATE_PLANT = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY_CODE,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().RATE_PLANT), oldValue, newValue,
                        EntityMessageType.Changed);
                }

                if (parentCOMMODITY_CODE.ISQUANTIFIABLE)
                {
                    var oldBoolValue = parentCOMMODITY_CODE.ISQUANTIFIABLE;
                    parentCOMMODITY_CODE.ISQUANTIFIABLE = false;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY_CODE,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().ISQUANTIFIABLE), oldBoolValue, false,
                        EntityMessageType.Changed);
                }

                if (oldValue != null)
                    childCommodityCodesList.Add(parentCOMMODITY_CODE);
            }

            if (!dontSave)
                MainViewModel.BulkSave(childCommodityCodesList);

            return childCommodityCodesList;
        }

        private void RecurseRenameChildrenFULLCODE(Guid guid_parent)
        {
            IEnumerable<COMMODITY_CODE> childCommodityCodes =
                MainViewModel.Entities.Where(x => x.GUID_PARENT == guid_parent).OrderBy(x => x.SORTORDER).ToList();
            foreach (var childCommodityCode in childCommodityCodes)
            {
                if (childCommodityCode.CODE == "temp")
                    childCommodityCode.FULLCODE = GenerateFullCode(childCommodityCode);
                else
                    childCommodityCode.FULLCODE = AddUndoOnFULLCODEChanges(childCommodityCode);

                RecurseRenameChildrenFULLCODE(childCommodityCode.GUID);
            }
        }

        private string AddUndoOnCODEChanges(COMMODITY_CODE entity, string newValue)
        {
            MainViewModel.EntitiesUndoRedoManager.AddUndo(entity,
                BindableBase.GetPropertyName(() => new COMMODITY_CODE().CODE), entity.CODE, newValue,
                EntityMessageType.Changed);
            return newValue;
        }

        private string AddUndoOnFULLCODEChanges(COMMODITY_CODE entity)
        {
            var newValue = GenerateFullCode(entity);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(entity,
                BindableBase.GetPropertyName(() => new COMMODITY_CODE().FULLCODE), entity.FULLCODE, newValue,
                EntityMessageType.Changed);
            return newValue;
        }

        private string GenerateOrderString(decimal OrderNum)
        {
            OrderNum /= 10;
            if (OrderNum < 10)
                return "0" + OrderNum.ToString();
            else
                return OrderNum.ToString();
        }

        public string GenerateFullCode(COMMODITY_CODE startChildEntity)
        {
            var nameString = string.Empty;

            nameString = startChildEntity.CODE;
            var iterateEntity = startChildEntity;
            do
            {
                iterateEntity = MainViewModel.Entities.FirstOrDefault(x => x.GUID == iterateEntity.GUID_PARENT);
                if (iterateEntity != null)
                    nameString = iterateEntity.CODE + "." + nameString;
                else
                    break;
            } while (iterateEntity.GUID_PARENT != Guid.Empty);

            return nameString;
        }

        public static IEnumerable<COMMODITY_CODE> RecurseFindChildren(COMMODITY_CODE parentEntity,
            IEnumerable<COMMODITY_CODE> entities)
        {
            foreach (var entity in entities)
                if (entity.GUID_PARENT == parentEntity.GUID)
                {
                    yield return entity;

                    foreach (var entityChild in RecurseFindChildren(entity, entities))
                        yield return entityChild;
                }
        }

        #endregion

        #region View Commands

        public void AddCommodityCodeRowBefore()
        {
            AddCommodityCodeRow(false);
        }

        public void AddCommodityCodeRowAfter()
        {
            AddCommodityCodeRow(true);
        }

        private void AddCommodityCodeRow(bool isAfter)
        {
            if (DISCIPLINECollection.Count() == 0)
            {
                MessageBoxService.ShowMessage(BluePrintsResources.CommodityCode_NoDiscipline);
                return;
            }

            var commodityCodeOrder = 0;
            var guid_parent = Guid.Empty;
            if (DisplayEntities != null)
            {
                if (isAfter)
                    commodityCodeOrder = DisplaySelectedEntity.SORTORDER + 1;
                else
                    commodityCodeOrder = DisplaySelectedEntity.SORTORDER - 1;

                guid_parent = DisplaySelectedEntity.GUID_PARENT;
            }

            var newCommodityCode = new COMMODITY_CODE();
            newCommodityCode.CODE = "temp";
            newCommodityCode.FULLCODE = "temp";
            newCommodityCode.NAME = BluePrintsResources.CommodityCode_NewCommodity;
            newCommodityCode.GUID_DISCIPLINE = DisplaySelectedEntity == null
                ? DISCIPLINECollection.First().GUID
                : DisplaySelectedEntity.GUID_DISCIPLINE;
            newCommodityCode.GUID_DEPARTMENT = DisplaySelectedEntity == null
                ? DEPARTMENTCollection.First().GUID
                : DisplaySelectedEntity.GUID_DEPARTMENT;
            newCommodityCode.SORTORDER = commodityCodeOrder;
            newCommodityCode.GUID_PARENT = guid_parent;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Save will unpause this
            MainViewModel.EntitiesUndoRedoManager.AddUndo(newCommodityCode, null, null, null, EntityMessageType.Added);
            MainViewModel.Save(newCommodityCode);
            ReorderAndSave(guid_parent);
        }

        private void ReorderAndSave(IEnumerable<Guid> guid_parents)
        {
            var childEntities = new List<COMMODITY_CODE>();
            foreach (var guid_parent in guid_parents)
                childEntities = childEntities.Concat(ReorderAndSave(guid_parent, true)).ToList();

            MainViewModel.BulkSave(childEntities);
        }

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "COMMODITY_CODESViewModelWrapper"; }
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

        public IEnumerable<UOM> UOMCollection
        {
            get
            {
                var collection = GetEntities<UOM>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.UOM1);
                return collection;
            }
        }

        public IEnumerable<INDIRECT_TYPE> INDIRECT_TYPECollection
        {
            get
            {
                var collection = GetEntities<INDIRECT_TYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        #endregion
    }
}