using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using BluePrints.Common.Projections;
using BluePrints.Data.Helpers;
using System.Collections.Generic;
using DevExpress.Xpf.Grid.TreeList;
using BluePrints.Common.ViewModel.Utils;
using DevExpress.Mvvm;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Views;
using DevExpress.Xpf.Grid;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITIES collection view model.
    /// </summary>
    public partial class COMMODITYCollectionViewModelWrapper : CollectionViewModelsWrapper<COMMODITY, COMMODITYProjection, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<COMMODITY, COMMODITYProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of COMMODITYCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITYCollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITYCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITYCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITYCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITYCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operation
        PROJECT loadPROJECT;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            EntitiesParameter<BluePrints.Data.PROJECT> PROJECTParameter = (EntitiesParameter<BluePrints.Data.PROJECT>)parameter;
            this.loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, null, null, null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.UOMS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.COMMODITIES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY>, IQueryable<COMMODITYProjection>> ConstructMainViewModelProjection()
        {
            return query => COMMODITYProjectionQueries.ConvertToProjectionCOMMODITIES(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITYProjection> entities)
        {
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.CanBulkDeleteCallBack = this.CanBulkDeleteCallBack;
            MainViewModel.OnEntitySavedCallBack = this.OnEntitiesSavedCallBack;
            MainViewModel.EntitiesBeforeDeletionCallBack = this.EntitiesBeforeDeletionCallBack;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected override void OnEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender == MainViewModel)
                return;

            if (loadPROJECT != null && changedType == typeof(PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null )
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }
        }

        #region Collection Call Backs
        public void ApplyProjectionPropertiesToEntity(COMMODITYProjection projectionEntity, COMMODITY entity)
        {
            DataUtils.ShallowCopy(entity, projectionEntity.COMMODITY);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.COMMODITY.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.COMMODITY.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, COMMODITYProjection projectionEntity, COMMODITY entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.COMMODITY.GUID = entity.GUID;
        }

        public bool CanBulkDeleteCallBack(IEnumerable<COMMODITYProjection> entities)
        {
            if (entities == null || entities.Count() == 0)
                return false;
            if (entities.Any(x => x.GUID_PARENT != Guid.Empty))
                return false;

            return true;
        }

        //Remove children before parent deletion
        private void EntitiesBeforeDeletionCallBack(IEnumerable<COMMODITYProjection> entities)
        {
            //Only principal entities can be deleted
            IEnumerable<COMMODITYProjection> principalEntities = entities.Where(x => x.GUID_PARENT == Guid.Empty).AsEnumerable();

            //Undo manager is paused in bulk deletion and will be unpaused in bulk deletion too
            List<COMMODITYProjection> childrenEntities = new List<COMMODITYProjection>();
            foreach (var entity in principalEntities)
            {
                var totalChildrenCOMMODITY_CODE = RecurseFindChildren(entity, MainViewModel.Entities);
                List<COMMODITYProjection> childrenEntitiesNotInDeletionCollection = new List<COMMODITYProjection>();
                foreach (COMMODITYProjection childrenEntityInTotal in totalChildrenCOMMODITY_CODE)
                {
                    if (!entities.Any(x => x.GUID == childrenEntityInTotal.GUID))
                        childrenEntitiesNotInDeletionCollection.Add(childrenEntityInTotal);
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

        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            foreach (TreeListNode obj in e.DraggedRows)
            {
                COMMODITY_CODE droppedCOMMODITY_CODE = obj.Content as COMMODITY_CODE;
                if (droppedCOMMODITY_CODE == null)
                    continue;

                COMMODITY newCOMMODITY = new COMMODITY();
                newCOMMODITY.GUID_COMMODITYCODE = droppedCOMMODITY_CODE.GUID;
                newCOMMODITY.COMMODITY_CODE = droppedCOMMODITY_CODE;
                AddCommodity(newCOMMODITY);
            }

            e.Handled = true;
        }

        public void TreelistExistingRowAddUndoAndSave(TreeListCellValueChangedEventArgs e)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            COMMODITYProjection editedCOMMODITY = (COMMODITYProjection)e.Row;
            MainViewModel.EntitiesUndoRedoManager.AddUndo(editedCOMMODITY, e.Column.FieldName, e.OldValue, e.Value, EntityMessageType.Changed);
            List<COMMODITYProjection> editParentCOMMODITY = new List<COMMODITYProjection>();
            RecurseSumParentEditValue(editedCOMMODITY, e.Column.FieldName, editParentCOMMODITY);
            editParentCOMMODITY.Add(editedCOMMODITY);
            MainViewModel.BulkSave(editParentCOMMODITY);
        }

        private void RecurseSumParentEditValue(COMMODITYProjection childCOMMODITY, string fieldName, List<COMMODITYProjection> editedCOMMODITY)
        {
            if (childCOMMODITY.COMMODITY.GUID_PARENT == Guid.Empty)
                return;
            else
            {
                COMMODITYProjection parentCOMMODITY = MainViewModel.Entities.FirstOrDefault(x => x.GUID == childCOMMODITY.GUID_PARENT);
                if (parentCOMMODITY != null)
                {
                    decimal newValue = -1;
                    decimal? oldValue = null;
                    if (fieldName == BindableBase.GetPropertyName(() => new COMMODITYProjection().COMMODITY) + "." + BindableBase.GetPropertyName(() => new COMMODITY().RATE_FREIGHT))
                    {
                        newValue = MainViewModel.Entities.Where(x => x.GUID_PARENT == parentCOMMODITY.GUID && x.COMMODITY != null && x.COMMODITY.RATE_FREIGHT != null).Sum(x => (decimal)x.COMMODITY.RATE_FREIGHT);
                        oldValue = parentCOMMODITY.COMMODITY.RATE_FREIGHT;
                        parentCOMMODITY.COMMODITY.RATE_FREIGHT = newValue;
                    }
                    else if (fieldName == BindableBase.GetPropertyName(() => new COMMODITYProjection().COMMODITY) + "." + BindableBase.GetPropertyName(() => new COMMODITY().RATE_SUPPLY))
                    {
                        newValue = MainViewModel.Entities.Where(x => x.GUID_PARENT == parentCOMMODITY.GUID && x.COMMODITY != null && x.COMMODITY.RATE_SUPPLY != null).Sum(x => (decimal)x.COMMODITY.RATE_SUPPLY);
                        oldValue = parentCOMMODITY.COMMODITY.RATE_SUPPLY;
                        parentCOMMODITY.COMMODITY.RATE_SUPPLY = newValue;
                    }
                    else if (fieldName == BindableBase.GetPropertyName(() => new COMMODITYProjection().COMMODITY) + "." + BindableBase.GetPropertyName(() => new COMMODITY().HOURS_INSTALL))
                    {
                        newValue = MainViewModel.Entities.Where(x => x.GUID_PARENT == parentCOMMODITY.GUID && x.COMMODITY != null && x.COMMODITY.HOURS_INSTALL != null).Sum(x => (decimal)x.COMMODITY.HOURS_INSTALL);
                        oldValue = parentCOMMODITY.COMMODITY.HOURS_INSTALL;
                        parentCOMMODITY.COMMODITY.HOURS_INSTALL = newValue;
                    }

                    if(newValue != -1)
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY, fieldName, oldValue, newValue, EntityMessageType.Changed);
                        editedCOMMODITY.Add(parentCOMMODITY);
                    }

                    RecurseSumParentEditValue(parentCOMMODITY, fieldName, editedCOMMODITY);
                }
                else
                    return;
            }
        }

        private decimal SumSameLevelEditValue(Guid parentKey, string fieldName)
        {
            IEnumerable<COMMODITYProjection> sameLevelCOMMODITYProjection = MainViewModel.Entities.Where(x => x.GUID_PARENT == parentKey);
            decimal sumValue = 0;
            if(fieldName == BindableBase.GetPropertyName(() => new COMMODITYProjection().COMMODITY) + "." + BindableBase.GetPropertyName(() => new COMMODITY().HOURS_INSTALL))
                sumValue = sameLevelCOMMODITYProjection.Where(x => x.COMMODITY != null && x.COMMODITY.HOURS_INSTALL != null).Sum(x => (decimal)x.COMMODITY.HOURS_INSTALL);
            else if(fieldName == BindableBase.GetPropertyName(() => new COMMODITYProjection().COMMODITY) + "." + BindableBase.GetPropertyName(() => new COMMODITY().RATE_FREIGHT))
                sumValue = sameLevelCOMMODITYProjection.Where(x => x.COMMODITY != null && x.COMMODITY.RATE_FREIGHT != null).Sum(x => (decimal)x.COMMODITY.RATE_FREIGHT);
            else if(fieldName == BindableBase.GetPropertyName(() => new COMMODITYProjection().COMMODITY) + "." + BindableBase.GetPropertyName(() => new COMMODITY().RATE_SUPPLY))
                sumValue = sameLevelCOMMODITYProjection.Where(x => x.COMMODITY != null && x.COMMODITY.RATE_SUPPLY != null).Sum(x => (decimal)x.COMMODITY.RATE_SUPPLY);

            return sumValue;
        }

        public static IEnumerable<COMMODITYProjection> RecurseFindChildren(COMMODITYProjection parentEntity, IEnumerable<COMMODITYProjection> entities)
        {
            foreach (COMMODITYProjection entity in entities)
            {
                if (entity.GUID_PARENT == parentEntity.GUID)
                {
                    yield return entity;

                    foreach (COMMODITYProjection entityChild in RecurseFindChildren(entity, entities))
                        yield return entityChild;
                }
            }
        }
        #endregion
        #endregion

        #region View Properties
        DevExpress.Mvvm.IDialogService AddCOMMODITYDialogService { get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("AddCOMMODITYService"); } }


        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "COMMODITYCollectionViewModelWrapper";
            }
        }
        #endregion

        #region View Commands
        public void AddCommodityFromDialog()
        {
            AddCOMMODITYViewModel addCOMMODITYViewModel = AddCOMMODITYViewModel.Create(loaderCollection.GetCollection<COMMODITY_CODE>());
            if (AddCOMMODITYDialogService.ShowDialog(MessageButton.OKCancel, "Add Commodity", "AddCOMMODITYView", addCOMMODITYViewModel) == MessageResult.OK)
            {
                AddCommodity(addCOMMODITYViewModel.newCOMMODITY);
            }
        }

        private void AddCommodity(COMMODITY newCOMMODITY)
        {
            List<COMMODITYProjection> addedEntitiesLookUp = new List<COMMODITYProjection>();
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            COMMODITYProjection addCOMMODITY = new COMMODITYProjection();
            addCOMMODITY.COMMODITY = newCOMMODITY;
            addCOMMODITY.COMMODITY.GUID_PROJECT = loadPROJECT.GUID;
            addCOMMODITY.COMMODITY.GUID_PARENT = Guid.Empty;
            MainViewModel.EntitiesUndoRedoManager.AddUndo(addCOMMODITY, null, null, null, EntityMessageType.Added);
            MainViewModel.Save(addCOMMODITY);
            addedEntitiesLookUp.Add(addCOMMODITY);

            COMMODITY_CODE parentCOMMODITY_CODE = loaderCollection.GetCollection<COMMODITY_CODE>().FirstOrDefault(x => x.GUID == addCOMMODITY.COMMODITY.GUID_COMMODITYCODE);
            if (parentCOMMODITY_CODE != null)
            {
                IEnumerable<COMMODITY_CODE> childrenCOMMODITY_CODES = COMMODITY_CODESViewModelWrapper.RecurseFindChildren(parentCOMMODITY_CODE, loaderCollection.GetCollection<COMMODITY_CODE>());
                foreach (COMMODITY_CODE childrenCOMMODITY_CODE in childrenCOMMODITY_CODES)
                {
                    COMMODITYProjection childrenCOMMODITY = new COMMODITYProjection();
                    childrenCOMMODITY.COMMODITY.GUID_PROJECT = loadPROJECT.GUID;
                    COMMODITYProjection parentCOMMODITYProjection = addedEntitiesLookUp.First(x => x.COMMODITY.GUID_COMMODITYCODE == childrenCOMMODITY_CODE.GUID_PARENT);
                    childrenCOMMODITY.COMMODITY.GUID_PARENT = parentCOMMODITYProjection.GUID;
                    childrenCOMMODITY.COMMODITY.GUID_COMMODITYCODE = childrenCOMMODITY_CODE.GUID;
                    childrenCOMMODITY.COMMODITY.COMMODITY_CODE = childrenCOMMODITY_CODE;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(childrenCOMMODITY, null, null, null, EntityMessageType.Added);
                    MainViewModel.Save(childrenCOMMODITY);
                    addedEntitiesLookUp.Add(childrenCOMMODITY);
                }
            }

            foreach (COMMODITYProjection addedEntity in addedEntitiesLookUp)
            {
                if (addedEntitiesLookUp.Any(x => x.GUID_PARENT == addedEntity.GUID))
                {
                    addedEntity.ISREADONLY = true;
                    addedEntity.ISEXPANDED = true;
                }
            }

            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }
        #endregion

        #region View Properties
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
        #endregion

        #region View Behavior

        #endregion
    }
}