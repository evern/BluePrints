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
using BluePrints.Common.Resources;

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
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => loadBASELINE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => loadPROGRESS = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACKProjection>> ConstructMainViewModelProjection()
        {
            IEnumerable<BASELINE_ITEM> BASELINE_ITEMS = loaderCollection.GetCollection<BASELINE_ITEM>();
            var getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            var getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getDELIVERABLES_STATUSESFunc = loaderCollection.GetCollectionFunc<DELIVERABLES_STATUS>();

            return query => WORKPACKProjectionQueries.JoinPROGRESSProjectionOnWORKPACKS(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), BASELINE_ITEMS, getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<WORKPACKProjection> entities)
        {
            MainViewModel.CreateNewProjectionFromNewEntityCallBack = CreateNewProjectionFromNewEntityCallBack;
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntity;
            MainViewModel.ApplyEntityPropertiesToProjectionCallBack = OnEntitySavedCallBack;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        public WORKPACKProjection CreateNewProjectionFromNewEntityCallBack()
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