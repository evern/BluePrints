using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
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
    public partial class COMMODITY_CODECollectionViewModelWrapper :
        BluePrintsEntitiesTreeCollectionWrapper
        <COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODESCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODECollectionViewModelWrapper(unitOfWorkFactory));
        }

        private DispatcherTimer delayedCOMMODITY_CODEPopulateDispatcher;

        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODESCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODESCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODECollectionViewModelWrapper(
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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc);
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
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COMMODITYCODETYPE == loadCommodityCodeType);
            else
                return query => query.Where(x => x.GUID_PROJECT == null && x.COMMODITYCODETYPE == loadCommodityCodeType);
        }

        public Action ShowDISCIPLINE;
        public Action ShowDEPARTMENT;
        public Action ShowINDIRECT_TYPE;
        public Action ShowHOURSAWEEK;
        public Action ShowDIRECT_RATES;
        public Action ShowINDIRECT_RATES;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_CODE> entities)
        {
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

            mainThreadDispatcher.BeginInvoke(new Action(() => delayedCOMMODITY_CODEPopulateDispatcher.Start()));
            base.AssignCallBacksAndRaisePropertyChange(entities);
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
                        newCOMMODITY_CODE.ISQUANTIFIABLE = false;
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
                            newCOMMODITY_CODE.ISQUANTIFIABLE = false;
                            MainViewModel.Save(newCOMMODITY_CODE);
                        }
            }
            else if (loadCommodityCodeType == CommodityCodeType.Indirect)
            {
                foreach (var populateINDIRECT_TYPE in INDIRECT_TYPECollection)
                    if (!MainViewModel.Entities.Any(x => x.GUID_PARENT == Guid.Empty && x.GUID_INDIRECTTYPE == populateINDIRECT_TYPE.GUID))
                    {
                        var newCOMMODITY_CODE = new COMMODITY_CODE();
                        newCOMMODITY_CODE.CODE = populateINDIRECT_TYPE.CODE;
                        newCOMMODITY_CODE.FULLCODE = populateINDIRECT_TYPE.CODE;
                        newCOMMODITY_CODE.NAME = populateINDIRECT_TYPE.NAME;
                        newCOMMODITY_CODE.GUID_INDIRECTTYPE = populateINDIRECT_TYPE.GUID;
                        newCOMMODITY_CODE.ISQUANTIFIABLE = false;
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
        #endregion

        #endregion

        #region View Behavior
        protected override void onAfterDroppedCopySpecificProperties(COMMODITY_CODE droppedProjection, COMMODITY_CODE targetProjection)
        {
            MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedProjection,
                            BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_DISCIPLINE),
                            droppedProjection.GUID_DISCIPLINE, targetProjection.GUID_DISCIPLINE,
                            EntityMessageType.Changed);

            droppedProjection.GUID_DISCIPLINE = targetProjection.GUID_DISCIPLINE;

            MainViewModel.EntitiesUndoRedoManager.AddUndo(droppedProjection,
                BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_DEPARTMENT),
                droppedProjection.GUID_DEPARTMENT, targetProjection.GUID_DEPARTMENT,
                EntityMessageType.Changed);

            droppedProjection.GUID_DEPARTMENT = targetProjection.GUID_DEPARTMENT;
        }

        protected override void onReorderingPopulateOrderSpecificProperties(COMMODITY_CODE orderingProjection)
        {
            int tryParseInt;
            if (orderingProjection.CODE == "temp")
                orderingProjection.CODE = GenerateOrderString(orderingProjection.SORTORDER);
            else
                orderingProjection.CODE = int.TryParse(orderingProjection.CODE, out tryParseInt)
                    ? AddUndoOnCODEChanges(orderingProjection, GenerateOrderString(orderingProjection.SORTORDER))
                    : orderingProjection.CODE;
        }

        protected override void onReorderingPopulateParentSpecificProperties(COMMODITY_CODE parentProjection)
        {
            var oldBoolValue = parentProjection.ISQUANTIFIABLE;
            var newBoolValue = false;
            parentProjection.ISQUANTIFIABLE = newBoolValue;
            MainViewModel.EntitiesUndoRedoManager.AddUndo(parentProjection, BindableBase.GetPropertyName(() => new COMMODITY_CODE().ISQUANTIFIABLE), oldBoolValue, newBoolValue, EntityMessageType.Changed);
        }

        protected override void onReorderingPopulateChildSpecificProperties(COMMODITY_CODE childProjection)
        {
            var oldBoolValue = childProjection.ISQUANTIFIABLE;
            var newBoolValue = true;
            childProjection.ISQUANTIFIABLE = newBoolValue;
            MainViewModel.EntitiesUndoRedoManager.AddUndo(childProjection, BindableBase.GetPropertyName(() => new COMMODITY_CODE().ISQUANTIFIABLE), oldBoolValue, newBoolValue, EntityMessageType.Changed);
        }

        protected override COMMODITY_CODE onAfterReorderingParentProperties(Guid? guid_parent)
        {
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

                bool? oldBoolValue = null;
                if (parentCOMMODITY_CODE.ISQUANTIFIABLE)
                {
                    oldBoolValue = parentCOMMODITY_CODE.ISQUANTIFIABLE;
                    parentCOMMODITY_CODE.ISQUANTIFIABLE = false;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(parentCOMMODITY_CODE,
                        BindableBase.GetPropertyName(() => new COMMODITY_CODE().ISQUANTIFIABLE), oldBoolValue, false,
                        EntityMessageType.Changed);
                }

                if (oldValue != null || oldBoolValue != null)
                    return parentCOMMODITY_CODE;
                else
                    return null;
            }

            return null;
        }

        private void RecurseRenameChildrenFULLCODE(Guid? guid_parent)
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

        #endregion

        #region View Commands
        protected override string GetParentEntityKeyFieldName()
        {
            return BindableBase.GetPropertyName(() => new COMMODITY_CODE().GUID_PARENT);
        }

        protected override string GetSortOrderFieldName()
        {
            return BindableBase.GetPropertyName(() => new COMMODITY_CODE().SORTORDER);
        }

        protected override void PopulateNewProjection(COMMODITY_CODE projection)
        {
            projection.CODE = "temp";
            projection.FULLCODE = "temp";
            projection.NAME = BluePrintsResources.CommodityCode_NewCommodity;
            projection.GUID_DISCIPLINE = DisplaySelectedEntity == null
                ? DISCIPLINECollection.First().GUID
                : DisplaySelectedEntity.GUID_DISCIPLINE;
            projection.GUID_DEPARTMENT = DisplaySelectedEntity == null
                ? DEPARTMENTCollection.First().GUID
                : DisplaySelectedEntity.GUID_DEPARTMENT;
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