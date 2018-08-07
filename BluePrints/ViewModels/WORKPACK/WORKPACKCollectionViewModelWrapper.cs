using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class WORKPACKCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <WORKPACK, WORKPACKProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of WORKPACKCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACKCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new WORKPACKCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the WORKPACKCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACKCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected WORKPACKCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private Data.PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<Data.PROJECT, Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_GROUPS, STOCK_GROUPProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS, ESTIMATE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS, ESTIMATE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.TASK, TASKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.P6_ASSIGNMENTS, P6_ASSIGNMENTProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<STOCK_GROUP>, IQueryable<STOCK_GROUP>> STOCK_GROUPProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID));
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {

            return query => query.Where(x => x.BASELINE.GUID_PROJECT == loadPROJECT.GUID && x.BASELINE.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEM>> ESTIMATE_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.ESTIMATE.GUID_PROJECT == loadPROJECT.GUID && x.ESTIMATE.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<P6Data.PROJECT>, IQueryable<P6Data.PROJECT>> P6PROJECTProjectionFunc()
        {
            BASELINE liveBASELINE = loadPROJECT.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
            ESTIMATE liveESTIMATE = loadPROJECT.ESTIMATE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);

            if (liveBASELINE != null && liveESTIMATE != null)
                return query => query.Where(x => x.proj_short_name == liveBASELINE.P6BASELINE_NAME || x.proj_short_name == liveESTIMATE.P6BASELINE_NAME);
            else if (liveBASELINE != null)
                return query => query.Where(x => x.proj_short_name == liveBASELINE.P6BASELINE_NAME);
            else if (liveESTIMATE != null)
                return query => query.Where(x => x.proj_short_name == liveESTIMATE.P6BASELINE_NAME);
            else
                return query => query.Where(x => x.proj_short_name == "N/A");
        }

        private Func<IRepositoryQuery<TASK>, IQueryable<TASK>> TASKProjectionFunc()
        {
            PROGRESS designPROGRESS = PROGRESSCollection.FirstOrDefault(x => x.TYPE == PhaseType.Design);
            PROGRESS constructPROGRESS = PROGRESSCollection.FirstOrDefault(x => x.TYPE == PhaseType.Construct);

            if (constructPROGRESS != null && designPROGRESS != null)
                return query => query.Where(x => (x.PROJECT.proj_short_name == designPROGRESS.P6PROGRESS_NAME || x.PROJECT.proj_short_name == constructPROGRESS.P6PROGRESS_NAME) && x.delete_date == null).Where(x => x.TASKACTV.Count > 0).Where(x => x.delete_date == null).Where(x => x.TASKACTV.Any(taskact => taskact.ACTVCODE != null && (taskact.ACTVCODE.actv_code_name.ToUpper() == ProgressType.Design.ToString().ToUpper() || taskact.ACTVCODE.actv_code_name.ToUpper() == PhaseType.Construct.ToString().ToUpper())));
            else if (constructPROGRESS == null)
                return query => query.Where(x => (x.PROJECT.proj_short_name == designPROGRESS.P6PROGRESS_NAME) && x.delete_date == null).Where(x => x.TASKACTV.Count > 0).Where(x => x.delete_date == null).Where(x => x.TASKACTV.Any(taskact => taskact.ACTVCODE != null && (taskact.ACTVCODE.actv_code_name.ToUpper() == ProgressType.Design.ToString().ToUpper())));
            else if (designPROGRESS == null)
                return query => query.Where(x => (x.PROJECT.proj_short_name == constructPROGRESS.P6PROGRESS_NAME) && x.delete_date == null).Where(x => x.TASKACTV.Count > 0).Where(x => x.delete_date == null).Where(x => x.TASKACTV.Any(taskact => taskact.ACTVCODE != null && (taskact.ACTVCODE.actv_code_name.ToUpper() == PhaseType.Construct.ToString().ToUpper())));
            else
                return query => query.Where(x => x.proj_id == 9999);
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>> P6_ASSIGNMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACKProjection>> specifyMainViewModelProjection()
        {
            return query => WORKPACKQueries.WORKPACKProjectionSiteAndOffsiteTransformation(query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID), BASELINE_ITEMCollection, ESTIMATE_ITEMCollection, P6_ASSIGNMENTCollection, RATECollection, null, STOCK_GROUPCollection, STOCK_CODECollection, PROGRESSCollection, P6TASKCollection, loadPROJECT);
        }

        //Do not refresh because it is refresh heavy
        private bool onEntityMessageCallBack(object primaryKey, Type entityType, EntityMessageType messageType, object sender, bool bulkRefresh)
        {
            return false;
        }

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                var collection = GetEntities<P6_ASSIGNMENT>();
                return collection;
            }
        }

        public IEnumerable<BASELINE_ITEM> BASELINE_ITEMCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM>();
                return collection;
            }
        }

        public IEnumerable<TASK> P6TASKCollection
        {
            get
            {
                var collection = GetEntities<TASK>();
                return collection;
            }
        }

        public IEnumerable<ESTIMATE_ITEM> ESTIMATE_ITEMCollection
        {
            get
            {
                var collection = GetEntities<ESTIMATE_ITEM>();
                return collection;
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                var collection = GetEntities<RATE>();
                return collection;
            }
        }

        public IEnumerable<PROGRESS> PROGRESSCollection
        {
            get
            {
                var collection = GetEntities<PROGRESS>();
                return collection;
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                var collection = GetEntities<PROGRESS_ITEM>();
                return collection;
            }
        }

        public IEnumerable<VARIATION> VARIATIONCollection
        {
            get
            {
                var collection = GetEntities<VARIATION>();
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUP> STOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> STOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                return collection;
            }
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<WORKPACKProjection> entities)
        {
            MainViewModel.OnBeforeEntitiesChangedCallBack = onEntityMessageCallBack;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySave;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private bool OnBeforeEntitySave(WORKPACKProjection workpack)
        {
            BluePrintsDataUtils.WORKPACK_Populate_Name(workpack.Entity, SUBJOBCollection, DISCIPLINECollection);
            return true;
        }
        #endregion

        #region View Commands
        public void GenerateWORKPACKS()
        {
            loadBASELINE_ITEMViewModel();
        }

        BASELINE_ITEMCollectionViewModelWrapper baseline_itemCollectionViewModel;
        private void loadBASELINE_ITEMViewModel()
        {
            baseline_itemCollectionViewModel = BASELINE_ITEMCollectionViewModelWrapper.Create();
            baseline_itemCollectionViewModel.OnEntitiesLoadedCallBackManualDispose = true;
            baseline_itemCollectionViewModel.SetParentViewModel(this);
            baseline_itemCollectionViewModel.OnEntitiesLoadedCallBack = onBASELINE_ITEMLoaded;
            var baselineSupportParameterObj = baseline_itemCollectionViewModel as ISupportParameter;
            baselineSupportParameterObj.Parameter = new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(loadPROJECT, null, DeliverablesViewType.Both);
        }

        private void onBASELINE_ITEMLoaded(IEnumerable<BASELINE_ITEMProgress> baseline_items, object parentId)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => generateWorkpacks(baseline_items, parentId)));
        }

        private void generateWorkpacks(IEnumerable<BASELINE_ITEMProgress> baseline_items, object parentId)
        {
            List<WORKPACKProjection> removeWORKPACKS = new List<WORKPACKProjection>();
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            LoadingScreenManager.ShowLoadingScreen(MainViewModel.Entities.Count);
            //LoadingScreenManager.SetMessage("Removing redundant workpacks");
            foreach (WORKPACKProjection workpack in MainViewModel.Entities)
            {
                if (!baseline_items.Any(x => x.Entity.Entity.GUID_WORKPACK == workpack.GUID))
                {
                    removeWORKPACKS.Add(workpack);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(workpack, null, null, null, EntityMessageType.Deleted);
                }

                LoadingScreenManager.Progress();
            }
            MainViewModel.BaseBulkDelete(removeWORKPACKS);

            List<BASELINE_ITEMProgress> baseline_itemsToSave = new List<BASELINE_ITEMProgress>();
            LoadingScreenManager.CloseLoadingScreen();
            LoadingScreenManager.ShowLoadingScreen(baseline_items.Count() * 2);
            //LoadingScreenManager.SetMessage("Assigning workpacks to deliverables");
            foreach(BASELINE_ITEMProgress deliverable in baseline_items)
            {
                Guid? subjob_guid = deliverable.Entity.Entity.GUID_SUBJOB;
                Guid? discipline_guid = deliverable.Entity.Entity.GUID_DISCIPLINE;
                decimal discipline_number = deliverable.Entity.Entity.DISCIPLINE_NUM;

                if(subjob_guid != null && discipline_guid != null)
                {
                    WORKPACKProjection queryWORKPACK = MainViewModel.Entities.FirstOrDefault(x => x.Entity.GUID_DISCIPLINE == discipline_guid && x.Entity.GUID_SUBJOB == subjob_guid && x.Entity.DISCIPLINE_NUM == discipline_number);
                    if (queryWORKPACK == null)
                    {
                        WORKPACKProjection newWORKPACK = new WORKPACKProjection();
                        newWORKPACK.Entity.GUID_SUBJOB = (Guid)subjob_guid;
                        newWORKPACK.Entity.GUID_DISCIPLINE = (Guid)discipline_guid;
                        newWORKPACK.Entity.DISCIPLINE_NUM = discipline_number;
                        MainViewModel.Save(newWORKPACK);
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(newWORKPACK, null, null, null, EntityMessageType.Added);
                        queryWORKPACK = newWORKPACK;
                    }
                    else
                    {
                        //fix internal number with OnBeforeEntitySaved
                        MainViewModel.Save(queryWORKPACK);
                    }

                    deliverable.Entity.Entity.GUID_WORKPACK = queryWORKPACK.GUID;
                    baseline_itemsToSave.Add(deliverable);
                }

                LoadingScreenManager.Progress();
            }

            foreach(BASELINE_ITEMProgress deliverable in baseline_itemsToSave)
            {
                LoadingScreenManager.Progress();
                baseline_itemCollectionViewModel.MainViewModel.Save(deliverable);
            }

            LoadingScreenManager.CloseLoadingScreen();
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();

            baseline_itemCollectionViewModel.CleanUpEntitiesLoader();
            baseline_itemCollectionViewModel = null;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "WORKPACKCollectionViewModelWrapper"; }
        }

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
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
        #endregion

        public override string UnifiedValueValidation(WORKPACKProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(WORKPACKProjection projection)
        {
            return string.Empty;
        }
    }
}