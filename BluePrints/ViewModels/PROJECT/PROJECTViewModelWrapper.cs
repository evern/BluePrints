using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BluePrints.Reports;
using System.IO;
using BluePrints.Common.Reports;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTViewModelWrapper :
        DashboardViewModelWrapper<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTViewModelWrapper()
        {
        }

        #region Database Operation
        private PROJECT loadPROJECT;
        public Action<BASELINECollectionViewModelWrapper> AssignBASELINEDelegates;
        public Action<PROGRESSCollectionViewModelWrapper> AssignPROGRESSDelegates;
        public Action<ESTIMATECollectionViewModelWrapper> AssignESTIMATEDelegates;
        public Action<AREACollectionViewModelWrapper> AssignAREADelegates;
        public Action<RATECollectionViewModelWrapper> AssignRATEDelegates;
        private DispatcherTimer selectAllDispatcher;
        private List<DashboardTreeStructure> hierarchicalDashboard = null;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter =
                (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            isSuppressPropertyChange = true;

            selectAllDispatcher = new DispatcherTimer();
            selectAllDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            selectAllDispatcher.Tick += SelectAllDispatcher_Tick;
        }

        public override void OnLoaded()
        {
            if (AppNotificationService == null || GlobalVariables.IsProjectViewNotificationShown)
            {
                base.OnLoaded();
                return;
            }

            //INotification notification = AppNotificationService.CreatePredefinedNotification("If view is not responding please email and report to su.bing-wen@primero.com.au, sorry for any inconvenience!", null, null, null);
            //GlobalVariables.IsProjectViewNotificationShown = true;
            //notification.ShowAsync();

            base.OnLoaded();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEM_WORKS, BASELINE_ITEM_WORKProjectionFunc);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.REVISION);
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.REVISION);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.STATUS);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM_WORK>, IQueryable<BASELINE_ITEM_WORK>> BASELINE_ITEM_WORKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Project_Report.ToString());
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NAME);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.INTERNAL_NUM);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.RATE1);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT_Dashboard>>
            specifyMainViewModelProjection()
        {
            var BASELINE = loaderCollection.GetObject<BASELINE>();
            var ESTIMATE = loaderCollection.GetObject<ESTIMATE>();
            var PROGRESSES = loaderCollection.GetCollection<PROGRESS>();
            var PROGRESS_ITEMS = loaderCollection.GetCollection<PROGRESS_ITEM>();
            var RATES = loaderCollection.GetCollection<RATE>();
            var VARIATIONS = loaderCollection.GetCollection<VARIATION>();

            List<PROJECT_Dashboard> project_dashboards = new List<PROJECT_Dashboard>();
            PROJECT_Dashboard project_dashboard = DashboardQueries.Single_Project_DashboardTransformation(loadPROJECT, BASELINE, ESTIMATE, PROGRESSES, PROGRESS_ITEMS, RATES, VARIATIONS, false, USERCollection, BASELINE_ITEM_WORKCollection);

            project_dashboards.Add(project_dashboard);
            return query => project_dashboards.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel.CanFillDownCallBack = CanFillDownCallBack;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void SelectAllDispatcher_Tick(object sender, EventArgs e)
        {
            selectAllDispatcher.Stop();
            mainThreadDispatcher.BeginInvoke(new Action(() => SelectAll()));
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel =
                (CollectionViewModel<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)
                mainEntityLoaderDescription.GetViewModel();

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);

            //DisplaySelectedEntities_CollectionChanged(null, null);

            if(entities.Count() > 0)
            {
                this.DisplaySelectedEntity = entities.First();
                BackgroundWorker summaryBackgroundWorker = new BackgroundWorker();
                summaryBackgroundWorker.DoWork += summaryBackgroundWorker_DoWork;
                summaryBackgroundWorker.RunWorkerCompleted += summaryBackgroundWorker_RunWorkerCompleted;
                summaryBackgroundWorker.WorkerSupportsCancellation = true;
                summaryBackgroundWorker.RunWorkerAsync(new object[] { entities.First() });
            }

            base.OnMainViewModelLoaded(entities);
            return true;
        }

        private void summaryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var argumentObject = (object[])e.Argument;
            var project = (PROJECT_Dashboard)argumentObject[0];
            
            if(project != null)
            {
                project.BuildStats(false);
                project.RecalculateStats(false);
                project.Subjob_Dashboards = DashboardHelpers.ProjectDashboardSummaryBuilder((ProjectSummaryStats)project.Stats, out hierarchicalDashboard, SUBJOBCollection);
                project.Update();

                mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.SingleProjectDashboards)));
                mainThreadDispatcher.BeginInvoke(new Action(() => IsSummaryLoading = false));
            }

            if (((BackgroundWorker)sender).CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        private void summaryBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //for raising can export to excel
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaiseCanExecuteChanged(x => x.ExportToExcel())));
            selectAllDispatcher.Start();
        }

        public bool CanExportToExcel()
        {
            return DisplayEntities != null && DisplayEntities.Count > 0 && DisplayEntities.First().Subjob_Dashboards != null;
        }

        public override void ExportToExcel()
        {
            if (hierarchicalDashboard == null)
                return;

            LoadingScreenManager.ShowLoadingScreen(1);
            PROJECT_Dashboard project = DisplayEntities.First();
            project.Export_Data = DashboardHelpers.BuildExportData(hierarchicalDashboard);
            this.RaisePropertyChanged(x => x.ExcelExportData);
            LoadingScreenManager.CloseLoadingScreen();
            base.ExportToExcel();
        }

        public List<Dashboard_Export_Data_Point> ExcelExportData => DisplayEntities == null ? null : DisplayEntities.Count == 0 ? null : DisplayEntities.First().Export_Data;
        #endregion

        #region View Behavior

        public Action Redraw;

        public void RaisePropertyChanged()
        {
            if (Redraw != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => Redraw()));

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected IOpenFileDialogService OpenFileDialogService
        {
            get { return this.GetService<IOpenFileDialogService>(); }
        }

        public bool CanFillDownCallBack(IEnumerable<PROJECT_Dashboard> selectedEntities, GridMenuInfo info)
        {
            if (info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                info.Column.FieldName == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
                return false;

            return true;
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, PROJECT_Dashboard projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF) ||
                   field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT) ||
                   field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId(); //Unpaused in existingRowAddUndoAndSave
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes)
                {
                    OpenFileDialogService.Filter = "PDF (*.PDF)|*.PDF";
                    bool DialogResult;

                    DialogResult = OpenFileDialogService.ShowDialog();
                    if (DialogResult)
                    {
                        string fullPath = OpenFileDialogService.File.GetFullName();
                        if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), null, fullPath, EntityMessageType.Changed);
                            projection.Entity.DOC_KICKOFF_PATH = fullPath;
                        }
                        else if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), null, fullPath, EntityMessageType.Changed);
                            projection.Entity.DOC_CLOSEOUT_PATH = fullPath;
                        }
                        else
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT), null, fullPath, EntityMessageType.Changed);
                            projection.Entity.DOC_SIDREPORT_PATH = fullPath;
                        }
                    }
                }
                else
                {
                    if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF_PATH), projection.Entity.DOC_KICKOFF_PATH, null, EntityMessageType.Changed);
                        projection.Entity.DOC_KICKOFF_PATH = null;
                    }
                    else if (field_name == BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT_PATH), projection.Entity.DOC_CLOSEOUT_PATH, null, EntityMessageType.Changed);
                        projection.Entity.DOC_CLOSEOUT_PATH = null;
                    }
                    else
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT_PATH), projection.Entity.DOC_SIDREPORT_PATH, null, EntityMessageType.Changed);
                        projection.Entity.DOC_SIDREPORT_PATH = null;
                    }
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().STATUS))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(projection.Entity.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

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
                        runningTotals += ((IHaveStats)e.Row).Stats.TotalCosts;

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
                        runningTotals += ((IHaveStats)e.Row).Stats.TotalUnits;
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
                        else if(e.IsTotalSummary)
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

        public async void Refresh_From_P6()
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            await BluePrintsContextHelper.RefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
            LoadingScreenManager.Progress();
            FullRefresh();
        }

        public override string UnifiedValueValidation(PROJECT_Dashboard projection, string field_name, object new_value)
        {
            string missingPathErrorString = "Path not selected";
            bool isError = false;

            if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_KICKOFF))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes && projection.Entity.DOC_KICKOFF_PATH == null)
                    isError = true;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_CLOSEOUT))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes && projection.Entity.DOC_CLOSEOUT_PATH == null)
                    isError = true;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Entity) + "." + BindableBase.GetPropertyName(() => new PROJECT().DOC_SIDREPORT))
            {
                ProjectDocumentStatus newValue = (ProjectDocumentStatus)new_value;
                if (newValue == ProjectDocumentStatus.Yes && projection.Entity.DOC_SIDREPORT_PATH == null)
                    isError = true;
            }

            if (isError)
                return missingPathErrorString;

            return string.Empty;
        }
#endregion

#region View Properties
        protected override void OnAfterSelectedEntitiesChanged()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaiseSelectionChanged()));
            base.OnAfterSelectedEntitiesChanged();
        }

        public void RaiseSelectionChanged()
        {
            this.RaisePropertyChanged(x => x.IsAllSelected);
            this.RaisePropertyChanged(x => x.IsAllDesignSelected);
            this.RaisePropertyChanged(x => x.IsAllConstructSelected);
        }

        public bool IsAllSelected
        {
            get
            {
                if (SingleProjectDashboards == null)
                    return false;

                return Selected_Dashboards.Count >= SingleProjectDashboards.Count();
            }
        }

        public bool IsAllDesignSelected
        {
            get
            {
                if (SingleProjectDashboards == null)
                    return false;

                IEnumerable<DashboardFlatStructure> selected_dashboards = Selected_Dashboards.Select(x => (DashboardFlatStructure)x);
                IEnumerable<DashboardFlatStructure> designDashboards = SingleProjectDashboards.Where(x => x.Phase != null && x.Phase == PhaseType.Design);
                return designDashboards.All(x => Selected_Dashboards.Any(y => y == x));
            }
        }

        public bool IsAllConstructSelected
        {
            get
            {
                if (SingleProjectDashboards == null)
                    return false;

                IEnumerable<DashboardFlatStructure> selected_dashboards = Selected_Dashboards.Select(x => (DashboardFlatStructure)x);
                IEnumerable<DashboardFlatStructure> constructDashboard = SingleProjectDashboards.Where(x => x.Phase != null && x.Phase == PhaseType.Construct);
                return constructDashboard.All(x => Selected_Dashboards.Any(y => y == x));
            }
        }

        public void SelectAll()
        {
            if (SingleProjectDashboards == null)
                return;

            Selected_Dashboards.Clear();
            foreach (DashboardFlatStructure subjob_dashboard in SingleProjectDashboards)
            {
                Selected_Dashboards.Add(subjob_dashboard);
            }
            RaiseSelectionChanged();
        }

        public bool CanSelectSubjob(BarCheckItem button)
        {
            if (SingleProjectDashboards == null)
                return false;

            PhaseType? button_phase = button.Content.ToString() == PhaseType.Construct.ToString() ? PhaseType.Construct : button.Content.ToString() == PhaseType.Design.ToString() ? PhaseType.Design : (PhaseType?)null;
            return SingleProjectDashboards.Any(x => x.Phase == button_phase);
        }

        public void SelectSubjob(BarCheckItem button)
        {
            PhaseType? select_phase = button.Content.ToString() == PhaseType.Construct.ToString() ? PhaseType.Construct : button.Content.ToString() == PhaseType.Design.ToString() ? PhaseType.Design : (PhaseType?)null;
            
            if(button.IsChecked != null)
            {
                bool isCheck = (bool)button.IsChecked;
                if (isCheck)
                {
                    foreach (DashboardFlatStructure subjob_dashboard in SingleProjectDashboards.Where(x => x.Phase == select_phase))
                    {
                        Selected_Dashboards.Add(subjob_dashboard);
                    }
                }
                else
                {
                    List<DashboardFlatStructure> removeSubJobs = new List<DashboardFlatStructure>();
                    foreach (DashboardFlatStructure subjob_dashboard in SingleProjectDashboards.Where(x => x.Phase == select_phase))
                    {
                        removeSubJobs.Add(subjob_dashboard);
                    }

                    foreach(DashboardFlatStructure removeSubJob in removeSubJobs)
                    {
                        Selected_Dashboards.Remove(removeSubJob);
                    }
                }
            }

            RaiseSelectionChanged();
        }

        public IEnumerable<DashboardFlatStructure> SingleProjectDashboards
        {
            get
            {
                if (DisplayEntities == null || DisplayEntities.Count == 0)
                    return null;

                List<DashboardFlatStructure> singleProjectDashboard = DisplayEntities.First().Subjob_Dashboards;
                return singleProjectDashboard;
            }
        }

        private BASELINECollectionViewModelWrapper baselineViewModel;

        public BASELINECollectionViewModelWrapper BASELINEViewModel
        {
            get
            {
                if (baselineViewModel == null && loadPROJECT != null)
                {
                    baselineViewModel = BASELINECollectionViewModelWrapper.Create();
                    baselineViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = baselineViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignBASELINEDelegates?.Invoke(baselineViewModel);
                }

                return baselineViewModel;
            }
        }

        private PROGRESSCollectionViewModelWrapper progressViewModel;

        public PROGRESSCollectionViewModelWrapper PROGRESSViewModel
        {
            get
            {
                if (progressViewModel == null && loadPROJECT != null)
                {
                    progressViewModel = PROGRESSCollectionViewModelWrapper.Create();
                    progressViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = progressViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignPROGRESSDelegates?.Invoke(progressViewModel);
                }

                return progressViewModel;
            }
        }

        private AREACollectionViewModelWrapper areaViewModel;

        public AREACollectionViewModelWrapper AREAViewModel
        {
            get
            {
                if (areaViewModel == null && loadPROJECT != null)
                {
                    areaViewModel = AREACollectionViewModelWrapper.Create();
                    areaViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = areaViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignAREADelegates?.Invoke(areaViewModel);
                }

                return areaViewModel;
            }
        }

        private RATECollectionViewModelWrapper rateViewModel;

        public RATECollectionViewModelWrapper RATEViewModel
        {
            get
            {
                if (rateViewModel == null && loadPROJECT != null)
                {
                    rateViewModel = RATECollectionViewModelWrapper.Create();
                    rateViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = rateViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignRATEDelegates?.Invoke(rateViewModel);
                }

                return rateViewModel;
            }
        }

        private ESTIMATECollectionViewModelWrapper estimationDirectViewModel;

        public ESTIMATECollectionViewModelWrapper ESTIMATEViewModel
        {
            get
            {
                if (estimationDirectViewModel == null && loadPROJECT != null)
                {
                    estimationDirectViewModel = ESTIMATECollectionViewModelWrapper.Create();
                    estimationDirectViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = estimationDirectViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignESTIMATEDelegates?.Invoke(estimationDirectViewModel);
                }

                return estimationDirectViewModel;
            }
        }

        public bool CanEditReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Project_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        protected override void loadReportLayoutFromDatabase(XtraReportDashboard xtraReport)
        {
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    xtraReport.LoadLayout(sw.BaseStream);
                }
            }

            base.loadReportLayoutFromDatabase(xtraReport);
        }

        public override void FullRefresh()
        {
            ReloadEntitiesCollection();
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString() + "SubjobDashboardView",
                DisplaySelectedEntity,
                "SUBJOBDashboardView",
                "[" + DisplaySelectedEntity.Entity.NUMBER + "] SUBJOB Dashboard");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTViewModelWrapper_v1.01"; }
        }

        public void EditArea()
        {
            if (loadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectAreas" + loadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "AREACollectionView",
                    "[" + loadPROJECT.NUMBER + "] Areas");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditRate()
        {
            if (loadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectRates" + loadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "RATECollectionView",
                    "[" + loadPROJECT.NUMBER + "] Rates");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditBaseline()
        {
            if (loadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectBaselines" + loadPROJECT.GUID.ToString() ,
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "BASELINECollectionView",
                    "[" + loadPROJECT.NUMBER + "] Baseline Revisions");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditEstimate()
        {
            if (loadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectEstimates" + loadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "ESTIMATECollectionView",
                    "[" + loadPROJECT.NUMBER + "] Estimate Revisions");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public void EditProgress()
        {
            if (loadPROJECT == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo("View_ProjectProgress" + loadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "PROGRESSCollectionView",
                    "[" + loadPROJECT.NUMBER + "] Progress Revisions");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public IEnumerable<USER> MANAGERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.Where(x => x.ROLE != null && x.ROLE.ISMANAGER).OrderBy(x => x.NAME);
                return collection;
            }
        }

        public string DesignDataDate
        {
            get
            {
                var collection = GetEntities<PROGRESS>();

                PROGRESS livePROGRESS = null;
                if (collection != null)
                    livePROGRESS = collection.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);

                if(livePROGRESS != null)
                {
                    DateTime dateToUse;
                    if (livePROGRESS.REPORT_DATE != null)
                        dateToUse = (DateTime)livePROGRESS.REPORT_DATE;
                    else
                        dateToUse = livePROGRESS.DATA_DATE;

                    return dateToUse.ToString("dd-MMM-yy");
                }

                return "N/A";
            }
        }

        public string ConstructDataDate
        {
            get
            {
                var collection = GetEntities<PROGRESS>();

                PROGRESS livePROGRESS = null;
                if (collection != null)
                    livePROGRESS = collection.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Construct);

                if (livePROGRESS != null)
                {
                    DateTime dateToUse;
                    if (livePROGRESS.REPORT_DATE != null)
                        dateToUse = (DateTime)livePROGRESS.REPORT_DATE;
                    else
                        dateToUse = livePROGRESS.DATA_DATE;

                    return dateToUse.ToString("dd-MMM-yy");
                }

                return "N/A";
            }
        }

        public IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM_WORK>();
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

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                return collection;
            }
        }
#endregion
    }
}