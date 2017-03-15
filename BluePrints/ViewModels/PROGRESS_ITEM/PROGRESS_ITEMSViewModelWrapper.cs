using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using BluePrints.Data.Helpers;
using DevExpress.Xpf.Grid;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.ViewModel.Reporting;
using System.Windows.Threading;
using BluePrints.Views;
using BluePrints.Reports;
using System.IO;
using DevExpress.Xpf.Printing;
using System.Windows;
using System.Threading.Tasks;
using BluePrints.Common.Projections;
using System.ComponentModel;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Data;
using DevExpress.Xpf.Editors;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class PROGRESS_ITEMSViewModelWrapper :
        CollectionViewModelsWrapper
        <BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROGRESS_ITEMSViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROGRESS_ITEMSViewModelWrapper());
        }

        #region Database Operation

        private Data.PROJECT loadPROJECT;
        private PROGRESS loadPROGRESS;
        private BASELINE loadBASELINE;
        private bool isQueryForLiveStatus;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 10);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            var receiveParameter =
                (OptionalEntitiesParameter<Data.PROJECT, PROGRESS>) parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadPROGRESS = receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => loadBASELINE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, SetPROGRESStoCurrentDateOnLoaded);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, false);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        private void SetPROGRESStoCurrentDateOnLoaded(PROGRESS entity)
        {
            loadPROGRESS = entity;
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
            return query => query.Where(x => x.APPROVED != null && x.GUID_PROJECT == loadPROJECT.GUID);
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

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
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
            return
                query =>
                    PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(
                        query.OrderBy(x => x.INTERNAL_NUM), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc,
                        getRATESFunc, getDELIVERABLES_STATUSESFunc);
        }

        private PROJECTSummary currentPROJECTSummary;

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS_ITEMProjection> entities)
        {
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = ApplyProjectionPropertiesToEntityCallBack;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingRowAddUndoAndSaveCallBack;
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterBASELINE_ITEMEntitySaved;
            //have to add this so that undo will have an effect on progress item
            //MainViewModel.BulkPreSave = this.MainEntityBulkPreSave;
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.BeforeShownEditor = BeforeShownEditor;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => InitializePROJECTSummary(entities)));
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void InitializePROJECTSummary(IEnumerable<PROGRESS_ITEMProjection> entities)
        {
            currentPROJECTSummary = PROJECTSummary.Create();
            currentPROJECTSummary.LiveBASELINE = loadBASELINE;
            currentPROJECTSummary.LivePROGRESS = loadPROGRESS;
            currentPROJECTSummary.VARIATIONS = loaderCollection.GetCollection<VARIATION>();
            currentPROJECTSummary.ReportingDataDate = loadPROGRESS.DATA_DATE;
            currentPROJECTSummary.RATES = loaderCollection.GetCollection<RATE>();
            currentPROJECTSummary.ReportableObjects = entities;

            var projectSummaryBuilder = new PROJECTSummaryBuilder(currentPROJECTSummary);
            var summaryBackgroundWorker = new BackgroundWorker();
            summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            summaryBackgroundWorker.WorkerSupportsCancellation = true;
            summaryBackgroundWorker.RunWorkerAsync(projectSummaryBuilder);
        }

        private void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var summaryBuilder = (PROJECTSummaryBuilder) e.Argument;
            CalculateMinimalStats(summaryBuilder);
            CalculateStatsForReport(summaryBuilder);

            if (((BackgroundWorker) sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        private void CalculateMinimalStats(PROJECTSummaryBuilder summaryBuilder)
        {
            var summaryManufacturer =
                new BuildMinimalStatsForPlannedOriginalPercentage();
            summaryManufacturer.Manufacture(summaryBuilder);

            RefreshView();
            //mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        private bool isReportReady;

        private void CalculateStatsForReport(PROJECTSummaryBuilder summaryBuilder)
        {
            var summaryManufacturer =
                new BuildFullStatsIncludingPROGRESS_ITEMSummary();
            summaryManufacturer.Manufacture(summaryBuilder);
            isReportReady = true;
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if(changedType == typeof(PROGRESS_ITEM))
            {
                PROGRESS_ITEMProjection mainEntity = MainViewModel.Entities.Where(x => x.PROGRESS_ITEMCurrent != null).FirstOrDefault(x => x.PROGRESS_ITEMCurrent.GUID.ToString() == key.ToString());
                if (mainEntity != null)
                {
                    //got to make sure sender is not MainViewModel or else it'll not be refreshed
                    mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(mainEntity.GUID, EntityMessageType.Changed, this))));
                    return true;
                }
            }

            return false;
        }

        //protected override void OnAfterCompulsoryEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
        //    object sender)
        //{
        //    //Map the changes from PROGRESS_ITEM to BASELINE_ITEM so undo/redo operation is valid
        //    if ((sender != null && PROGRESS_ITEMSCollectionViewModel != null) && changedType == typeof(PROGRESS_ITEM))
        //    {
        //        PROGRESS_ITEMProjection mappedEntity = MainViewModel.Entities.FirstOrDefault(x => x.PROGRESS_ITEMCurrent != null && x.PROGRESS_ITEMCurrent.GUID.ToString() == key.ToString());
        //        if (mappedEntity != null)
        //            mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(mappedEntity.GUID, EntityMessageType.Changed, this))));

        //        if (MainViewModel != null)
        //            mainThreadDispatcher.BeginInvoke(new Action(() => this.InitializePROJECTSummary(MainViewModel.Entities)));

        //        return;
        //    }

        //    base.OnAfterCompulsoryEntitiesChanged(key, changedType, messageType, sender);
        //}
        #region Collection Call Backs

        private bool ExistingRowAddUndoAndSaveCallBack(PROGRESS_ITEMProjection projectionEntity, CellValueChangedEventArgs e)
        {
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
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().BASELINE_ITEMJoinRATE) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_STATUS)
                ||
                e.Column.FieldName ==
                BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().BASELINE_ITEMJoinRATE) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().BASELINE_ITEM) + "." +
                BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_USER))
            {
                return true;
            }

            return false;
        }

        private void ApplyProjectionPropertiesToEntityCallBack(PROGRESS_ITEMProjection projectionEntity, BASELINE_ITEM entity)
        {
            entity = projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM;
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
                        x.GUID_ORIBASEITEM == projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL &&
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
            savePROGRESS_ITEM.GUID_ORIBASEITEM = projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL;
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

            var newPercentage = (decimal) fillValue;
            if (newPercentage > fillDownEntity.MaxPercentage)
                return false;
            else if (newPercentage < fillDownEntity.MinPercentage)
                return false;

            return true;
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
            if (MainViewModel == null || MainViewModel.IsLoading)
                return false;

            if (loadPROGRESS.DATA_DATE > loadPROGRESS.PROGRESS_START)
                return true;

            return false;
        }

        public bool CanDateForward()
        {
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
            this.RaisePropertyChanged(x => x.DataDate);
        }

        private void DateChange(DateNavigationType navigationType)
        {
            var interval = ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(loadPROGRESS);
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

        private WORKPACKSchedulingViewModelWrapper WORKPACK_DashboardViewModel;

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
            if (loadPROGRESS == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
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

        private void PushToP6(BaselineMappingSelectionType mappingSelectionType)
        {
            WORKPACK_DashboardViewModel = WORKPACKSchedulingViewModelWrapper.Create();
            WORKPACK_DashboardViewModel.OnPROJECTWORKPACKSMappingViewModelLoaded =
                OnPROJECTWORKPACKSMappingViewModelLoaded;
            var ParameterObj = WORKPACK_DashboardViewModel as ISupportParameter;
            ParameterObj.Parameter = new object[] { loadPROGRESS, mappingSelectionType };
        }

        private void OnPROJECTWORKPACKSMappingViewModelLoaded(IEnumerable<WORKPACK_Dashboard> entities)
        {
            IEnumerable<TASK> PROJECTTASK = WORKPACK_DashboardViewModel.P6TASKCollection;
            IEnumerable<ProgressInfo> cumulativeEarnedDataPoints = entities.Where(x => x.Summary_CumulativeEarnedDataPoints != null).SelectMany(x => x.Summary_CumulativeEarnedDataPoints);
            cumulativeEarnedDataPoints = cumulativeEarnedDataPoints.OrderBy(x => x.ProgressDate).ToList();
            TimeSpan intervalTimeSpan = ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(loadPROGRESS);
            ICollectionViewModel<TASK> P6TASKCollectionViewModel = WORKPACK_DashboardViewModel.P6TASKCollectionViewModel;

            foreach (WORKPACK_Dashboard workpack in entities)
            {
                ProgressInfo fWorkpackEarnedDataPoint = cumulativeEarnedDataPoints.FirstOrDefault(dataPoint => dataPoint.WorkpackGuid == workpack.WORKPACK.GUID && dataPoint.Units > 0);
                if (fWorkpackEarnedDataPoint != null)
                {
                    ProgressInfo lWorkpackEarnedDataPoint = cumulativeEarnedDataPoints.LastOrDefault(dataPoint => dataPoint.WorkpackGuid == workpack.WORKPACK.GUID && dataPoint.ProgressDate <= loadPROGRESS.DATA_DATE);
                    List<WORKPACK_ASSIGNMENT> workpackAssignments = workpack.WORKPACK.WORKPACK_ASSIGNMENT.Where(assignment => assignment.LOW_VALUE <= lWorkpackEarnedDataPoint.Units).OrderBy(assignment => assignment.LOW_VALUE).ToList();
                    for (int i = 0; i < workpackAssignments.Count; i++)
                    {
                        WORKPACK_ASSIGNMENT workpackAssignment = workpackAssignments[i];
                        TASK P6TASK = PROJECTTASK.FirstOrDefault(P6Task => P6Task.task_code == workpackAssignment.P6_ACTIVITYID);
                        if (P6TASK != null)
                        {
                            DateTime proposedStartDate = fWorkpackEarnedDataPoint.ProgressDate.AddDays(-1 * intervalTimeSpan.Days).AddSeconds(1);
                            if (P6TASK.act_start_date == null || P6TASK.act_start_date > proposedStartDate)
                                P6TASK.act_start_date = proposedStartDate;

                            decimal actUnits = lWorkpackEarnedDataPoint.Units < workpackAssignment.HIGH_VALUE ? lWorkpackEarnedDataPoint.Units : workpackAssignment.HIGH_VALUE;
                            decimal actWorkUnitNormalize = i == 0 ? actUnits : (actUnits - workpackAssignments[i - 1].HIGH_VALUE);
                            P6TASK.act_work_qty = actWorkUnitNormalize;
                            if (P6TASK.remain_work_qty >= 0)
                                P6TASK.remain_work_qty = P6TASK.target_work_qty - P6TASK.act_work_qty;
                            P6TASK.remain_drtn_hr_cnt = P6TASK.target_drtn_hr_cnt * (P6TASK.remain_work_qty / P6TASK.target_work_qty);

                            if (P6TASK.remain_work_qty == 0)
                            {
                                P6TASK.status_code = P6TASKSTATUS.TK_Complete.ToString();
                                P6TASK.act_end_date = lWorkpackEarnedDataPoint.ProgressDate;
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
                    }
                }
            }
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
                    if (((GridSummaryItem) e.Item).FieldName == "TOTAL_EARNED_PERCENTAGE")
                    {
                        var budgetedUnits =
                            ((PROGRESS_ITEMProjection) e.Row).BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                        var previousUnits =
                            ((PROGRESS_ITEMProjection) e.Row).PROGRESS_ITEMSBeforeReportingDate.Sum(x => x.EARNED_UNITS);
                        var currentUnits = ((PROGRESS_ITEMProjection) e.Row).PROGRESS_ITEMCurrent == null
                            ? 0
                            : ((PROGRESS_ITEMProjection) e.Row).PROGRESS_ITEMCurrent.EARNED_UNITS;

                        cumulativePrincipalUnits += budgetedUnits;
                        cumulativeCurrentUnits += currentUnits + previousUnits;
                        if (cumulativePrincipalUnits > 0)
                            e.TotalValue = cumulativeCurrentUnits / cumulativePrincipalUnits;
                    }
                    else if (((GridSummaryItem) e.Item).FieldName == "PERIOD_EARNED_PERCENTAGE")
                    {
                        var totalUnits =
                            ((PROGRESS_ITEMProjection) e.Row).BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                        var currentUnits = ((PROGRESS_ITEMProjection) e.Row).PROGRESS_ITEMCurrent == null
                            ? 0
                            : ((PROGRESS_ITEMProjection) e.Row).PROGRESS_ITEMCurrent.EARNED_UNITS;

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
            var reportDesigner = new REPORTDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Progress_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public bool CanViewReport()
        {
            return isReportReady;
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

            var projectSummaryBuilder = new PROJECTSummaryBuilder(currentPROJECTSummary);
            //CalculateStatsForReport(projectSummaryBuilder);
            progressReport.AssignProperties(currentPROJECTSummary, loadPROGRESS.PROJECT.NAME);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = progressReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            progressReport.RequestParameters = false;
            progressReport.CreateDocument(true);
            previewWindow.ShowDialog();
        }

        #endregion
    }
}