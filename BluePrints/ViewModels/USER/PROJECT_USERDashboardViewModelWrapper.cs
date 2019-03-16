using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECT_USERDashboardViewModelWrapper :
        DashboardViewModelWrapper<BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECT_USERDashboardViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECT_USERDashboardViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECT_USERDashboardViewModelWrapper()
        {
        }

        #region Database Operation
        private PROJECT _loadPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        ActionObject actionObject;
        private DispatcherTimer selectAllDispatcher;
        protected override void resolveParameters(object parameter)
        {
            actionObject = parameter as ActionObject;
            if(actionObject == null)
            {
                var USERParameter = (EntitiesParameter<PROJECT>)parameter;
                _loadPROJECT = USERParameter.GetEntity();
            }

            selectAllDispatcher = new DispatcherTimer();
            selectAllDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            selectAllDispatcher.Tick += SelectAllDispatcher_Tick;
        }

        private void SelectAllDispatcher_Tick(object sender, EventArgs e)
        {
            selectAllDispatcher.Stop();
            mainThreadDispatcher.BeginInvoke(new Action(() => SelectAll()));
        }

        public void SelectAll()
        {
            Selected_Dashboards.Clear();
            foreach (BASELINE_ITEMProgress entity in DisplayEntities)
            {
                Selected_Dashboards.Add(entity);
            }
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEM_WORKS, BASELINE_ITEM_WORKProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM_WORK>, IQueryable<BASELINE_ITEM_WORK>> BASELINE_ITEM_WORKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == _loadPROJECT.GUID);
        }

        protected Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.PROGRESS.GUID_PROJECT == _loadPROJECT.GUID && x.PROGRESS.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            if (actionObject != null)
            {
                actionObject.ExecuteAction();
                return;
            }

            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>> specifyMainViewModelProjection()
        {
            return query => ProgressQueries.ProjectUser_OffsiteDirectProgressItemTransformation(query, _loadPROJECT, PROGRESS_ITEMCollection, USERCollection, BASELINE_ITEM_WORKCollection);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            MainViewModel = (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>)mainEntityLoaderDescription.GetViewModel();
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => selectAllDispatcher.Start()));
            return base.OnMainViewModelLoaded(entities);
        }
        #endregion

        #region View Behavior
        List<Dashboard_Export_Data_Point> exportExcelData;
        public List<Dashboard_Export_Data_Point> ExcelExportData
        {
            get => exportExcelData;
            set
            {
                exportExcelData = value;
                this.RaisePropertyChanged(x => x.ExcelExportData);
            }
        }

        public override void ExportToExcel()
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            IEnumerable<BASELINE_ITEMProgress> deliverables = Selected_Dashboards.Select(x => (BASELINE_ITEMProgress)x);
            ExcelExportData = DashboardHelpers.BuildExportData(deliverables);
            this.RaisePropertyChanged(x => x.ExcelExportData);
            LoadingScreenManager.CloseLoadingScreen();
            base.ExportToExcel();
        }

        public override void FullRefresh()
        {
            ReloadEntitiesCollection();
        }

        public override string UnifiedRowValidation(BASELINE_ITEMProgress projection)
        {
            return string.Empty;
        }


        public override string UnifiedValueValidation(BASELINE_ITEMProgress projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        #endregion

        #region View Properties
        decimal runningTotals;
        decimal runningCurrent;
        decimal runningPeriod;
        decimal currentValue;
        public void CustomSummary(CustomSummaryEventArgs e)
        {
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                runningPeriod = 0;
                runningCurrent = 0;
                runningTotals = 0;
                currentValue = 0;
            }
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                GridSummaryItem gridSummaryItem = e.Item as GridSummaryItem;
                if (gridSummaryItem != null)
                {
                    string fieldName = gridSummaryItem.FieldName;
                    bool is_cost = fieldName.ToUpper().Contains("COSTS");
                    bool is_period = !fieldName.ToUpper().Contains("CUMULATIVE");

                    if (is_cost)
                    {
                        runningTotals += ((IHaveStats)e.Row).Stats.BudgetedCosts;

                        if (e.IsGroupSummary && ((IHaveStats)e.Row).Stats.Earned != null)
                        {
                            if (is_period)
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                    currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Costs;
                            }
                            else
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                    currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs;
                            }
                        }
                        else if (e.IsTotalSummary)
                        {
                            if (is_period)
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                    runningPeriod += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Costs;
                            }
                            else
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                    runningCurrent += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs;
                            }
                        }
                    }
                    else
                    {
                        runningTotals += ((IHaveStats)e.Row).Stats.BudgetedUnits;
                        if (e.IsGroupSummary && ((IHaveStats)e.Row).Stats.Earned != null)
                        {
                            if (is_period)
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                    currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Units;
                            }
                            else
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                    currentValue = ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Units;
                            }
                        }
                        else if (e.IsTotalSummary)
                        {
                            if (is_period)
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint != null)
                                    runningPeriod += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodDataPoint.Units;
                            }
                            else
                            {
                                if (((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint != null)
                                    runningCurrent += ((IHaveStats)e.Row).Stats.Earned.CurrentPeriodCumulativeDataPoint.Units;
                            }
                        }
                    }


                    if (runningTotals != 0)
                    {
                        if (e.IsGroupSummary)
                            e.TotalValue = currentValue / runningTotals;
                        else if (e.IsTotalSummary)
                        {
                            if (is_period)
                                e.TotalValue = runningPeriod / runningTotals;
                            else
                                e.TotalValue = runningCurrent / runningTotals;
                        }
                    }
                    else
                        e.TotalValue = 0;
                }
                else
                    e.TotalValue = 0;
            }
        }
        //private List<DashboardTreeStructure> hierarchicalDashboard = null;
        //public List<Dashboard_Export_Data_Point> ExcelExportData => DisplayEntities == null ? null : DisplayEntities.Count == 0 ? null : DisplayEntities.First().Export_Data;
        //public bool CanExportToExcel()
        //{
        //    return DisplayEntities != null && DisplayEntities.Count > 0;
        //}

        //public override void ExportToExcel()
        //{
        //    if (hierarchicalDashboard == null)
        //        return;

        //    LoadingScreenManager.ShowLoadingScreen(1);
        //    PROJECT_Dashboard project = DisplayEntities.First();
        //    project.Export_Data = DashboardHelpers.BuildExportData(hierarchicalDashboard);
        //    this.RaisePropertyChanged(x => x.ExcelExportData);
        //    LoadingScreenManager.CloseLoadingScreen();
        //    base.ExportToExcel();
        //}

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

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }

        public IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection
        {
            get
            {
                return GetEntities<BASELINE_ITEM_WORK>();
            }
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECT_USERDashboardViewModelWrapper"; }
        }
        #endregion
    }
}