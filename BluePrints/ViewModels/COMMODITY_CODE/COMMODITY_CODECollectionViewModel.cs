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

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITY_CODES collection view model.
    /// </summary>
    public partial class COMMODITY_CODESCollectionViewModel : CollectionViewModel<COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of COMMODITY_CODESCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODESCollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODESCollectionViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODESCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODESCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODESCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.COMMODITY_CODES, query => query.OrderBy(x => x.SORTORDER))
        {
            EntitiesAfterDeletionCallBack = this.EntitiesAfterDeletion;
            EntitiesBeforeDeletionCallBack = this.EntitiesBeforeDeletion;
        }

        /// <summary>
        /// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((COMMODITY_CODESCollectionViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }

        /// <summary>
        /// The view model that contains a look-up collection of UOM for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<UOM> LookUpUOMS
        {
            get { return GetLookUpEntitiesViewModel((COMMODITY_CODESCollectionViewModel x) => x.LookUpUOMS, x => x.UOMS); }
        }

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
            if (LookUpDISCIPLINES.Entities.Count() == 0)
            {
                MessageBoxService.ShowMessage(CommonResources.CommodityCode_NoDiscipline);
                return;
            }

            int commodityCodeOrder = 0;
            Guid guid_parent = Guid.Empty;
            if(SelectedEntity != null)
            {
                if (isAfter)
                    commodityCodeOrder = SelectedEntity.SORTORDER + 1;
                else
                    commodityCodeOrder = SelectedEntity.SORTORDER - 1;

                guid_parent = SelectedEntity.GUID_PARENT;
            }

            COMMODITY_CODE newCommodityCode = this.CreateEntity();
            newCommodityCode.CODE = "temp";
            newCommodityCode.FULLCODE = "temp";
            newCommodityCode.NAME = "new commodity";
            newCommodityCode.GUID_DISCIPLINE = LookUpDISCIPLINES.Entities.First().GUID;
            newCommodityCode.SORTORDER = commodityCodeOrder;
            newCommodityCode.GUID_PARENT = guid_parent;

            EntitiesUndoRedoManager.PauseActionId(); //Save will unpause this
            EntitiesUndoRedoManager.AddUndo(newCommodityCode, null, null, null, EntityMessageType.Added);
            this.Save(newCommodityCode);
            ReorderAndSave(guid_parent);
        }

        private void ReorderAndSave(IEnumerable<Guid> guid_parents)
        {
            List<COMMODITY_CODE> childEntities = new List<COMMODITY_CODE>();
            foreach (Guid guid_parent in guid_parents)
            {
                childEntities = childEntities.Concat(ReorderAndSave(guid_parent, true)).ToList();
            }

            BulkSave(childEntities);
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletion(IEnumerable<COMMODITY_CODE> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            List<COMMODITY_CODE> childrenEntities = new List<COMMODITY_CODE>();
            foreach (var entity in entities)
            {
                var childrenEntitiesInTotal = RecurseFindChildren(entity, this.Entities);
                List<COMMODITY_CODE> childrenEntitiesNotInDeletionCollection = new List<COMMODITY_CODE>();
                foreach(var childrenEntityInTotal in childrenEntitiesInTotal)
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

                EntitiesUndoRedoManager.AddUndo(childrenEntity, null, null, null, EntityMessageType.Deleted);
                Delete(childrenEntity);
            }
        }

        public static IEnumerable<COMMODITY_CODE> RecurseFindChildren(COMMODITY_CODE parentEntity, IEnumerable<COMMODITY_CODE> entities)
        {
            foreach (COMMODITY_CODE entity in entities)
            {
                if(entity.GUID_PARENT == parentEntity.GUID)
                {
                    yield return entity;

                    foreach (COMMODITY_CODE entityChild in RecurseFindChildren(entity, entities))
                        yield return entityChild;
                }
            }
        }

        //Reorder tree after deletion
        private void EntitiesAfterDeletion(IEnumerable<COMMODITY_CODE> entities)
        {
            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            //uniqueGUID_PARENTS is initialized in EntitiesBeforeDeletion
            foreach(var entity in entities)
            {
                if (!uniqueGUID_PARENTS.Any(x => x == entity.GUID_PARENT))
                    uniqueGUID_PARENTS.Add(entity.GUID_PARENT);
            }

            EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
            ReorderAndSave(uniqueGUID_PARENTS);
        }

        List<Guid> uniqueGUID_PARENTS; //stores dropping entity parent guid before it gets reassigned
        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            uniqueGUID_PARENTS = new List<Guid>();
            if (e.TargetNode != null)
            {
                EntitiesUndoRedoManager.PauseActionId(); //save will unpause this
                foreach (object obj in e.DraggedRows)
                {
                    COMMODITY_CODE editCommodityCode = (e.SourceManager.GetObject(obj) as COMMODITY_CODE);

                    if (!uniqueGUID_PARENTS.Any(x => x == editCommodityCode.GUID_PARENT))
                        uniqueGUID_PARENTS.Add(editCommodityCode.GUID_PARENT);

                    COMMODITY_CODE targetCommodityCode = (e.TargetNode.Content as COMMODITY_CODE);
                    EntitiesUndoRedoManager.AddUndo(editCommodityCode, BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_PARENT), editCommodityCode.GUID_PARENT, targetCommodityCode.GUID, EntityMessageType.Changed);
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
                        IEnumerable<COMMODITY_CODE> targetCommodityCodeChild = this.Entities.Where(x => x.GUID_PARENT == targetCommodityCode.GUID);
                        int maxTargetChildrenOrder = 0;
                        if (targetCommodityCodeChild.Count() > 0)
                            maxTargetChildrenOrder = targetCommodityCodeChild.Max(x => x.SORTORDER);

                        maxTargetChildrenOrder += 1;
                        droppedCOMMODITY_CODE.SORTORDER = maxTargetChildrenOrder;
                    }

                    newParentGuid = droppedCOMMODITY_CODE.GUID_PARENT;
                }

                uniqueGUID_PARENTS.Add(newParentGuid);
                ReorderAndSave(uniqueGUID_PARENTS);
            }
        }

        public override void TreelistExistingRowAddUndoAndSave(TreeListCellValueChangedEventArgs e)
        {
            base.TreelistExistingRowAddUndoAndSave(e);
            if(e.Column.FieldName == BindableBase.GetPropertyName(() => new COMMODITY_CODE().CODE))
            {
                EntitiesUndoRedoManager.RewindActionId(1);
                EntitiesUndoRedoManager.PauseActionId();
                COMMODITY_CODE editedCOMMODITY_CODE = (COMMODITY_CODE)e.Row;
                AddUndoOnFULLCODEChanges(editedCOMMODITY_CODE);
                RecurseRenameChildrenFULLCODE(editedCOMMODITY_CODE.GUID);
                IEnumerable<COMMODITY_CODE> childrenCOMMODITY_CODES = RecurseFindChildren(editedCOMMODITY_CODE, this.Entities);
                BulkSave(childrenCOMMODITY_CODES);
            }
        }

        private IEnumerable<COMMODITY_CODE> ReorderAndSave(Guid guid_parent, bool dontSave = false)
        {
            IEnumerable<COMMODITY_CODE> childCommodityCodes = this.Entities.Where(x => x.GUID_PARENT == guid_parent).OrderBy(x => x.SORTORDER).ToList();
            int commodityCodeOrderCount = 10;
            foreach (COMMODITY_CODE childCommodityCode in childCommodityCodes)
            {
                if (childCommodityCode.SORTORDER != commodityCodeOrderCount)
                {
                    EntitiesUndoRedoManager.AddUndo(childCommodityCode, BindableBase.GetPropertyName(() => new COMMODITY_CODE().SORTORDER), childCommodityCode.SORTORDER, commodityCodeOrderCount, EntityMessageType.Changed);
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
                this.BulkSave(childCommodityCodes);

            return childCommodityCodes;
        }

        private void RecurseRenameChildrenFULLCODE(Guid guid_parent)
        {
            IEnumerable<COMMODITY_CODE> childCommodityCodes = this.Entities.Where(x => x.GUID_PARENT == guid_parent).OrderBy(x => x.SORTORDER).ToList();
            foreach (COMMODITY_CODE childCommodityCode in childCommodityCodes)
            {
                if(childCommodityCode.CODE == "temp")
                    childCommodityCode.FULLCODE = GenerateFullCode(childCommodityCode);
                else
                    childCommodityCode.FULLCODE = AddUndoOnFULLCODEChanges(childCommodityCode);

                RecurseRenameChildrenFULLCODE(childCommodityCode.GUID);
            }
        }

        private string AddUndoOnCODEChanges(COMMODITY_CODE entity, string newValue)
        {
            EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new COMMODITY_CODE().CODE), entity.CODE, newValue, EntityMessageType.Changed);
            return newValue;
        }

        private string AddUndoOnFULLCODEChanges(COMMODITY_CODE entity)
        {
            string newValue = GenerateFullCode(entity);
            EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new COMMODITY_CODE().FULLCODE), entity.FULLCODE, newValue, EntityMessageType.Changed);
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
                iterateEntity = this.Entities.FirstOrDefault(x => x.GUID == iterateEntity.GUID_PARENT);
                if (iterateEntity != null)
                    nameString = iterateEntity.CODE + nameString;
                else
                    break;
            } while (iterateEntity.GUID_PARENT != Guid.Empty);

            return nameString;
        }
    }
}