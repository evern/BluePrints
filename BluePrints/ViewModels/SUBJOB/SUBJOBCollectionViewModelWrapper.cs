using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class SUBJOBCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <SUBJOB, SUBJOBProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of SUBJOBCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SUBJOBCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new SUBJOBCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the SUBJOBCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the SUBJOBCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected SUBJOBCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT loadPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected BackgroundWorker summaryBackgroundWorker;
        Timer summaryBackgroundWorkerTimer;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
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
            return query => query;
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS);
        }

        protected override Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOBProjection>> specifyMainViewModelProjection()
        {
            return query => SUBJOBProjectionQueries.IDeliverable_Rates_Group_Transformation(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<SUBJOBProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<SUBJOBProjection> entities)
        {
            summaryBackgroundWorker = new BackgroundWorker();
            summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            summaryBackgroundWorker.RunWorkerCompleted += SummaryBackgroundWorker_RunWorkerCompleted;
            summaryBackgroundWorker.WorkerSupportsCancellation = true;

            summaryBackgroundWorkerTimer = new Timer(new TimerCallback(summaryBackgroundTimerCallBack), null, 2000, 1000);
            return base.OnMainViewModelLoaded(entities);
        }

        private void summaryBackgroundTimerCallBack(object state)
        {
            summaryBackgroundWorkerTimer.Change(Timeout.Infinite, Timeout.Infinite);
            summaryBackgroundWorker.RunWorkerAsync();
        }

        private void SummaryBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (summaryBackgroundWorker.CancellationPending)
                return;

            foreach (SUBJOBProjection entity in Entities)
            {
                entity.Update();
            }
        }

        private void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (summaryBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            PROGRESS liveDesignPROGRESS = PROGRESSProjectionFunc()(MainEntityUnitOfWork.PROGRESSES).FirstOrDefault();
            BASELINE liveDesignBASELINE = BASELINEProjectionFunc()(MainEntityUnitOfWork.BASELINES).FirstOrDefault();
            if (liveDesignPROGRESS != null && liveDesignBASELINE != null)
            {
                List<RATE> RATES = RATEProjectionFunc()(MainEntityUnitOfWork.RATES).ToList();
                List<PROGRESS_ITEM> PROGRESS_ITEMS = MainEntityUnitOfWork.PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == liveDesignPROGRESS.GUID).ToList();
                List<VARIATION> VARIATIONS = VARIATIONProjectionFunc()(MainEntityUnitOfWork.VARIATIONS).ToList();
                IQueryable<BASELINE_ITEM> BASELINE_ITEMS = MainEntityUnitOfWork.BASELINE_ITEMS.Where(x => x.GUID_BASELINE == liveDesignBASELINE.GUID);
                IQueryable<BASELINE_ITEMProgress> deliverables = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, loadPROJECT, liveDesignPROGRESS, RATES, PROGRESS_ITEMS, VARIATIONS);

                foreach(SUBJOBProjection entity in Entities)
                {
                    entity.DeliverableRates = deliverables.Where(x => x.Subjob_Guid == entity.GUID);
                }
            }
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(SUBJOBProjection projection, out bool isNew)
        {
            projection.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "SUBJOBCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "SUBJOBCollectionViewModelWrapper_v2"; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
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
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<VARIATION> VARIATIONCollection
        {
            get
            {
                return GetEntities<VARIATION>();
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                return GetSUBAREACollection();
            }
        }

        public IEnumerable<AREA> GetSUBAREACollection()
        {
            var collection = GetEntities<AREA>();
            if (collection != null)
                collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
            return collection;
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
        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, SUBJOBProjection projection, bool isNew)
        {
            if (MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation)
                return;

            if(isNew && (loadPROJECT.STATUS == ProjectStatus.Tender || loadPROJECT.STATUS == ProjectStatus.TenderSubmitted) && projection.Entity.BELLCURVESHAPE == null)
            {
                projection.Entity.BELLCURVESHAPE = BellCurveShape.Balanced;
            }

            if (field_name == "Entity.STARTDATE" || field_name == "Entity.ENDDATE" || field_name == "Entity.REVIEWSTARTDATE" || field_name == "Entity.REVIEWENDDATE")
            {
                if(projection.Entity.AUTOGENERATED != false)
                {
                    string autoGeneratedFieldName = BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().AUTOGENERATED);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, autoGeneratedFieldName, true, false, EntityMessageType.Changed);
                    projection.Entity.AUTOGENERATED = false;
                }
            }

            //auto populate internal name
            if (field_name == "Entity.GUID_DPHASE" || field_name == "Entity.GUID_DDISCIPLINE" || field_name == "Entity.GUID_DDOCTYPE" || field_name == "Entity.GUID_DAREA" || field_name == "Entity.GUID_DSUBAREA")
            {
                Guid? phaseGuid = null;
                string newInternalName = BluePrintsDataUtils.SUBJOB_Generate_InternalNumber(projection.Entity.GUID_DAREA, projection.Entity.GUID_DSUBAREA, loadPROJECT, AREACollection, SUBAREACollection, out phaseGuid, projection.Entity.GUID_DPHASE, PHASECollection);

                if (newInternalName == string.Empty || projection.Entity.INTERNAL_NAME1 == newInternalName)
                    return;

                if(!MainViewModel.EntitiesUndoRedoManager.IsInUndoRedoOperation)
                {
                    string internalNumFieldName = BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().INTERNAL_NAME1);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, internalNumFieldName, projection.Entity.INTERNAL_NAME1, newInternalName, EntityMessageType.Changed);
                    projection.Entity.INTERNAL_NAME1 = newInternalName;
                }
            }

            //set sub area to null when area changed
            if (field_name == BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().GUID_DAREA))
            {
                Guid? oldValue = projection.Entity.GUID_DSUBAREA;
                Guid? newValue = (Guid?)null;

                projection.Entity.GUID_DSUBAREA = newValue;
                if (!isNew)
                {
                    string subAreaFieldName = BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().SubAreaGuid);
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
                else
                {
                    //Area is required immediately for subarea selection
                    projection.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    projection.Update();
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().STARTDATE) ||
                     field_name == BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().ENDDATE))
            {
                DateTime startDate;
                DateTime endDate;

                string endDateFieldname = BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().ENDDATE);
                string startDateFieldname = BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().STARTDATE);
                if (field_name == BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().STARTDATE))
                {
                    if(projection.Entity.ENDDATE != null && new_value != null)
                    {
                        startDate = (DateTime)new_value;
                        endDate = (DateTime)projection.Entity.ENDDATE;
                        if (endDate < startDate)
                        {
                            endDate = BluePrintsDataUtils.SUBJOB_Calculate_EndDate(startDate, loadPROJECT);
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, endDateFieldname, projection.Entity.ENDDATE, endDate, EntityMessageType.Changed);
                            projection.Entity.ENDDATE = endDate;
                        }

                        calculateReviewStartEndDate(projection, startDate, endDate);
                    }
                }
                else
                {
                    if(new_value != null)
                    {
                        endDate = (DateTime)new_value;
                        if (projection.Entity.STARTDATE != null)
                        {
                            startDate = (DateTime)projection.Entity.STARTDATE;
                            if (endDate < startDate)
                            {
                                startDate = BluePrintsDataUtils.SUBJOB_Calculate_StartDate(endDate, loadPROJECT);
                                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, startDateFieldname, projection.Entity.STARTDATE, startDate, EntityMessageType.Changed);
                                projection.Entity.STARTDATE = startDate;
                            }

                            calculateReviewStartEndDate(projection, startDate, endDate);
                        }
                    }
                }
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        private void calculateReviewStartEndDate(SUBJOBProjection subjob, DateTime reviewStartDate, DateTime reviewEndDate)
        {
            DateTime endDate = reviewEndDate;
            BluePrintsDataUtils.SUBJOB_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate, loadPROJECT, false);
            subjob.Entity.REVIEWSTARTDATE = reviewStartDate;

            string reviewEndDateFieldname = BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().REVIEWENDDATE);
            if (reviewEndDate >= endDate)
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(subjob, reviewEndDateFieldname, subjob.Entity.REVIEWENDDATE, endDate, EntityMessageType.Changed);
                subjob.Entity.REVIEWENDDATE = endDate;
            }
            else
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(subjob, reviewEndDateFieldname, subjob.Entity.REVIEWENDDATE, endDate, EntityMessageType.Changed);
                subjob.Entity.REVIEWENDDATE = reviewEndDate;
            }

            subjob.Update();
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
                var newProjection = new SUBJOBProjection();
                DataUtils.ShallowCopy(newProjection.Entity, selectedEntity.Entity);
                newProjection.Entity.GUID = Guid.Empty;
                var selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.Entity.GUID_DAREA);
                var selectedSUBAREA = SUBAREACollection.FirstOrDefault(x => x.GUID == newProjection.Entity.GUID_DSUBAREA);
                var selectedPHASE =
                    DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.GUID_DPHASE);

                Guid? phaseGuid;
                string newInternalName = BluePrintsDataUtils.SUBJOB_Generate_InternalNumber(
                        newProjection.Entity.GUID_DAREA, newProjection.Entity.GUID_DSUBAREA, loadPROJECT, 
                        AREACollection, SUBAREACollection, out phaseGuid, newProjection.Entity.GUID_DPHASE, PHASECollection);

                newProjection.Entity.INTERNAL_NAME1 = newInternalName;

                //handled in save
                //MainViewModel.EntitiesUndoRedoManager.AddUndo(newProjection, null, null, null, EntityMessageType.Added);
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

        public bool CanAutoPopulate(object button)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            if (info.Column == null)
                return;

            if (info.Column.FieldName != BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." +
                    BindableBase.GetPropertyName(() => new SUBJOB().INTERNAL_NAME1))
                return;

            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            var entitiesToSave = new List<SUBJOBProjection>();
            foreach (var entity in MainViewModel.SelectedEntities)
            {
                Guid? phaseGuid;
                string generatedInternalName = BluePrintsDataUtils.SUBJOB_Generate_InternalNumber
                    (entity.Entity.GUID_DAREA, 
                    entity.Entity.GUID_DSUBAREA, loadPROJECT,
                    AREACollection, SUBAREACollection, out phaseGuid, entity.Entity.GUID_DPHASE, PHASECollection);

                if (generatedInternalName == string.Empty)
                    return;

                SetMainNestedValueWithUndoAndRefresh(entity, info.Column.FieldName, generatedInternalName);
                entitiesToSave.Add(entity);
            }

            MainViewModel.BaseBulkSave(entitiesToSave);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            BackgroundRefresh();
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

        public override string UnifiedRowValidation(SUBJOBProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(SUBJOBProjection projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().REVIEWSTARTDATE) ||
                     field_name == BindableBase.GetPropertyName(() => new SUBJOBProjection().Entity) + "." + BindableBase.GetPropertyName(() => new SUBJOB().REVIEWENDDATE))
            {
                if (new_value != null)
                {
                    DateTime newDate = (DateTime)new_value;
                    if (newDate > projection.Entity.ENDDATE || newDate < projection.Entity.STARTDATE)
                        return "Review dates cannot be more than end date or less than start date";

                }
            }

            return string.Empty;
        }

        protected override void OnClose(CancelEventArgs e)
        {
            summaryBackgroundWorker.CancelAsync();
            base.OnClose(e);
        }
        #endregion
    }
}