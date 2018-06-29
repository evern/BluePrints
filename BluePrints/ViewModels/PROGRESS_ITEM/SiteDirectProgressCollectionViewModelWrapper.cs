using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using BaseModel.Data.Helpers;
using BluePrints.Common.Resources;

namespace BluePrints.ViewModels
{
    public class SiteDirectProgressCollectionViewModelWrapper :
        BluePrintsEntitiesProgressCollectionWrapper
        <ESTIMATE_ITEM, ReportablesDisplay, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of SiteDirectProgressCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SiteDirectProgressCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new SiteDirectProgressCollectionViewModelWrapper());
        }

        #region Database Operations
        private ESTIMATE loadESTIMATE;
        protected override void resolveParameters(object parameter)
        {
            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 10);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            var receiveParameter =
                (DualEntitiesParameter<Data.PROJECT, PROGRESS>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadPROGRESS = receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc, x => assign_estimation_direct(x));
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_GROUPS, STOCK_GROUPProjectionFunc);
            base.initializeEntitiesLoadersDescription();
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID_PROJECT).OrderBy(x => x.NUMBER);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private void assign_estimation_direct(ESTIMATE estimation_direct)
        {
            if (estimation_direct == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live estimate not found")));

            loadESTIMATE = estimation_direct;
        }

        private Func<IRepositoryQuery<STOCK_GROUP>, IQueryable<STOCK_GROUP>> STOCK_GROUPProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ReportablesDisplay>> specifyMainViewModelProjection()
        {
            return query => ProgressQueries.SiteDirectProgressItemTransformation(query.Where(x => x.GUID_ESTIMATE == loadESTIMATE.GUID && x.GUID_PHASE != null), loadPROJECT, loadPROGRESS, PROGRESS_ITEMCollection, STOCK_GROUPCollection, STOCK_CODECollection, RATECollection, false);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ReportablesDisplay> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Behavior
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if(changedType == typeof(PROGRESS_ITEM))
            {
                PROGRESS_ITEM newPROGRESSITEM = PROGRESS_ITEMCollection.FirstOrDefault(x => x.GUID == (Guid)key);
                if(newPROGRESSITEM != null)
                {
                    ReportablesDisplay affectedDisplayEntity = getAffectedDisplayEntity(newPROGRESSITEM);
                    if (affectedDisplayEntity != null)
                        affectedDisplayEntity.Update();
                }

                return true;
            }

            return false;
        }

        public override void FullRefresh()
        {
            ReloadEntitiesCollection();
        }

        protected override void BackgroundRefresh()
        {
            if (MainViewModel == null)
                return;

            foreach(ReportablesDisplay reportable in MainViewModel.Entities)
            {
                reportable.Update();
            }

            base.BackgroundRefresh();
        }

        private ReportablesDisplay getAffectedDisplayEntity(PROGRESS_ITEM newPROGRESS_ITEM)
        {
            foreach (ReportablesDisplay entity in MainViewModel.Entities)
            {
                if(entity.Reportables != null)
                {
                    foreach(DisplayQuantityReportable reportable in entity.Reportables)
                    {
                        if (reportable.OriginalEntityKey == newPROGRESS_ITEM.GUID_ORIBASEITEM)
                        {
                            setReportableNewProgress(reportable, newPROGRESS_ITEM);
                            return entity;
                        }
                    }
                }
                else
                {
                    if (entity.ProgressItem.OriginalEntityKey == newPROGRESS_ITEM.GUID_ORIBASEITEM)
                    {
                        setReportableNewProgress(entity.ProgressItem, newPROGRESS_ITEM);
                        return entity;
                    }
                }
            }

            return null;
        }

        private void setReportableNewProgress(IDeliverable updateEntity, PROGRESS_ITEM newPROGRESS_ITEM)
        {
            IReportable reportableProjection = updateEntity as IReportable;
            ICanSetProgresses setProgressEntity = updateEntity as ICanSetProgresses;
            if(reportableProjection != null && setProgressEntity != null)
            {
                if (reportableProjection.PROGRESS_ITEM_Current == null)
                {
                    setProgressEntity.AppendProgressItem(newPROGRESS_ITEM);
                }
            }
        }

        public override ObservableCollection<ReportablesDisplay> DisplayEntities => base.DisplayEntities;

        /// <summary>
        /// Intercept MainViewModel Saving because bulk or single selective saving is required
        /// </summary>
        public bool OnBeforeEntitySaved(ReportablesDisplay entity)
        {
            bool is_group = save_reportables_display(entity);
            //save progress is only used for saving standalone or group
            if (entity.ProgressItem.Progress_Type == EstimateProgressType.Standalone || is_group)
            {
                save_progress(entity);
                //update must be here or else installed quantity will be cleared and progress will be saved with 0 units
                entity.Update();
            }

            //only DisplayQuantityReportable is allowed to be saved
            return !is_group;
        }

        public bool CanAutoProgressIndirects()
        {
            return !IsLoading && loadPROGRESS != null;
        }

        public void AutoProgressIndirects()
        {
            if (MessageBoxService.ShowMessage("Warning\nThis action will update or delete progresses based on baseline start and finish dates and is not reversible\nDo you wish to continue?",
                         BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            IEnumerable<ReportablesDisplay> deliverables = MainViewModel.Entities;
            List<PROGRESS_ITEM> updateProgress = new List<PROGRESS_ITEM>();
            
            foreach (var deliverable in deliverables.Where(x => x.Charge == ChargeType.Indirect || x.IsByDuration))
            {
                if(deliverable.Stats.Budgeted.CumulativeDataPoints != null)
                {
                    DateTime firstProgressDate = deliverable.Stats.Budgeted.CumulativeDataPoints.Min(x => x.ProgressDate);
                    DateTime lastProgressDate = deliverable.Stats.Budgeted.CumulativeDataPoints.Max(x => x.ProgressDate);

                    if (lastProgressDate > firstProgressDate && loadPROGRESS.DATA_DATE > firstProgressDate)
                    {
                        double elapsedDays = (loadPROGRESS.DATA_DATE - firstProgressDate).TotalDays;
                        double totalDays = (lastProgressDate - firstProgressDate).TotalDays;

                        if (totalDays > 0)
                        {
                            decimal autoPercent = Convert.ToDecimal(elapsedDays / totalDays);
                            if (deliverable.Total_Earned_Percentage < autoPercent)
                            {
                                decimal oldPercentage = deliverable.Total_Earned_Percentage;
                                decimal newPercentage = autoPercent;

                                decimal totalQuantity = deliverable.ProgressItem.Total_Quantity;
                                decimal currentPeriodInstalledQuantity = totalQuantity * newPercentage;

                                deliverable.ProgressItem.CurrentPeriodInstalledQuantity = currentPeriodInstalledQuantity;
                                IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = deliverable.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                                updateProgress.AddRange(newPRORESS_ITEMS);
                            }
                            else if (deliverable.Total_Earned_Percentage > autoPercent)
                            {
                                decimal totalDeliverableUnits = deliverable.Total_Units;
                                decimal maxAllowableEarnedUnit = totalDeliverableUnits * autoPercent;
                                if (maxAllowableEarnedUnit > 0)
                                {
                                    decimal iterateEarnedUnits = 0;
                                    List<PROGRESS_ITEM> progressesByDate = deliverable.PROGRESS_ITEMS.OrderBy(x => x.EARNED_DATE).ToList();
                                    foreach (PROGRESS_ITEM progressByDate in progressesByDate)
                                    {
                                        decimal postProgressEarnedUnit = (iterateEarnedUnits + progressByDate.EARNED_UNITS);
                                        decimal oldProgressEarnUnit = progressByDate.EARNED_UNITS;
                                        if (postProgressEarnedUnit > maxAllowableEarnedUnit)
                                        {
                                            decimal newProgressEarnUnit = (maxAllowableEarnedUnit - iterateEarnedUnits);
                                            progressByDate.EARNED_UNITS = newProgressEarnUnit < 0 ? 0 : newProgressEarnUnit;
                                            updateProgress.Add(progressByDate);
                                        }

                                        iterateEarnedUnits += oldProgressEarnUnit;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            PROGRESS_ITEMSCollectionViewModel.BulkSave(updateProgress);
            FullRefresh();
        }

        private bool save_reportables_display(ReportablesDisplay entity)
        {
            IReportable_Group reportable_Group = entity.ProgressItem as IReportable_Group;
            if (reportable_Group != null)
            {
                foreach(IReportable reportable in reportable_Group.Reportables)
                {
                    MainViewModel.Save(new ReportablesDisplay() { ProgressItem = (DisplayQuantityReportable)reportable });
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remembers an entity property old value for undoing
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the AddUndoCommand property that can be used as a binding source in views.
        /// </summary>
        public void ExistingChildRowAddUndoAndSave(CellValueChangedEventArgs e)
        {
            var projection = (DisplayQuantityReportable)e.Row;
            if (e.RowHandle != DataControlBase.NewItemRowHandle)
            {
                IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = projection.Reportable.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                PROGRESS_ITEMSCollectionViewModel.BulkSave(newPRORESS_ITEMS);
            }
        }

        private void save_progress(ReportablesDisplay entity)
        {
            if (entity.ProgressItem.ShouldSaveProgress)
            {
                IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = entity.ProgressItem.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                PROGRESS_ITEMSCollectionViewModel.BulkSave(newPRORESS_ITEMS);
            }
        }

        private PROGRESS_ITEM createNewPROGRESS_ITEM(Guid originalEntityKey)
        {
            PROGRESS_ITEM savePROGRESS_ITEM = new PROGRESS_ITEM();
            savePROGRESS_ITEM.GUID_ORIBASEITEM = originalEntityKey;
            savePROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
            savePROGRESS_ITEM.EARNED_DATE = loadPROGRESS.DATA_DATE;
            savePROGRESS_ITEM.CREATED = DateTime.Now;

            return savePROGRESS_ITEM;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "SiteDirectProgressCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "SiteDirectProgressCollectionViewModelWrapper_v1" + view_project_specific_affix; }
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

        public IEnumerable<STOCK_GROUP> STOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> STOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
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

        protected override CostGroup cost_group => CostGroup.Site;

        protected override IEnumerable<IReportable> ReportableCollection => MainViewModel == null || MainViewModel.Entities == null ? new ObservableCollection<ESTIMATE_ITEMProgress>() : MainViewModel.Entities.Select(x => x.ProgressItem.Reportable);

        private BUDGET_ITEMSchedulingViewModelWrapper estimation_direct_item_scheduling_view_model;
        protected override IEntitiesSchedulingCollectionWrapper scheduling_view_model
        {
            get
            {
                if (estimation_direct_item_scheduling_view_model == null)
                    estimation_direct_item_scheduling_view_model = BUDGET_ITEMSchedulingViewModelWrapper.Create();

                return estimation_direct_item_scheduling_view_model;
            }
        }

        protected override void dispose_scheduling_view_model()
        {
            estimation_direct_item_scheduling_view_model = null;
        }

        protected override PhaseType progress_type => PhaseType.Construct;

        protected override bool have_group_entity => true;
        #endregion

        #region Disposing
        private void CancelBackgroundWorker()
        {
            if (calculatePlannedBackgroundWorker != null)
                calculatePlannedBackgroundWorker.CancelAsync();
        }
        #endregion
    }
}