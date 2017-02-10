using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.DataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Common.Helpers;
using DevExpress.Xpf.Bars;

namespace BluePrints.ViewModels
{
    public class WORKPACKCollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <WORKPACK, WORKPACKProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<WORKPACK, WORKPACKProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
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

        private PROJECT loadPROJECT;
        private PROGRESS loadPROGRESS;
        private BASELINE loadBASELINE;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0,
                bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT,
                OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(1,
                bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, typeof(PROJECT), null, OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>(2,
                bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc, typeof(PROJECT), null, OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(3,
                bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, null, null, null, OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(4,
                bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES, null, null, null, OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(5,
                bluePrintsUnitOfWorkFactory, x => x.DOCTYPES, null, null, null, OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(6,
                bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, typeof(PROJECT), isContinueLoadingAfterBASELINE, 
                OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(7,
                bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc, typeof(BASELINE), null, 
                OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(8,
                bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, typeof(PROJECT), isContinueLoadingAfterPROGRESS,
                OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(9,
                bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, typeof(PROGRESS), null,
                OnSimpleEntitiesChanged);
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(10,
                bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROJECT), null,
                OnSimpleEntitiesChanged);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            loadPROJECT = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            loadBASELINE = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterPROGRESS(IEnumerable<PROGRESS> entities)
        {
            loadPROGRESS = entities.First();
            return true;
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if(loadPROGRESS == null)
                return query => query.Where(x => x.GUID_PROGRESS == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            if (loadBASELINE == null)
                return query => query.Where(x => x.GUID_BASELINE == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACKProjection>> ConstructMainViewModelProjection()
        {
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS = loaderCollection.GetCollection<BASELINE_ITEM>();
            var getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            var getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();

            return query => WORKPACKProjectionQueries.JoinPROGRESSProjectionOnWORKPACKS(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), BASELINE_ITEMS, getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<WORKPACKProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = OnEntitySavedCallBack;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        private void OnSimpleEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            if (sender.ToString() == MainViewModel.ToString())
                return;

            if (loadPROJECT != null && changedType == typeof(PROJECT) &&
                loadPROJECT.GUID.ToString() == key.ToString())
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored,
                        StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                        StringFormatUtils.GetEntityNameByType(changedType)));

            if (loadPROJECT != null)
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
        }

        #region Collection Call Backs

        public WORKPACKProjection CreateNewProjectionFromNewEntityCallBack(WORKPACK entity)
        {
            return new WORKPACKProjection();
        }

        public void ApplyProjectionPropertiesToEntity(WORKPACKProjection projectionEntity, WORKPACK entity)
        {
            projectionEntity.WORKPACK.GUID_PROJECT = loadPROJECT.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.WORKPACK);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.WORKPACK.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.WORKPACK.CREATED;
        }

        public void OnEntitySavedCallBack(Guid primaryKey, WORKPACKProjection projectionEntity,
            WORKPACK entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "WORKPACKCollectionViewModelWrapper"; }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
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

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        #endregion

        #region View Behavior
        /// <summary>
        /// Allow cells to commit immediately upon losing focus
        /// </summary>
        public void CellValueChanged(CellValueChangedEventArgs e)
        {
            var changedWORKPACK = (WORKPACKProjection)e.Row;
            if (e.Column.FieldName == "GUID_DDISCIPLINE" || e.Column.FieldName == "GUID_DDOCTYPE")
            {
                var newInternalName = BluePrintDataUtils.WORKPACK_Generate_InternalNumber1(loadPROJECT,
                    changedWORKPACK.WORKPACK, MainViewModel.Entities.Select(x => x.WORKPACK), AREACollection, DISCIPLINECollection, DOCTYPECollection);
                if (newInternalName == string.Empty)
                    return;

                if (
                    MessageBoxService.ShowMessage(CommonResources.WORKPACK_InternalNameChange,
                        CommonResources.Confirmation_Caption, MessageButton.YesNo) != MessageResult.Yes)
                    return;

                changedWORKPACK.WORKPACK.INTERNAL_NAME1 = newInternalName;

                if (e.Column.FieldName == "GUID_DDISCIPLINE")
                    changedWORKPACK.WORKPACK.INTERNAL_NAME2 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT,
                        changedWORKPACK.WORKPACK, MainViewModel.Entities.Select(x => x.WORKPACK), AREACollection, DISCIPLINECollection, PHASECollection);
            }
            else if (e.Column.FieldName == "GUID_DPHASE" || e.Column.FieldName == "GUID_DAREA")
            {
                var newInternalName = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT,
                    changedWORKPACK.WORKPACK, MainViewModel.Entities.Select(x => x.WORKPACK), AREACollection, DISCIPLINECollection, PHASECollection);
                if (newInternalName == string.Empty)
                    return;

                if (
                    MessageBoxService.ShowMessage(CommonResources.WORKPACK_InternalNameChange,
                        CommonResources.Confirmation_Caption, MessageButton.YesNo) != MessageResult.Yes)
                    return;

                changedWORKPACK.WORKPACK.INTERNAL_NAME2 = newInternalName;
            }
            else if (e.Column.FieldName == "STARTDATE" || e.Column.FieldName == "ENDDATE" ||
                     e.Column.FieldName == "REVIEWSTARTDATE" || e.Column.FieldName == "REVIEWENDDATE")
            {
                changedWORKPACK.WORKPACK.AUTOGENERATED = false;
            }

            if (e.RowHandle != DataControlBase.NewItemRowHandle)
                MainViewModel.Save(changedWORKPACK);
        }

        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "GUID_DDOCTYPE")
            {
                var changingWORKPACK = (WORKPACK) e.Row;
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid) e.Value);
                if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
                {
                    changingWORKPACK.GUID_DDEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;
                    MainViewModel.UpdateSelectedEntity();
                }
            }
            else if (e.Column.FieldName == "STARTDATE" || e.Column.FieldName == "ENDDATE")
            {
                DateTime startDate;
                DateTime endDate;

                var changingWORKPACK = (WORKPACK) e.Row;
                if (e.Column.FieldName == "STARTDATE")
                {
                    startDate = (DateTime) e.Value;
                    endDate = (DateTime) changingWORKPACK.ENDDATE;
                    if (endDate < startDate)
                    {
                        endDate = BluePrintDataUtils.WORKPACK_Calculate_EndDate(startDate, loadPROJECT);
                        changingWORKPACK.ENDDATE = endDate;
                    }
                }
                else
                {
                    endDate = (DateTime) e.Value;
                    startDate = (DateTime) changingWORKPACK.STARTDATE;
                    if (endDate < startDate)
                    {
                        startDate = BluePrintDataUtils.WORKPACK_Calculate_StartDate(endDate, loadPROJECT);
                        changingWORKPACK.STARTDATE = startDate;
                    }
                }

                var reviewStartDate = startDate;
                var reviewEndDate = endDate;

                BluePrintDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate, loadPROJECT,
                    false);
                changingWORKPACK.REVIEWSTARTDATE = reviewStartDate;

                if (reviewEndDate >= endDate)
                    changingWORKPACK.REVIEWENDDATE = endDate;
                else
                    changingWORKPACK.REVIEWENDDATE = reviewEndDate;

                MainViewModel.UpdateSelectedEntity();
            }
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
            if (!_isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            foreach (var selectedEntity in MainViewModel.SelectedEntities)
            {
                var newProjection = new WORKPACKProjection();
                DataUtils.ShallowCopy(newProjection.WORKPACK, selectedEntity.WORKPACK);
                newProjection.WORKPACK.GUID = Guid.Empty;
                var selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.WORKPACK.GUID_DAREA);
                var selectedDISCIPLINE =
                    DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.WORKPACK.GUID_DDISCIPLINE);
                var selectedDOCTYPE =
                    DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.WORKPACK.GUID_DDOCTYPE);
                var selectedPHASE =
                    DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.WORKPACK.GUID_DPHASE);

                newProjection.WORKPACK.INTERNAL_NAME1 =
                    BluePrintDataUtils.WORKPACK_Generate_InternalNumber1(loadPROJECT, newProjection.WORKPACK
                        , MainViewModel.Entities.Select(x => x.WORKPACK), AREACollection, DISCIPLINECollection,
                        DOCTYPECollection);

                newProjection.WORKPACK.INTERNAL_NAME2 =
                    BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT, newProjection.WORKPACK
                        , MainViewModel.Entities.Select(x => x.WORKPACK), AREACollection, DISCIPLINECollection,
                        PHASECollection);

                MainViewModel.EntitiesUndoRedoManager.AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                MainViewModel.Save(newProjection);
            }

            if (!_isProcessingMultipleDuplicates)
                MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        private bool _isProcessingMultipleDuplicates;

        /// <summary>
        /// Paste clipboard data multiple times
        /// </summary>
        public void DuplicateMultiple(BarEditItem barEdit)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            _isProcessingMultipleDuplicates = true;
            var timesToDuplicate = 0;
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
                for (var i = 0; i < timesToDuplicate; i++)
                    Duplicate();
            _isProcessingMultipleDuplicates = false;
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }
        #endregion
    }
}