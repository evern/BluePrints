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

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class PROGRESS_ITEMSViewModelWrapper : CollectionViewModelsWrapper<BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<BASELINE_ITEM, PROGRESS_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
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
        BluePrints.Data.PROJECT loadPROJECT;
        PROGRESS loadPROGRESS;
        BASELINE loadBASELINE;
        bool isQueryForLiveStatus;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 10);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            OptionalEntitiesParameter<BluePrints.Data.PROJECT, PROGRESS> receiveParameter = (OptionalEntitiesParameter<BluePrints.Data.PROJECT, PROGRESS>)parameter;
            this.loadPROJECT = receiveParameter.GetFirstEntity();
            this.loadPROGRESS = receiveParameter.GetSecondEntity();

            if (this.loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<BluePrints.Data.PROJECT, BluePrints.Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, typeof(BluePrints.Data.PROJECT), isContinueLoadingAfterBASELINE, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, typeof(BASELINE), isContinueLoadingAfterPROGRESS, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, typeof(BASELINE));
            loaderCollection.AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, typeof(PROGRESS), null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, typeof(BluePrints.Data.PROJECT));
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(6, bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(7, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(8, bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(9, bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(BluePrints.Data.PROJECT), null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork>(10, bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc, typeof(BluePrints.Data.PROJECT), null, OnEntitiesChanged);
            
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<BluePrints.Data.PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "BASELINE"))));
                return false;
            }

            this.loadBASELINE = entities.First();
            return true;
        }

        bool isContinueLoadingAfterPROGRESS(IEnumerable<PROGRESS> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROGRESS"))));
                return false;
            }

            this.loadPROGRESS = entities.First();
            mainThreadDispatcher.BeginInvoke(new Action(() => DateChange(DateNavigationType.Current)));

            return true;
        }

        Func<IRepositoryQuery<BluePrints.Data.PROJECT>, IQueryable<BluePrints.Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == this.loadPROGRESS.GUID_PROJECT).OrderBy(x => x.NUMBER);
        }

        Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.APPROVED != null && x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
            else
                return query => query.Where(x => x.GUID == this.loadPROGRESS.GUID);
        }

        Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Progress_Report.ToString());
        }

        Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<PROGRESS_ITEMProjection>> ConstructMainViewModelProjection()
        {
            Func<BASELINE> getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            Func<PROGRESS> getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            Func<IQueryable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            Func<IQueryable<RATE>> getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();

            return query => PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(query, getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc);
        }

        PROJECTSummary currentPROJECTSummary;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS_ITEMProjection> entities)
        {
            MainViewModel.PreSave = this.MainEntityPreSave;
            //MainViewModel.BulkPreSave = this.MainEntityBulkPreSave;
            MainViewModel.ValidateFillDownCallBack = this.ValidateFillDownCallBack;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.InitializePROJECTSummary(entities)));
        }

        void InitializePROJECTSummary(IEnumerable<PROGRESS_ITEMProjection> entities)
        {
            currentPROJECTSummary = PROJECTSummary.Create();
            currentPROJECTSummary.LiveBASELINE = loadBASELINE;
            currentPROJECTSummary.LivePROGRESS = loadPROGRESS;
            currentPROJECTSummary.VARIATIONS = loaderCollection.GetCollection<VARIATION>();
            currentPROJECTSummary.ReportingDataDate = loadPROGRESS.DATA_DATE;
            currentPROJECTSummary.RATES = loaderCollection.GetCollection<RATE>();
            currentPROJECTSummary.ReportableObjects = entities;

            PROJECTSummaryBuilder projectSummaryBuilder = new PROJECTSummaryBuilder(currentPROJECTSummary);
            BackgroundWorker summaryBackgroundWorker = new BackgroundWorker();
            summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
            summaryBackgroundWorker.WorkerSupportsCancellation = true;
            summaryBackgroundWorker.RunWorkerAsync(projectSummaryBuilder);
        }

        void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PROJECTSummaryBuilder summaryBuilder = (PROJECTSummaryBuilder)e.Argument;
            CalculateMinimalStats(summaryBuilder);
            CalculateStatsForReport(summaryBuilder);

            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        void CalculateMinimalStats(PROJECTSummaryBuilder summaryBuilder)
        {
            BuildMinimalStatsForPlannedOriginalPercentage summaryManufacturer = new BuildMinimalStatsForPlannedOriginalPercentage();
            summaryManufacturer.Manufacture(summaryBuilder);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        bool isReportReady;
        void CalculateStatsForReport(PROJECTSummaryBuilder summaryBuilder)
        {
            BuildFullStatsIncludingPROGRESS_ITEMSummary summaryManufacturer = new BuildFullStatsIncludingPROGRESS_ITEMSummary();
            summaryManufacturer.Manufacture(summaryBuilder);
            isReportReady = true;
        }

        protected override void OnEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            //Map the changes from PROGRESS_ITEM to BASELINE_ITEM so undo/redo operation is valid
            if (changedType == typeof(PROGRESS_ITEM))
            {
                PROGRESS_ITEMProjection mappedEntity = MainViewModel.Entities.FirstOrDefault(x => x.PROGRESS_ITEMCurrent != null && x.PROGRESS_ITEMCurrent.GUID.ToString() == key.ToString());
                if(mappedEntity != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(mappedEntity.GUID, EntityMessageType.Changed, this))));
                return;
            }

            if (sender == this)
                return;

            if (loadPROGRESS != null && changedType == typeof(PROGRESS) && loadPROGRESS.GUID.ToString() == key.ToString() ||
                loadBASELINE != null && changedType == typeof(BASELINE) && loadBASELINE.GUID.ToString() == key.ToString() ||
                loadPROJECT != null && changedType == typeof(BluePrints.Data.PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null || loadBASELINE != null || loadPROGRESS != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null || loadBASELINE != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }
        }

        #region Collection Call Backs
        //private bool MainEntityBulkPreSave(IEnumerable<PROGRESS_ITEMProjection> entities)
        //{
        //    foreach (var entity in entities)
        //    {
        //        MainEntityPreSave(entity);
        //    }

        //    return false;
        //}

        bool MainEntityPreSave(PROGRESS_ITEMProjection projectionEntity)
        {
            CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESS_ITEMSCollectionViewModel = (CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS_ITEM>();
            PROGRESS_ITEM savePROGRESS_ITEM = projectionEntity.PROGRESS_ITEMCurrent;
            savePROGRESS_ITEM.EARNED_DATE = loadPROGRESS.DATA_DATE;
            savePROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
            savePROGRESS_ITEM.GUID_ORIBASEITEM = projectionEntity.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL;
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (savePROGRESS_ITEM.CREATED.Date.Year == 1)
                savePROGRESS_ITEM.CREATED = DateTime.Now;

            savePROGRESS_ITEM = projectionEntity.PROGRESS_ITEMCurrent;
            PROGRESS_ITEMSCollectionViewModel.Save(savePROGRESS_ITEM);
            return false;
        }

        public bool ValidateFillDownCallBack(PROGRESS_ITEMProjection fillDownEntity, string fieldName, object fillValue)
        {
            if (fieldName != BindableBase.GetPropertyName(() => new PROGRESS_ITEMProjection().TOTAL_EARNED_PERCENTAGE))
                return false;

            decimal newPercentage = (decimal)fillValue;
            if (newPercentage > fillDownEntity.MaxPercentage)
                return false;
            else if (newPercentage < fillDownEntity.MinPercentage)
                return false;

            return true;
        }
        #endregion
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "PROGRESS_ITEMSViewModelWrapper";
            }
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

        DispatcherTimer delayedPROGRESSSavingDispatcher;

        void delayedPROGRESSSavingDispatcher_Tick(object sender, EventArgs e)
        {
            delayedPROGRESSSavingDispatcher.Stop();
            CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESSCollectionViewModel = (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS>();
            PROGRESSCollectionViewModel.Save(loadPROGRESS);
            this.RaisePropertyChanged(x => x.DataDate);
        }

        private void DateChange(DateNavigationType navigationType)
        {
            var interval = ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(loadPROGRESS);
            int multiplier;
            if (navigationType == DateNavigationType.Current)
            {
                TimeSpan timeDifferenceFromCurrent = loadPROGRESS.DATA_DATE - DateTime.Now;

                if (timeDifferenceFromCurrent.TotalSeconds > interval.TotalSeconds)
                {
                    do
                    {
                        loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(-1 * interval.Days);
                    } while (loadPROGRESS.DATA_DATE > DateTime.Now);
                }
                else
                {
                    if (timeDifferenceFromCurrent.TotalSeconds < (-1 * interval.TotalSeconds))
                    {
                        do
                        {
                            loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(1 * interval.Days);
                        } while (loadPROGRESS.DATA_DATE < (DateTime.Now - interval));
                    }
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
            //CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESSCollectionViewModel = (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS>();
            //PROGRESSCollectionViewModel.Save(loadPROGRESS);
            //this.RaisePropertyChanged(x => x.DataDate);
        }

        PROJECTWORKPACKSMappingViewModelWrapper WORKPACK_DashboardViewModel;
        public bool CanPushToP6()
        {
            if (loadPROGRESS == null || loadPROGRESS.P6PROGRESS_NAME == string.Empty)
                return false;

            return true;
        }

        public void PushToP6()
        {
            if (WORKPACK_DashboardViewModel == null)
            {
                WORKPACK_DashboardViewModel = PROJECTWORKPACKSMappingViewModelWrapper.Create();
                WORKPACK_DashboardViewModel.OnPROJECTWORKPACKSMappingViewModelLoaded = this.OnPROJECTWORKPACKSMappingViewModelLoaded;
                ISupportParameter ParameterObj = WORKPACK_DashboardViewModel as ISupportParameter;
                ParameterObj.Parameter = new object[] { this.loadPROGRESS, null };
            }
            else
                WORKPACK_DashboardViewModel.MainViewModel.Refresh();
        }

        void OnPROJECTWORKPACKSMappingViewModelLoaded(IEnumerable<WORKPACK_Dashboard> entities)
        {
            IEnumerable<TASK> PROJECTTASK = WORKPACK_DashboardViewModel.P6TASKCollection;
            IEnumerable<ProgressInfo> cumulativeEarnedDataPoints = WORKPACK_DashboardViewModel.MainViewModel.Entities.Where(x => x.Summary_CumulativeEarnedDataPoints != null).SelectMany(x => x.Summary_CumulativeEarnedDataPoints);
            cumulativeEarnedDataPoints = cumulativeEarnedDataPoints.OrderBy(x => x.ProgressDate).ToList();
            TimeSpan intervalTimeSpan = ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(loadPROGRESS);
            CollectionViewModel<TASK, int, IP6EntitiesUnitOfWork> P6TASKCollectionViewModel = WORKPACK_DashboardViewModel.P6TASKCollectionViewModel;

            foreach(WORKPACK_Dashboard workpack in WORKPACK_DashboardViewModel.MainViewModel.Entities)
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
                {
                    if (((GridSummaryItem)e.Item).FieldName == "TOTAL_EARNED_PERCENTAGE")
                    {
                        decimal budgetedUnits = ((PROGRESS_ITEMProjection)e.Row).BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                        decimal previousUnits = ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMSBeforeReportingDate.Sum(x => x.EARNED_UNITS);
                        decimal currentUnits = ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent == null ? 0 : ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent.EARNED_UNITS;

                        cumulativePrincipalUnits += budgetedUnits;
                        cumulativeCurrentUnits += (currentUnits + previousUnits);
                        if (cumulativePrincipalUnits > 0)
                            e.TotalValue = (cumulativeCurrentUnits / cumulativePrincipalUnits);
                    }
                    else if (((GridSummaryItem)e.Item).FieldName == "PERIOD_EARNED_PERCENTAGE")
                    {
                        decimal totalUnits = ((PROGRESS_ITEMProjection)e.Row).BASELINE_ITEMJoinRATE.BASELINE_ITEM.TOTAL_HOURS;
                        decimal currentUnits = ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent == null ? 0 : ((PROGRESS_ITEMProjection)e.Row).PROGRESS_ITEMCurrent.EARNED_UNITS;

                        cumulativePrincipalUnits += totalUnits;
                        cumulativeCurrentUnits += currentUnits;
                        if (cumulativePrincipalUnits > 0)
                            e.TotalValue = (cumulativeCurrentUnits / cumulativePrincipalUnits);
                    }
                    else
                        e.TotalValue = 0;
                }
            }
        }
        #endregion

        #region Reporting
        public void EditReport()
        {
            REPORTDesigner reportDesigner = new REPORTDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Progress_Report);
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
            XtraReportPROGRESS_ITEMS progressReport = new XtraReportPROGRESS_ITEMS();
            PROJECT_REPORT dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                string reportString = dbProjectReport.REPORT.ToString();
                using (StreamWriter sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    progressReport.LoadLayout(sw.BaseStream);
                }
            }

            PROJECTSummaryBuilder projectSummaryBuilder = new PROJECTSummaryBuilder(currentPROJECTSummary);
            //CalculateStatsForReport(projectSummaryBuilder);
            progressReport.AssignProperties(currentPROJECTSummary, loadPROGRESS.PROJECT.NAME);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
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
