using System;
using System.Linq;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Grid;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using System.Collections.Generic;
using BluePrints.Data.Helpers;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Grid.TreeList;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITY_CODES collection view model.
    /// </summary>
    public partial class COMMODITY_CODESViewModelWrapper : CollectionViewModelsWrapper<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODESCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODESViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODESViewModelWrapper(unitOfWorkFactory));
        }

        DispatcherTimer delayedCOMMODITY_CODEPopulateDispatcher;
        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODESCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODESCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODESViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        CommodityCodeType loadCommodityCodeType;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            CommodityCodeType receiveParameter = (CommodityCodeType)parameter;
            this.loadCommodityCodeType = receiveParameter;
            delayedCOMMODITY_CODEPopulateDispatcher = new DispatcherTimer();
            delayedCOMMODITY_CODEPopulateDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedCOMMODITY_CODEPopulateDispatcher.Tick += delayedCOMMODITY_CODEPopulateDispatcher_Tick;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, null, null, isContinueLoadingAfterDEPARTMENT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES, null, null, isContinueLoadingAfterDISCIPLINE, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<INDIRECT_TYPE, INDIRECT_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.INDIRECT_TYPES, null, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.UOMS, null, null, null, OnAfterEntitiesChanged);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterDEPARTMENT(IEnumerable<DEPARTMENT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDepartment)));
                return false;
            }

            return true;
        }

        bool isContinueLoadingAfterDISCIPLINE(IEnumerable<DISCIPLINE> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDiscipline)));
                return false;
            }

            return true;
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.COMMODITYCODETYPE == loadCommodityCodeType);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_CODE> entities)
        {
            MainViewModel.EntitiesAfterDeletionCallBack = this.EntitiesAfterDeletion;
            MainViewModel.EntitiesBeforeDeletionCallBack = this.EntitiesBeforeDeletion;
            MainViewModel.OnBeforeEntitySavedCallBack = this.OnBeforeEntitiesSaved;
            MainViewModel.treeListExistingRowAddUndoAndSavePostCallBack = this.TreelistExistingRowAddUndoAndSave;
            MainViewModel.SetParentViewModel(this);
            if (loadCommodityCodeType == CommodityCodeType.Design)
                mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowDEPARTMENT()));
            else if (loadCommodityCodeType == CommodityCodeType.Direct)
                mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowDISCIPLINE()));
            else
                mainThreadDispatcher.BeginInvoke(new Action(() => this.ShowINDIRECT_TYPE()));

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            mainThreadDispatcher.BeginInvoke(new Action(() => delayedCOMMODITY_CODEPopulateDispatcher.Start()));
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender.ToString() == MainViewModel.ToString())
                return;

            if (MainViewModel != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
            else
                mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
        }

        void delayedCOMMODITY_CODEPopulateDispatcher_Tick(object sender, EventArgs e)
        {
            delayedCOMMODITY_CODEPopulateDispatcher.Stop();

            if(loadCommodityCodeType == CommodityCodeType.Design)
            {
                IEnumerable<DEPARTMENT> populateDEPARTMENTS = DEPARTMENTCollection.Where(x => x.ISDESIGNCOMMODITY == true);
                foreach (DEPARTMENT populateDEPARTMENT in populateDEPARTMENTS)
                {
                    if (!MainViewModel.Entities.Any(x => x.GUID_PARENT == Guid.Empty && x.GUID_DEPARTMENT == populateDEPARTMENT.GUID))
                    {
                        COMMODITY_CODE newCOMMODITY_CODE = new COMMODITY_CODE();
                        newCOMMODITY_CODE.CODE = populateDEPARTMENT.CODE;
                        newCOMMODITY_CODE.FULLCODE = populateDEPARTMENT.CODE;
                        newCOMMODITY_CODE.NAME = populateDEPARTMENT.NAME;
                        newCOMMODITY_CODE.GUID_DEPARTMENT = populateDEPARTMENT.GUID;
                        MainViewModel.Save(newCOMMODITY_CODE);
                    }
                }
            }
            else if(loadCommodityCodeType == CommodityCodeType.Direct)
            {
                foreach(DISCIPLINE populateDISCIPLINE in DISCIPLINECollection)
                {
                    if (!MainViewModel.Entities.Any(x => x.GUID_PARENT == Guid.Empty && x.GUID_DISCIPLINE == populateDISCIPLINE.GUID))
                    {
                        COMMODITY_CODE newCOMMODITY_CODE = new COMMODITY_CODE();
                        newCOMMODITY_CODE.CODE = populateDISCIPLINE.CODE;
                        newCOMMODITY_CODE.FULLCODE = populateDISCIPLINE.CODE;
                        newCOMMODITY_CODE.NAME = populateDISCIPLINE.NAME;
                        newCOMMODITY_CODE.GUID_DISCIPLINE = populateDISCIPLINE.GUID;
                        MainViewModel.Save(newCOMMODITY_CODE);
                    }
                }
            }
            else if(loadCommodityCodeType == CommodityCodeType.Indirect)
            {
                foreach(INDIRECT_TYPE populateINDIRECT_TYPE in INDIRECT_TYPECollection)
                {
                    if (!MainViewModel.Entities.Any(x => x.GUID_PARENT == Guid.Empty && x.GUID_INDIRECTTYPE == populateINDIRECT_TYPE.GUID))
                    {
                        COMMODITY_CODE newCOMMODITY_CODE = new COMMODITY_CODE();
                        newCOMMODITY_CODE.CODE = populateINDIRECT_TYPE.CODE;
                        newCOMMODITY_CODE.FULLCODE = populateINDIRECT_TYPE.CODE;
                        newCOMMODITY_CODE.NAME = populateINDIRECT_TYPE.NAME;
                        newCOMMODITY_CODE.GUID_INDIRECTTYPE = populateINDIRECT_TYPE.GUID;
                        MainViewModel.Save(newCOMMODITY_CODE);
                    }
                }
            }
        }

        #region Collection Call Backs
        public void TreelistExistingRowAddUndoAndSave(TreeListCellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new COMMODITY_CODE().CODE))
            {
                MainViewModel.EntitiesUndoRedoManager.RewindActionId(1);
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                COMMODITY_CODE editedCOMMODITY_CODE = (COMMODITY_CODE)e.Row;
                AddUndoOnFULLCODEChanges(editedCOMMODITY_CODE);
                RecurseRenameChildrenFULLCODE(editedCOMMODITY_CODE.GUID);
                IEnumerable<COMMODITY_CODE> childrenCOMMODITY_CODES = RecurseFindChildren(editedCOMMODITY_CODE, MainViewModel.Entities);
                MainViewModel.BulkSave(childrenCOMMODITY_CODES);
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
            List<COMMODITY_CODE> childrenEntities = new List<COMMODITY_CODE>();
            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = RecurseFindChildren(entity, MainViewModel.Entities);
                List<COMMODITY_CODE> childrenEntitiesNotInDeletionCollection = new List<COMMODITY_CODE>();
                foreach (var childrenEntityInTotal in childrenEntitiesInTotal)
                {
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);
                }

                childrenEntities = childrenEntities.Concat(childrenEntitiesNotInDeletionCollection).ToList();
            }

            uniqueGUID_PARENTS = new List<Guid>();
            //can't use bulk delete here due to stack overflow
            foreach (var childrenEntity in childrenEntities)
            {
                if (!uniqueGUID_PARENTS.Any(x => x == childrenEntity.GUID_PARENT))
                    uniqueGUID_PARENTS.Add(childrenEntity.GUID_PARENT);

                MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenEntity, null, null, null, EntityMessageType.Deleted);
                MainViewModel.Delete(childrenEntity);
            }
        }

        //Reorder tree after deletion
        private void EntitiesAfterDeletion(IEnumerable<COMMODITY_CODE> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            //uniqueGUID_PARENTS is initialized in EntitiesBeforeDeletion
            foreach (var entity in entities)
            {
                if (!uniqueGUID_PARENTS.Any(x => x == entity.GUID_PARENT))
                    uniqueGUID_PARENTS.Add(entity.GUID_PARENT);
            }

            MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
            ReorderAndSave(uniqueGUID_PARENTS);
        }
        #endregion
        #endregion

        #region View Behavior
        public Action ShowDISCIPLINE;
        public Action ShowDEPARTMENT;
        public Action ShowINDIRECT_TYPE;

        List<Guid> uniqueGUID_PARENTS; //stores dropping entity parent guid before it gets reassigned
        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            uniqueGUID_PARENTS = new List<Guid>();
            if (e.TargetNode != null)
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
                foreach (object obj in e.DraggedRows)
                {
                    COMMODITY_CODE editCommodityCode = (e.SourceManager.GetObject(obj) as COMMODITY_CODE);

                    if (!uniqueGUID_PARENTS.Any(x => x == editCommodityCode.GUID_PARENT))
                        uniqueGUID_PARENTS.Add(editCommodityCode.GUID_PARENT);

                    COMMODITY_CODE targetCommodityCode = (e.TargetNode.Content as COMMODITY_CODE);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(editCommodityCode, BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_PARENT), editCommodityCode.GUID_PARENT, targetCommodityCode.GUID, EntityMessageType.Changed);
                }
            }
        }

        public void dragDropManager_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDroppedEventArgs e)
        {
            Guid newParentGuid = Guid.Empty;
            if (e.TargetNode != null)
            {
                foreach (TreeListNode obj in e.DraggedRows)
                {
                    COMMODITY_CODE droppedCOMMODITY_CODE = obj.Content as COMMODITY_CODE;
                    COMMODITY_CODE targetCommodityCode = (e.TargetNode.Content as COMMODITY_CODE);

                    if (e.DropTargetType == DropTargetType.InsertRowsAfter)
                        droppedCOMMODITY_CODE.SORTORDER = targetCommodityCode.SORTORDER + 1;
                    else if (e.DropTargetType == DropTargetType.InsertRowsBefore)
                        droppedCOMMODITY_CODE.SORTORDER = targetCommodityCode.SORTORDER - 1;
                    else
                    {
                        IEnumerable<COMMODITY_CODE> targetCommodityCodeChild = MainViewModel.Entities.Where(x => x.GUID_PARENT == targetCommodityCode.GUID);
                        
                        int maxTargetChildrenOrder = 0;
                        if (targetCommodityCodeChild.Count() > 0)
                            maxTargetChildrenOrder = targetCommodityCodeChild.Max(x => x.SORTORDER);

                        maxTargetChildrenOrder += 1;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedCOMMODITY_CODE, BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_DISCIPLINE), droppedCOMMODITY_CODE.GUID_DISCIPLINE, targetCommodityCode.GUID_DISCIPLINE, EntityMessageType.Changed);
                        droppedCOMMODITY_CODE.GUID_DISCIPLINE = targetCommodityCode.GUID_DISCIPLINE;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedCOMMODITY_CODE, BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_DEPARTMENT), droppedCOMMODITY_CODE.GUID_DEPARTMENT, targetCommodityCode.GUID_DEPARTMENT, EntityMessageType.Changed);
                        droppedCOMMODITY_CODE.GUID_DEPARTMENT = targetCommodityCode.GUID_DEPARTMENT;

                        droppedCOMMODITY_CODE.SORTORDER = maxTargetChildrenOrder;
                    }

                    newParentGuid = droppedCOMMODITY_CODE.GUID_PARENT;
                }

                uniqueGUID_PARENTS.Add(newParentGuid);
                ReorderAndSave(uniqueGUID_PARENTS);
            }
        }

        private IEnumerable<COMMODITY_CODE> ReorderAndSave(Guid guid_parent, bool dontSave = false)
        {
            IEnumerable<COMMODITY_CODE> childCommodityCodes = MainViewModel.Entities.Where(x => x.GUID_PARENT == guid_parent).OrderBy(x => x.SORTORDER).ToList();
            int commodityCodeOrderCount = 10;
            foreach (COMMODITY_CODE childCommodityCode in childCommodityCodes)
            {
                if (childCommodityCode.SORTORDER != commodityCodeOrderCount)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childCommodityCode, BindableBase.GetPropertyName(() => new COMMODITY_CODE().SORTORDER), childCommodityCode.SORTORDER, commodityCodeOrderCount, EntityMessageType.Changed);
                    childCommodityCode.SORTORDER = commodityCodeOrderCount;
                    int tryParseInt;
                    if (childCommodityCode.CODE == "temp")
                        childCommodityCode.CODE = GenerateOrderString(childCommodityCode.SORTORDER);
                    else
                        childCommodityCode.CODE = Int32.TryParse(childCommodityCode.CODE, out tryParseInt) ? AddUndoOnCODEChanges(childCommodityCode, GenerateOrderString(childCommodityCode.SORTORDER)) : childCommodityCode.CODE;
                }

                commodityCodeOrderCount += 10;
            }

            RecurseRenameChildrenFULLCODE(guid_parent);

            if (!dontSave)
                MainViewModel.BulkSave(childCommodityCodes);

            return childCommodityCodes;
        }

        private void RecurseRenameChildrenFULLCODE(Guid guid_parent)
        {
            IEnumerable<COMMODITY_CODE> childCommodityCodes = MainViewModel.Entities.Where(x => x.GUID_PARENT == guid_parent).OrderBy(x => x.SORTORDER).ToList();
            foreach (COMMODITY_CODE childCommodityCode in childCommodityCodes)
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
            MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new COMMODITY_CODE().CODE), entity.CODE, newValue, EntityMessageType.Changed);
            return newValue;
        }

        private string AddUndoOnFULLCODEChanges(COMMODITY_CODE entity)
        {
            string newValue = GenerateFullCode(entity);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new COMMODITY_CODE().FULLCODE), entity.FULLCODE, newValue, EntityMessageType.Changed);
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
            string nameString = string.Empty;

            nameString = startChildEntity.CODE;
            COMMODITY_CODE iterateEntity = startChildEntity;
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

        public static IEnumerable<COMMODITY_CODE> RecurseFindChildren(COMMODITY_CODE parentEntity, IEnumerable<COMMODITY_CODE> entities)
        {
            foreach (COMMODITY_CODE entity in entities)
            {
                if (entity.GUID_PARENT == parentEntity.GUID)
                {
                    yield return entity;

                    foreach (COMMODITY_CODE entityChild in RecurseFindChildren(entity, entities))
                        yield return entityChild;
                }
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
                MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDiscipline);
                return;
            }

            int commodityCodeOrder = 0;
            Guid guid_parent = Guid.Empty;
            if (MainViewModel.SelectedEntity != null)
            {
                if (isAfter)
                    commodityCodeOrder = MainViewModel.SelectedEntity.SORTORDER + 1;
                else
                    commodityCodeOrder = MainViewModel.SelectedEntity.SORTORDER - 1;

                guid_parent = MainViewModel.SelectedEntity.GUID_PARENT;
            }

            COMMODITY_CODE newCommodityCode = new COMMODITY_CODE();
            newCommodityCode.CODE = "temp";
            newCommodityCode.FULLCODE = "temp";
            newCommodityCode.NAME = CommonResources.CommodityCode_NewCommodity;
            newCommodityCode.GUID_DISCIPLINE = MainViewModel.SelectedEntity == null ? DISCIPLINECollection.First().GUID : MainViewModel.SelectedEntity.GUID_DISCIPLINE;
            newCommodityCode.GUID_DEPARTMENT = MainViewModel.SelectedEntity == null ? DEPARTMENTCollection.First().GUID : MainViewModel.SelectedEntity.GUID_DEPARTMENT;
            newCommodityCode.SORTORDER = commodityCodeOrder;
            newCommodityCode.GUID_PARENT = guid_parent;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Save will unpause this
            MainViewModel.EntitiesUndoRedoManager.AddUndo(newCommodityCode, null, null, null, EntityMessageType.Added);
            MainViewModel.Save(newCommodityCode);
            ReorderAndSave(guid_parent);
        }

        private void ReorderAndSave(IEnumerable<Guid> guid_parents)
        {
            List<COMMODITY_CODE> childEntities = new List<COMMODITY_CODE>();
            foreach (Guid guid_parent in guid_parents)
            {
                childEntities = childEntities.Concat(ReorderAndSave(guid_parent, true)).ToList();
            }

            MainViewModel.BulkSave(childEntities);
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
                return "COMMODITY_CODESViewModelWrapper";
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