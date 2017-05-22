using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class PROGRESS_ITEMSCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        //ensure mainviewmodel is loaded before calling background worker
        private DispatcherTimer onMainViewModelFirstLoadedTimer;
        //calculates the planned values only for each deliverables
        BackgroundWorker calculatePlannedBackgroundWorker;

        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROGRESS_ITEMSCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROGRESS_ITEMSCollectionViewModelWrapper());
        }

        public PROGRESS_ITEMSCollectionViewModelWrapper()
        {
            onMainViewModelFirstLoadedTimer = new DispatcherTimer();
            onMainViewModelFirstLoadedTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            onMainViewModelFirstLoadedTimer.Tick += onMainViewModelFirstLoaded;
            calculatePlannedBackgroundWorker = new BackgroundWorker();
            calculatePlannedBackgroundWorker.DoWork += calculatePlannedBackgroundWorker_DoWork;
            calculatePlannedBackgroundWorker.RunWorkerCompleted += CalculatePlannedBackgroundWorker_RunWorkerCompleted;
            calculatePlannedBackgroundWorker.WorkerSupportsCancellation = true;
        }

        #region Database Operation

        private Data.PROJECT loadPROJECT;
        private PROGRESS loadPROGRESS;
        private BASELINE loadBASELINE;
        private bool isQueryForLiveStatus;
        FullSummarizer fullSummarizer;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IP6EntitiesUnitOfWork p6UOW = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        protected override void InitializeParameters(object parameter)
        {
            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 10);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            var receiveParameter =
                (OptionalEntitiesParameter<Data.PROJECT, PROGRESS>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadPROGRESS = receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => loadBASELINE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, SetPROGRESStoCurrentDateOnLoaded);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, false);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        private void SetPROGRESStoCurrentDateOnLoaded(PROGRESS entity)
        {
            loadPROGRESS = entity;
            if(!isFirstLoaded)
            mainThreadDispatcher.BeginInvoke(new Action(() => DateChange(DateNavigationType.Current)));
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID_PROJECT).OrderBy(x => x.NUMBER);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.APPROVED != null && x.TYPE == VariationType.External && x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        private Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x =>
                            x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Progress_Report.ToString());
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<PROGRESS_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            var getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            var getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getDELIVERABLES_STATUSESFunc =
                loaderCollection.GetCollectionFunc<DELIVERABLES_STATUS>();
            var getWORKPACKSFunc = loaderCollection.GetCollectionFunc<WORKPACK>();
            var getVARIATIONSFunc = loaderCollection.GetCollectionFunc<VARIATION>();
            IP6EntitiesUnitOfWork p6UOW = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            return
                query =>
                    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        query.OrderBy(x => x.INTERNAL_NUM), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, 
                        getRATESFunc, getDELIVERABLES_STATUSESFunc);
        }

        bool isFirstLoaded;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS_ITEMProjection> entities)
        {
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntityCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingRowAddUndoAndSaveCallBack;
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterBASELINE_ITEMEntitySaved;
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.BeforeShownEditor = BeforeShownEditor;
            MainViewModel.SetParentViewModel(this);
            //mainThreadDispatcher.BeginInvoke(new Action(() => InitializeSummarizer(entities)));
            onMainViewModelFirstLoadedTimer.Start();
            base.AssignCallBacksAndRaisePropertyChange(entities);
            isFirstLoaded = true;
        }

        ProjectSummaryStats projectSummary;
        private void onMainViewModelFirstLoaded(object sender, EventArgs e)
        {
            onMainViewModelFirstLoadedTimer.Stop();
            InitializeSummarizer();
            calculatePlannedBackgroundWorker.RunWorkerAsync();
        }

        private void InitializeSummarizer()
        {
            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            List<VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONCollection.AsQueryable(), MainViewModel.Entities.Select(x => x.Entity));
            projectSummary = new ProjectSummaryStats(MainViewModel.Entities, loadPROGRESS, projectVariationAdjustment);
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT, loadBASELINE, loadPROGRESS, WORKPACKCollection, WORKPACKCollection.SelectMany(x => x.WORKPACK_ASSIGNMENT).ToList(), p6UOW);
            fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, loadPROJECT.NUMBER);
        }

        private void calculatePlannedBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            isBusy = true;
            if (calculatePlannedBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            fullSummarizer.BuildBudgetedOnly();
        }

        public bool isBusy { get; set; }
        private void CalculatePlannedBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isBusy = false;
            mainThreadDispatcher.BeginInvoke(new Action(() => RefreshView()));
        }

        public override void OnAfterAffectingEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            //do no perform any refresh when progress is changed from here
            if (sender == PROGRESS_ITEMSCollectionViewModel)
                return;

            base.OnAfterAffectingEntitiesChanged(key, changedType, messageType, sender);
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (changedType == typeof(PROGRESS_ITEM))
            {
                PROGRESS_ITEMProjection mainEntity = MainViewModel.Entities.Where(x => x.PROGRESS_ITEMCurrent != null).FirstOrDefault(x => x.PROGRESS_ITEMCurrent.GUID.ToString() == key.ToString());
                if (mainEntity != null)
                {
                    //got to make sure sender is not MainViewModel or else it'll not be refreshed
                    mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(mainEntity.EntityKey, EntityMessageType.Changed, this))));
                    return true;
                }
            }

            return false;
        }

        #region Collection Call Backs
        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanged(CellValueChangedEventArgs e)
        {

        }

        private bool ExistingRowAddUndoAndSaveCallBack(PROGRESS_ITEMProjection projectionEntity, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                 BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." + BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_STATUS))
            {
                PROGRESS_ITEMProjection activeEntity = (PROGRESS_ITEMProjection)e.Row;
                DELIVERABLES_STATUS findDeliverableStatus = DELIVERABLES_STATUSCollection.FirstOrDefault(x => x.GUID == (Guid)e.Value);
                if(findDeliverableStatus != null)
                {
                    decimal? autoAssignPercentage = findDeliverableStatus.AUTO_PERCENTAGE;
                    if (autoAssignPercentage != null)
                    {
                        if(autoAssignPercentage >= activeEntity.MinPercentage)
                            activeEntity.TOTAL_EARNED_PERCENTAGE = (decimal)autoAssignPercentage;
                    }

                    SaveProgressItem(projectionEntity);
                }

                return true;
            }
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().TOTAL_EARNED_PERCENTAGE))
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity, e.Column.FieldName, e.OldValue, e.Value,
                    EntityMessageType.Changed);
                SaveProgressItem(projectionEntity);
                return false;
            }
            else if (
                e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_STATUS)
                ||
                e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_USER))
            {
                return true;
            }

            return false;
        }

        private void ApplyProjectionPropertiesToEntityCallBack(PROGRESS_ITEMProjection projectionEntity, BASELINE_ITEM entity)
        {
            entity = projectionEntity.Entity.Entity;
        }

        /// <summary>
        /// Save progress item during BASELINE_ITEM Undo/Redo operation
        /// </summary>
        /// <param name="projectionEntity"></param>
        /// <param name="isNewEntity"></param>
        private void OnAfterBASELINE_ITEMEntitySaved(PROGRESS_ITEMProjection projectionEntity, bool isNewEntity)
        {
            SaveProgressItem(projectionEntity);
        }

        /// <summary>
        /// Saving derivated progress_item from projectionEntity which is fundamentally baseline_item
        /// </summary>
        /// <param name="projectionEntity"></param>
        private void SaveProgressItem(PROGRESS_ITEMProjection projectionEntity)
        {
            var findPROGRESS_ITEM =
                PROGRESS_ITEMSCollectionViewModel.Entities.FirstOrDefault(
                    x =>
                        x.GUID_ORIBASEITEM == projectionEntity.Entity.Entity.GUID_ORIGINAL &&
                        x.EARNED_DATE == loadPROGRESS.DATA_DATE);
            PROGRESS_ITEM savePROGRESS_ITEM;
            if (findPROGRESS_ITEM == null)
            {
                if (projectionEntity.PROGRESS_ITEMCurrent == null)
                    return;

                savePROGRESS_ITEM = projectionEntity.PROGRESS_ITEMCurrent;
            }
            else
            {
                savePROGRESS_ITEM = findPROGRESS_ITEM;
                savePROGRESS_ITEM.EARNED_UNITS = projectionEntity.PROGRESS_ITEMCurrent.EARNED_UNITS;
            }

            savePROGRESS_ITEM.EARNED_DATE = loadPROGRESS.DATA_DATE;
            savePROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
            savePROGRESS_ITEM.GUID_ORIBASEITEM = projectionEntity.Entity.Entity.GUID_ORIGINAL;
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (savePROGRESS_ITEM.CREATED.Date.Year == 1)
                savePROGRESS_ITEM.CREATED = DateTime.Now;

            PROGRESS_ITEMSCollectionViewModel.Save(savePROGRESS_ITEM);
            projectionEntity.PROGRESS_ITEMCurrent = savePROGRESS_ITEM;
        }

        public bool ValidateFillDownCallBack(PROGRESS_ITEMProjection fillDownEntity, string fieldName, object fillValue)
        {
            if (fieldName != BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().TOTAL_EARNED_PERCENTAGE))
                return false;

            var newPercentage = (decimal)fillValue;
            if (newPercentage > fillDownEntity.MaxPercentage)
                return false;
            else if (newPercentage < fillDownEntity.MinPercentage)
                return false;

            return true;
        }

        public override void FullRefresh()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => StoreViewState()));
            InitializeAndLoadEntitiesLoaderDescription();
        }
        #endregion

        #endregion

        #region View Behavior

        private bool BeforeShownEditor(EditorEventArgs e)
        {
            if (e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().TOTAL_EARNED_PERCENTAGE))
            {
                var view = e.Source as TableView;
                if (view == null)
                    return false;

                var textEditor = view.ActiveEditor as TextEdit;
                if (textEditor == null)
                    return false;

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textEditor.SelectionStart = 0;
                    textEditor.SelectionLength = textEditor.Text.Length;
                }), DispatcherPriority.Background);

                return false;
            }

            return true;
        }

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROGRESS_ITEMSViewModelWrapper"; }
        }

        /// <summary>
        /// The workpack internal name to be used
        /// </summary>
        public string WORKPACKDisplayMember
        {
            get
            {
                if (loadBASELINE == null || loadBASELINE.PROJECT.USELEGACYWORKPACK)
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME1);
                else
                    return BindableBase.GetPropertyName(() => new WORKPACK().INTERNAL_NAME2);
            }
        }

        public CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
            PROGRESS_ITEMSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<PROGRESS_ITEM>();
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1).OrderBy(x => x.INTERNAL_NAME2);
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

        public IEnumerable<VARIATION> VARIATIONCollection
        {
            get
            {
                var collection = GetEntities<VARIATION>();
                return collection;
            }
        }

        public IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection
        {
            get
            {
                var collection = GetEntities<DELIVERABLES_STATUS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.MAX_PERCENTAGE);
                return collection;
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public string DataDate
        {
            get
            {
                if (loadPROGRESS == null || loadPROGRESS.DATA_DATE == null)
                    return string.Empty;

                return loadPROGRESS.DATA_DATE.ToString("g");
            }
        }

        public bool CanDateBackward()
        {
            //if (isBusy)
            //    return false;

            if (MainViewModel == null || MainViewModel.IsLoading)
                return false;

            if (loadPROGRESS.DATA_DATE > loadPROGRESS.PROGRESS_START)
                return true;

            return false;
        }

        public bool CanDateForward()
        {
            //if (isBusy)
            //    return false;

            if (MainViewModel == null || MainViewModel.IsLoading)
                return false;

            return true;
        }

        public void DateForward()
        {
            DateChange(DateNavigationType.Forward);
        }

        public void DateBackward()
        {
            DateChange(DateNavigationType.Backward);
        }

        private DispatcherTimer delayedPROGRESSSavingDispatcher;

        private void delayedPROGRESSSavingDispatcher_Tick(object sender, EventArgs e)
        {
            delayedPROGRESSSavingDispatcher.Stop();
            var PROGRESSCollectionViewModel =
                (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROGRESS>();
            mainThreadDispatcher.BeginInvoke(new Action(() => PROGRESSCollectionViewModel.Save(loadPROGRESS)));
            CancelBackgroundWorker();
            FullRefresh();
        }

        private void DateChange(DateNavigationType navigationType)
        {
            if (isBusy)
                return;

            var interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            int multiplier;
            if (navigationType == DateNavigationType.Current)
            {
                var timeDifferenceFromCurrent = loadPROGRESS.DATA_DATE - DateTime.Now;

                if (timeDifferenceFromCurrent.TotalSeconds > interval.TotalSeconds)
                {
                    do
                    {
                        loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(-1 * interval.Days);
                    } while (loadPROGRESS.DATA_DATE > DateTime.Now);
                }
                else
                {
                    if (timeDifferenceFromCurrent.TotalSeconds < -1 * interval.TotalSeconds)
                        do
                        {
                            loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(1 * interval.Days);
                        } while (loadPROGRESS.DATA_DATE < DateTime.Now - interval);
                    else
                        return;
                }
            }
            else
            {
                multiplier = navigationType == DateNavigationType.Forward ? 1 : -1;
                loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(multiplier * interval.Days);
            }

            delayedPROGRESSSavingDispatcher.Start();
        }

        public bool CanPushToP6Original()
        {
            return CanPushToP6();
        }

        public bool CanPushToP6Modified()
        {
            return CanPushToP6();
        }

        private bool CanPushToP6()
        {
            if (isPushingToP6 || loadPROGRESS == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return false;

            return true;
        }

        public void PushToP6Original()
        {
            PushToP6(BaselineMappingSelectionType.Original);
        }

        public void PushToP6Modified()
        {
            PushToP6(BaselineMappingSelectionType.Modified);
        }

        bool isPushingToP6;
        private BASELINE_ITEMSchedulingViewModelWrapper BASELINE_ITEMSchedulingViewModel;
        private void PushToP6(BaselineMappingSelectionType mappingSelectionType)
        {
            if (loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return;

            isPushingToP6 = true;
            //Stats will be built in SummarizeSinglePROJECTDashboard within SummarizeBASELINE_ITEMDashboard in ConstructMainViewModelProjection
            BASELINE_ITEMSchedulingViewModel = BASELINE_ITEMSchedulingViewModelWrapper.Create();
            BASELINE_ITEMSchedulingViewModel.OnMappingViewModelLoaded = OnPROJECTBASELINE_ITEMSMappingViewModelLoaded;
            var ParameterObj = BASELINE_ITEMSchedulingViewModel as ISupportParameter;
            ParameterObj.Parameter = new object[] { loadPROGRESS, mappingSelectionType };
        }

        private void OnPROJECTBASELINE_ITEMSMappingViewModelLoaded(IEnumerable<BASELINE_ITEMProjection> entities)
        {
            IEnumerable<TASK> PROJECTTASK = BASELINE_ITEMSchedulingViewModel.P6TASKCollection;
            ICollectionViewModel<TASK> P6TASKCollectionViewModel = BASELINE_ITEMSchedulingViewModel.P6TASKCollectionViewModel;
            if (PROJECTTASK.Count() == 0)
                return;

            List<string> processedP6Task = new List<string>();
            TimeSpan intervalTimeSpan = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);

            IEnumerable<BASELINE_ITEMProjection> baseline_itemProjection = entities.Where(x => x.TOTAL_UNITS > 0);

            LoadingScreenManager.ShowLoadingScreen(baseline_itemProjection.Count());
            string errorMessage = string.Empty;

            foreach (BASELINE_ITEMProjection baseline_item in baseline_itemProjection)
            {
                PROGRESS_ITEMProjection currentPROGRESS_ITEM = MainViewModel.Entities.FirstOrDefault(x => x.GUID == baseline_item.GUID);
                LoadingScreenManager.Progress();
                if (currentPROGRESS_ITEM == null)
                    continue;

                if (currentPROGRESS_ITEM.Stats == null || currentPROGRESS_ITEM.Stats.totalUnits == 0)
                    continue;

                if (currentPROGRESS_ITEM.PROGRESS_ITEMSUpToCurrentDate == null || currentPROGRESS_ITEM.PROGRESS_ITEMSUpToCurrentDate.Count == 0)
                    continue;

                DateTime firstEarnedDate = currentPROGRESS_ITEM.PROGRESS_ITEMSUpToCurrentDate.Min(x => x.EARNED_DATE);
                DateTime lastEarnedDate = currentPROGRESS_ITEM.PROGRESS_ITEMSUpToCurrentDate.Max(x => x.EARNED_DATE);
                decimal totalEarnedUnits = currentPROGRESS_ITEM.PROGRESS_ITEMSUpToCurrentDate.Sum(x => x.EARNED_UNITS);

                decimal baseline_itemEarnedPercentage = totalEarnedUnits / baseline_item.TOTAL_UNITS;
                if (baseline_item.BASELINE_ITEM_ASSIGNMENTS.Count == 0)
                    continue;

                //only process applicable assignments
                List<BASELINE_ITEM_ASSIGNMENT> baseline_itemAssignments = baseline_item.BASELINE_ITEM_ASSIGNMENTS.Where(assignment => assignment.LOW_VALUE <= baseline_itemEarnedPercentage).OrderBy(assignment => assignment.LOW_VALUE).ToList();
 
                for (int i = 0; i < baseline_itemAssignments.Count; i++)
                {
                    BASELINE_ITEM_ASSIGNMENT baseline_itemAssignment = baseline_itemAssignments[i];
                    TASK P6TASK = PROJECTTASK.FirstOrDefault(P6Task => P6Task.task_code == baseline_itemAssignment.P6_ACTIVITYID);
                    if (P6TASK != null && P6TASK.delete_date == null)
                    {
                        //set activity start date
                        DateTime firstEarnedWeekStartingDate = firstEarnedDate.AddDays(-1 * intervalTimeSpan.Days).AddSeconds(1);
                        if (P6TASK.act_start_date == null || P6TASK.act_start_date > firstEarnedWeekStartingDate)
                            P6TASK.act_start_date = firstEarnedWeekStartingDate;
                        
                        //current activity assignment value must be limited to total earned percentage
                        decimal highValueToUse = baseline_itemAssignment.HIGH_VALUE > baseline_itemEarnedPercentage ? baseline_itemEarnedPercentage : baseline_itemAssignment.HIGH_VALUE;

                        //current activity assignment unit
                        decimal currentAssignmentUnits = ((highValueToUse - baseline_itemAssignment.LOW_VALUE) + 0.01m) * baseline_item.TOTAL_UNITS;

                        //if this is the first time processing the task
                        //another way of doing this is to reset everything to zero and not started, but we do not want to override user changes on the p6 schedule
                        if (!processedP6Task.Any(x => x == P6TASK.task_code))
                        {
                            P6TASK.act_work_qty = currentAssignmentUnits;
                            processedP6Task.Add(P6TASK.task_code);
                        }
                        else
                            P6TASK.act_work_qty += currentAssignmentUnits;

                        if (P6TASK.target_work_qty <= 0)
                        {
                            errorMessage = "Current P6 activity doesn't have budgeted units, please re-populate budgeted units on baseline";
                            break;
                        }

                        if (P6TASK.remain_work_qty >= 0)
                            P6TASK.remain_work_qty = P6TASK.target_work_qty - P6TASK.act_work_qty;

                        if (P6TASK.remain_work_qty < 0)
                        {
                            errorMessage = "Negative remaining units because budgeted units is less than earned units, please re-populate budgeted units on baseline";
                            break;
                        }

                        P6TASK.remain_drtn_hr_cnt = P6TASK.target_drtn_hr_cnt * (P6TASK.remain_work_qty / P6TASK.target_work_qty);

                        if (P6TASK.remain_work_qty == 0)
                        {
                            P6TASK.status_code = P6TASKSTATUS.TK_Complete.ToString();
                            P6TASK.act_end_date = lastEarnedDate;
                        }
                        else if (P6TASK.remain_work_qty > 0)
                        {
                            P6TASK.status_code = P6TASKSTATUS.TK_Active.ToString();
                            P6TASK.act_end_date = null;
                        }
                        else if (P6TASK.status_code == P6TASKSTATUS.TK_NotStart.ToString())
                            P6TASK.status_code = P6TASKSTATUS.TK_Active.ToString();

                        P6TASKCollectionViewModel.Save(P6TASK);
                    }
                    else
                    {
                        errorMessage = "P6 activity named " + baseline_itemAssignment.P6_ACTIVITYID + " not found, please check deliverable's assignment";
                        break;
                    }
                }
            }

            LoadingScreenManager.CloseLoadingScreen();

            //Dispose viewmodel
            IDocumentContent documentContentViewModel = BASELINE_ITEMSchedulingViewModel as IDocumentContent;
            documentContentViewModel.OnDestroy();
            BASELINE_ITEMSchedulingViewModel = null;

            if (errorMessage == string.Empty)
                MessageBoxService.ShowMessage(BluePrintsResources.WORKPACK_ASSIGNMENT_P6ProgressWriteSuccess);
            else
                MessageBoxService.ShowMessage(errorMessage);

            isPushingToP6 = false;
        }

        private decimal cumulativePrincipalUnits = 0;
        private decimal cumulativeCurrentUnits = 0;

        public void CustomSummary(CustomSummaryEventArgs e)
        {
            if (e.IsTotalSummary || e.IsGroupSummary)
            {
                if (e.SummaryProcess == CustomSummaryProcess.Start)
                {
                    cumulativePrincipalUnits = 0;
                    cumulativeCurrentUnits = 0;
                }
                if (e.SummaryProcess == CustomSummaryProcess.Calculate)
                    if (((GridSummaryItem)e.Item).FieldName == "TOTAL_EARNED_PERCENTAGE")
                    {
                        var budgetedUnits =
                            ((PROGRESS_ITEMProjection)e.Row).Entity.Entity.TOTAL_HOURS;
                        var previousUnits =
                            ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMSBeforeReportingDate.Sum(x => x.EARNED_UNITS);
                        var currentUnits = ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent == null
                            ? 0
                            : ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent.EARNED_UNITS;

                        cumulativePrincipalUnits += budgetedUnits;
                        cumulativeCurrentUnits += currentUnits + previousUnits;
                        if (cumulativePrincipalUnits > 0)
                            e.TotalValue = cumulativeCurrentUnits / cumulativePrincipalUnits;
                    }
                    else if (((GridSummaryItem)e.Item).FieldName == "PERIOD_EARNED_PERCENTAGE")
                    {
                        var totalUnits =
                            ((PROGRESS_ITEMProjection)e.Row).Entity.Entity.TOTAL_HOURS;
                        var currentUnits = ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent == null
                            ? 0
                            : ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent.EARNED_UNITS;

                        cumulativePrincipalUnits += totalUnits;
                        cumulativeCurrentUnits += currentUnits;
                        if (cumulativePrincipalUnits > 0)
                            e.TotalValue = cumulativeCurrentUnits / cumulativePrincipalUnits;
                    }
                    else
                    {
                        e.TotalValue = 0;
                    }
            }
        }

        #endregion

        #region Reporting

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Progress_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public bool CanViewReport()
        {
            return true;
        }

        private ProjectSummaryStats GetProgressSummary()
        {
            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            List<VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONCollection.AsQueryable(), MainViewModel.Entities.Select(x => x.Entity));
            ProjectSummaryStats projectSummary = new ProjectSummaryStats(MainViewModel.Entities, loadPROGRESS, projectVariationAdjustment);
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT, loadBASELINE, loadPROGRESS, WORKPACKCollection, WORKPACKCollection.SelectMany(x => x.WORKPACK_ASSIGNMENT).ToList(), p6UOW);
            fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder);
            fullSummarizer.Build();
            return projectSummary;
        }

        public void ViewReport()
        {

            var progressReport = new XtraReportPROGRESS_ITEMS();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    progressReport.LoadLayout(sw.BaseStream);
                }
            }

            ProjectSummaryStats projectSummary = GetProgressSummary();
            progressReport.AssignProperties(projectSummary, loadPROGRESS.DATA_DATE, loadPROGRESS.PROJECT.NAME);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = progressReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            progressReport.RequestParameters = false;
            progressReport.CreateDocument(true);
            previewWindow.Show();
        }

        #endregion

        #region Disposing
        protected override void OnClose(CancelEventArgs e)
        {
            CancelBackgroundWorker();
            base.OnClose(e);
        }

        private void CancelBackgroundWorker()
        {
            if (calculatePlannedBackgroundWorker != null)
                calculatePlannedBackgroundWorker.CancelAsync();
        }
        #endregion
    }
}