using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the COMMODITIES collection view model.
    /// </summary>
    public partial class COMMODITY_GROUP_DIRECTCollectionViewModelWrapper :
        BluePrintsEntitiesMasterDetailCollectionsWrapper
        <COMMODITY_GROUP_DIRECT, COMMODITY_GROUP_DIRECTProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_GROUP_DIRECTCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_GROUP_DIRECTCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_GROUP_DIRECTCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITY_GROUP_DIRECTCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_GROUP_DIRECTCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_GROUP_DIRECTCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operation
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private List<Guid> RestoreExpandedGuids = new List<Guid>();
        protected override void InitializeParameters(object parameter)
        {
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.COMMODITYCODETYPE == CommodityCodeType.Direct && x.GUID_PROJECT == null);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_GROUP_DIRECT);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<COMMODITY_GROUP_DIRECT>, IQueryable<COMMODITY_GROUP_DIRECTProjection>>
            ConstructMainViewModelProjection()
        {
            return query => COMMODITY_GROUP_DIRECTProjectionQueries.ConvertToProjectionCOMMODITY_GROUP_DIRECT(query);
        }

        #region View Refresh
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_GROUP_DIRECTProjection> entities)
        {
            MainViewModel.AdditionalValidateCellCallBack = AdditionalCellValidation;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Collection Call Backs
        private void AdditionalCellValidation(GridCellValidationEventArgs e)
        {
            if (e.Column.FieldName == 
                BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECTProjection().Entity) 
                + "." +
                BindableBase.GetPropertyName(() => new COMMODITY_GROUP_DIRECT().GUID_COMMODITYCODE))
            {
                var editingCOMMODITY_GROUP = (COMMODITY_GROUP_DIRECTProjection)e.Row;
                if (editingCOMMODITY_GROUP.DetailEntities != null &&
                    editingCOMMODITY_GROUP.DetailEntities.Count > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = BluePrintsResources.CommodityGroup_CannotAssignCommodity;
                }

                //Avoid user from selecting WBS COMMODITY_CODE
                var newCOMMODITY_CODE = COMMODITY_CODECollection.First(x => x.GUID == (Guid) e.Value);
                if (!newCOMMODITY_CODE.ISQUANTIFIABLE)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = BluePrintsResources.CommodityGroup_CannotSelectParent;
                }
            }
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "COMMODITY_GROUP_DIRECTCollectionViewModelWrapper"; }
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

        #endregion

        #region View Behavior
        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            foreach (var obj in e.DraggedRows)
            {
                var droppedNode = obj as TreeListNode;
                if (droppedNode == null)
                    continue;

                var droppedCOMMODITY_CODE = droppedNode.Content as COMMODITY_CODE;
                if (droppedCOMMODITY_CODE == null || droppedCOMMODITY_CODE.COMMODITYCODETYPE != CommodityCodeType.Direct)
                    continue;

                var targetCOMMODITY_GROUP = e.TargetRow as COMMODITY_GROUP_DIRECTProjection;
                var newCOMMODITY_GROUP_DIRECT = new COMMODITY_GROUP_DIRECTProjection();
                newCOMMODITY_GROUP_DIRECT.Entity.DESCRIPTION = BluePrintsResources.CommodityCodeGroup_New;

                if (targetCOMMODITY_GROUP != null)
                {
                    if (targetCOMMODITY_GROUP.Entity.GUID_COMMODITYCODE != null)
                    {
                        MessageBoxService.ShowMessage(BluePrintsResources.CommodityGroup_CannotAssignCommodity);
                        continue;
                    }

                    newCOMMODITY_GROUP_DIRECT.Entity.GUID_PARENT = targetCOMMODITY_GROUP.EntityKey;
                    newCOMMODITY_GROUP_DIRECT.Entity.GUID_COMMODITYCODE = droppedCOMMODITY_CODE.GUID;

                    var errorMessage = string.Empty;
                    if (MainViewModel.IsValidEntity(newCOMMODITY_GROUP_DIRECT, ref errorMessage))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(newCOMMODITY_GROUP_DIRECT, null, null, null,
                            EntityMessageType.Added);
                        MainViewModel.Save(newCOMMODITY_GROUP_DIRECT);
                    }
                    else
                    {
                        MessageBoxService.ShowMessage(errorMessage + " is not unique");
                    }
                }
            }

            e.Handled = true;
        }

        protected override void OnClose(CancelEventArgs e)
        {
            base.OnClose(e);
        }

        #endregion
    }
}