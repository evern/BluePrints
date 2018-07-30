using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.Services;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.Common.ViewModel
{
    public class Summary
    {
        public SummaryItemType Type { get; set; }
        public string FieldName { get; set; }
    }

    public abstract class DashboardViewModelWrapper<TEntity, TProjection, TPrimaryKey, TUnitOfWork> :
        BluePrintsEntitiesCollectionWrapper
        <TEntity, TProjection, TPrimaryKey, TUnitOfWork>, ISupportStatsSwitching
        where TEntity : class, IGuidEntityKey, new()
        where TUnitOfWork : IUnitOfWork
        where TProjection : class, IGuidEntityKey, IHaveStats, ICanUpdate, new()
    {
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> UnitOfWorkFactory;
        private DispatcherTimer dispatchTimer;
        private DispatcherTimer first_loaded_dispatchTimer;
        //in single project mode selected change is suppressed but resume after summary has been loaded
        protected bool isSuppressPropertyChange;
        private bool isChartLoading { get; set; }
        private bool isSummaryLoading { get; set; }

        public bool IsChartLoading
        {
            get => isChartLoading;
            set
            {
                isChartLoading = value;
                this.RaisePropertyChanged(x => x.IsChartLoading);
            }
        }

        public bool IsSummaryLoading
        {
            get => isSummaryLoading;
            set
            {
                isSummaryLoading = value;
                this.RaisePropertyChanged(x => x.IsSummaryLoading);
            }
        }

        public DashboardViewModelWrapper()
        {
            DoNotAutoRefresh = true;
            IsSummaryLoading = true;
            IsChartLoading = true;
            isSuppressPropertyChange = false;

            Selected_Dashboards = new ObservableCollection<IHaveStats>();
            Selected_Dashboards.CollectionChanged += SelectedDashboard_CollectionChanged;

            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            first_loaded_dispatchTimer = new DispatcherTimer();
            first_loaded_dispatchTimer.Interval = new TimeSpan(0, 0, 0, 3);
            first_loaded_dispatchTimer.Tick += first_loaded_dispatchTimer_Tick;
        }
        
        protected override bool OnMainViewModelLoaded(IEnumerable<TProjection> entities)
        {
            //because dashboards are generally heavy we just want manual refreshes to take place
            MainViewModel.ManualUnregisterMessageHandler();
            mainThreadDispatcher.BeginInvoke(new Action(() => first_loaded_dispatchTimer.Start()));
            return base.OnMainViewModelLoaded(entities);
        }

        private void first_loaded_dispatchTimer_Tick(object sender, EventArgs e)
        {
            first_loaded_dispatchTimer.Stop();
            if (MainViewModel == null)
                return;

            if (MainViewModel.Entities.Count > 0)
            {
                this.SwitchBinding(DashboardViewType.Units, null, GridControlService);
                SummaryEntity = MainViewModel.Entities.First();
                this.RaisePropertyChanged(x => x.SummaryEntity);
            }

            executeFirstLoadedActions();
        }

        protected virtual void executeFirstLoadedActions()
        {

        }

        private void dispatchTimer_Tick(object sender, EventArgs e)
        {
            dispatchTimer.Stop();
            if (isSuppressPropertyChange)
                isSuppressPropertyChange = false;
            else
                OnSelectedEntitiesChanged(Selected_Dashboards);
        }

        private void SelectedDashboard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //multiple selection will call this multiple times. so this is used to remove unecessary calls
            dispatchTimer.Tick -= dispatchTimer_Tick;
            dispatchTimer.Tick += dispatchTimer_Tick;
            dispatchTimer.Start();
        }

        public virtual TProjection SummaryEntity { get; set; }
        public ObservableCollection<IHaveStats> Selected_Dashboards { get; set; }
        public void OnSelectedEntitiesChanged(IEnumerable<IHaveStats> entities)
        {
            IsChartLoading = true;
            if (MainViewModel == null)
                return;

            if (entities.Count() > 0)
            {
                SummaryEntity = ViewModelSource.Create(() => new TProjection());
                ProgressStats progressStats = entities.First().Stats as ProgressStats;
                SummaryStats summaryStats = entities.First().Stats as SummaryStats;

                if (summaryStats != null)
                {
                    IEnumerable<SummaryStats> entitiesSummary = entities.Select(x => (SummaryStats)x.Stats);
                    SummaryEntity.Stats = new SummaryStats(entitiesSummary);
                }
                else if (progressStats != null)
                {
                    IEnumerable<ProgressStats> entitiesSummary = entities.Select(x => (ProgressStats)x.Stats);
                    SummaryEntity.Stats = new ProgressStats(entitiesSummary);
                }
            }
            else
            {
                if (SummaryEntity == null)
                    return;

                SummaryEntity.Stats = null;
            }


            IsChartLoading = false;
            this.RaisePropertyChanged(x => x.SummaryEntity);
            OnAfterSelectedEntitiesChanged();
        }

        protected virtual void OnAfterSelectedEntitiesChanged()
        {

        }

        public virtual bool CanChangeStatsType(object checkButton)
        {
            return MainViewModel != null && !MainViewModel.IsLoading;
        }

        public Action<DashboardViewType> ChangeViewMemberFieldNames { get; set; }

        protected bool switchChartOnly;
        DashboardViewType viewType;
        public virtual void ChangeStatsType(object checkButton)
        {
            var button = (BarButtonItem) checkButton;
            viewType = button.Name.ToUpper().Contains("COSTS")
                ? DashboardViewType.Costs
                : button.Name.ToUpper().Contains("QTY") ? DashboardViewType.Quantity : DashboardViewType.Units;
            this.SwitchBinding(viewType, null, GridControlService, switchChartOnly);
            ChangeViewMemberFieldNames?.Invoke(viewType);

            IHaveSummary IHaveSummary = SummaryEntity as IHaveSummary;
            if (IHaveSummary != null)
                IHaveSummary.RecalculateStats(viewType == DashboardViewType.Costs);
        }

        public virtual void ChangeStatsPercentageType(object checkButton)
        {
            var button = (BarCheckItem)checkButton;
            bool usePercentage = button.Name.ToUpper().Contains("PERCENT");

            //pass in cost as true by default but when percentage is used isCost will be determine by what is currently used
            this.SwitchBinding(viewType, usePercentage, GridControlService, switchChartOnly);
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            
        }

        #region P6 Affinity
        public bool CanShowP6Errors()
        {

            if (DisplaySelectedEntity == null)
                return false;

            SummaryStats summaryStats = DisplaySelectedEntity.Stats as SummaryStats;
            if (summaryStats == null || summaryStats.Reportables == null)
                return false;

            if (!summaryStats.Reportables.Any(x => x.Stats != null && x.Stats.Budgeted != null && x.Stats.Budgeted.FromP6))
                return true;

            if (!summaryStats.Reportables.Any(x => x.Stats != null && x.Stats.Remaining != null && x.Stats.Remaining.FromP6))
                return true;

            return false;
        }

        public void ShowP6Errors()
        {
            if (DisplaySelectedEntity == null)
                return;

            SummaryStats summaryStats = DisplaySelectedEntity.Stats as SummaryStats;
            if (summaryStats == null)
                return;

            DialogCollectionViewModel<IReportable> viewModel = DialogCollectionViewModel<IReportable>.Create(summaryStats.Reportables);
            IssuesDialogService.ShowDialog(MessageButton.OK, "P6 Affinity Report", "PrimaveraAffinityReport", viewModel);
        }
        #endregion

        #region Exo Affinity
        private DevExpress.Mvvm.IDialogService IssuesDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("IssuesDialogService"); }
        }

        public bool CanShowExoErrors()
        {
            if (DisplaySelectedEntity == null)
                return false;

            ProjectSummaryStats projectSummary = DisplaySelectedEntity.Stats as ProjectSummaryStats;
            if (projectSummary == null || projectSummary.ExoMissingSUBJOBS == null || projectSummary.ExoMissingSUBJOBS.Count == 0)
                return false;

            return true;
        }

        public void ShowExoErrors()
        {
            ProjectSummaryStats projectSummary = DisplaySelectedEntity.Stats as ProjectSummaryStats;
            if (projectSummary == null || projectSummary.ExoMissingSUBJOBS == null || projectSummary.ExoMissingSUBJOBS.Count == 0)
                return;

            DialogCollectionViewModel<SUBJOB> viewModel = DialogCollectionViewModel<SUBJOB>.Create(projectSummary.ExoMissingSUBJOBS);
            IssuesDialogService.ShowDialog(MessageButton.OK, "Exo Affinity Report", "ExoAffinityReport", viewModel);
        }

        public virtual bool CanViewReport()
        {
            return SummaryEntity != null && SummaryEntity.Stats != null;
        }

        public void ViewReport()
        {
            SummaryStats displaySummary = SummaryEntity.Stats as SummaryStats;
            if (displaySummary == null)
                return;

            var progressReport = new XtraReportDashboard();
            loadReportLayoutFromDatabase(progressReport);

            string title = string.Empty;
            PROJECT_Dashboard project_dashboard = DisplaySelectedEntity as PROJECT_Dashboard;
            if (project_dashboard != null)
                title = project_dashboard.Entity.NAME;

            progressReport.AssignProperties(displaySummary, displaySummary.ReportingDataDate, title);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = progressReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            progressReport.RequestParameters = false;
            progressReport.CreateDocument(true);
            previewWindow.Show();
        }

        protected virtual void loadReportLayoutFromDatabase(XtraReportDashboard xtraReport)
        {

        }
        #endregion

        #region View Stats Binding
        public string Field_Mask { get; set; }
        public string Total_Budgeted { get; set; }
        public string Total_Current { get; set; }
        public string Cumulative_Current_Earned_Percentage { get; set; }
        public string Cumulative_Budgeted_Earned_Percentage { get; set; }
        public string Cumulative_Planned_Units { get; set; }
        public string Cumulative_Current_Units { get; set; }
        public string Cumulative_PlannedLate_Units { get; set; }
        public string Cumulative_Earned_Units { get; set; }
        public string Cumulative_Earned_Quantity { get; set; }
        public string Cumulative_Burned_Units { get; set; }
        public string Cumulative_Actual_Units { get; set; }
        public string Cumulative_Remaining_Units { get; set; }
        public string Period_Current_Earned_Percentage { get; set; }
        public string Period_Budgeted_Earned_Percentage { get; set; }
        public string Period_Planned_Units { get; set; }
        public string Period_PlannedLate_Units { get; set; }
        public string Period_Current_Units { get; set; }
        public string Period_Earned_Units { get; set; }
        public string Period_Burned_Units { get; set; }
        public string Period_Actual_Units { get; set; }
        public string Period_Earned_Quantity { get; set; }
        public bool IsActualVisible { get; set; }
        public string Header_Total_Budgeted { get; set; }
        public string Header_Total_Current { get; set; }
        public string Header_Budgeted_ToDate { get; set; }
        public string Header_BudgetedLate_ToDate { get; set; }
        public string Header_Current_ToDate { get; set; }
        public string Header_Earned_ToDate { get; set; }
        public string Header_Burned_ToDate { get; set; }
        public string Header_Actual_ToDate { get; set; }
        public string Cumulative_Planned_DisplayName { get; set; }
        public string Cumulative_Current_DisplayName { get; set; }
        public string Cumulative_Earned_DisplayName { get; set; }
        public string Cumulative_Burned_DisplayName { get; set; }
        public string Cumulative_Actual_DisplayName { get; set; }
        public string Periodic_Planned_DisplayName { get; set; }
        public string Periodic_Current_DisplayName { get; set; }
        public string Periodic_Earned_DisplayName { get; set; }
        public string Periodic_Burned_DisplayName { get; set; }
        public string Periodic_Actual_DisplayName { get; set; }
        public string Summary_Display_Format { get; set; }
        public string Header_Remaining_ToDate { get; set; }
        public string Header_RemainingActual_ToDate { get; set; }
        public string Header_Remaining_Period { get; set; }
        public string Header_RemainingActual_Period { get; set; }
        public string BarSeriesValueDataMember { get; set; }
        public string LineSeriesValueDataMember { get; set; }
        public string BarSeriesCrosshairPattern { get; set; }
        public string AxisYPrimaryLabel { get; set; }
        public string AxisYSecondaryLabel { get; set; }
        public string AxisYSecondaryTextPattern { get; set; }
        public string LineSeriesBudgetDisplayName { get; set; }
        public string LineSeriesCurrentDisplayName { get; set; }
        public string LineSeriesBudgetLateDisplayName { get; set; }
        public string LineSeriesRemainingDisplayName { get; set; }
        public string LineSeriesRemainingActualDisplayName { get; set; }
        public string LineSeriesEarnedDisplayName { get; set; }
        public string LineSeriesTenderEarnedDisplayName { get; set; }
        public string LineSeriesBurnedDisplayName { get; set; }
        public string LineSeriesActualDisplayName { get; set; }
        public string LineSeriesLabelPattern { get; set; }
        public string Total_Budgeted_Quantity { get; set; }
        public string Total_Current_Quantity { get; set; }
        public string Cumulative_Remaining_Quantity { get; set; }
        public string CumulativeEarnedVsBurned { get; set; }
        public string PeriodEarnedVsBurned { get; set; }
        public string CumulativePerformanceRatio { get; set; }
        public string PeriodPerformanceRatio { get; set; }
        public string AdjustedRemaining { get; set; }
        public string AdjustedDifference { get; set; }
        public string Header_RemainingQuantity { get; set; }
        public string Header_CumulativeEarnedVsBurned { get; set; }
        public string Header_CumulativePerformanceRatio { get; set; }
        public string Header_AdjustedRemaining { get; set; }
        public string Header_AdjustedDifference { get; set; }
        public string Header_PeriodEarnedVsBurned { get; set; }
        public string Header_PeriodPerformanceRatio { get; set; }
        public string Header_Total_Budgeted_Quantity { get; set; }
        public string Header_Total_Current_Quantity { get; set; }
        public string Header_Cumulative_Current_Earned_Percentage { get; set; }
        public string Header_Cumulative_Budgeted_Earned_Percentage { get; set; }
        public string Header_Period_Current_Earned_Percentage { get; set; }
        public string Header_Period_Budgeted_Earned_Percentage { get; set; }
        public string Header_Budgeted_Period { get; set; }
        public string Header_Current_Period { get; set; }
        public string Header_BudgetedLate_Period { get; set; }
        public string Header_Earned_Period { get; set; }
        public string Header_Burned_Period { get; set; }
        public string Header_Actual_Period { get; set; }
        public string Header_Cumulative_Earned_Quantity { get; set; }
        public string Header_Period_Earned_Quantity { get; set; }

        public void StatsUpdate()
        {
            this.RaisePropertyChanged(x => x.Field_Mask);
            this.RaisePropertyChanged(x => x.Total_Current);
            this.RaisePropertyChanged(x => x.Total_Budgeted);
            this.RaisePropertyChanged(x => x.Cumulative_Current_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Cumulative_Budgeted_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Cumulative_Planned_Units);
            this.RaisePropertyChanged(x => x.Cumulative_PlannedLate_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Current_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Earned_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Burned_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Actual_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Remaining_Units);
            this.RaisePropertyChanged(x => x.Period_Current_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Period_Budgeted_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Period_Planned_Units);
            this.RaisePropertyChanged(x => x.Period_PlannedLate_Units);
            this.RaisePropertyChanged(x => x.Period_Current_Units);
            this.RaisePropertyChanged(x => x.Period_Earned_Units);
            this.RaisePropertyChanged(x => x.Period_Burned_Units);
            this.RaisePropertyChanged(x => x.Period_Actual_Units);
            this.RaisePropertyChanged(x => x.IsActualVisible);
            this.RaisePropertyChanged(x => x.Header_Total_Current);
            this.RaisePropertyChanged(x => x.Header_Total_Budgeted);
            this.RaisePropertyChanged(x => x.Header_Budgeted_ToDate);
            this.RaisePropertyChanged(x => x.Header_Current_ToDate);
            this.RaisePropertyChanged(x => x.Header_BudgetedLate_ToDate);
            this.RaisePropertyChanged(x => x.Header_Earned_ToDate);
            this.RaisePropertyChanged(x => x.Header_Burned_ToDate);
            this.RaisePropertyChanged(x => x.Header_Actual_ToDate);
            this.RaisePropertyChanged(x => x.Header_Remaining_ToDate);
            this.RaisePropertyChanged(x => x.Header_RemainingActual_ToDate);
            this.RaisePropertyChanged(x => x.Cumulative_Planned_DisplayName);
            this.RaisePropertyChanged(x => x.Cumulative_Earned_DisplayName);
            this.RaisePropertyChanged(x => x.Cumulative_Burned_DisplayName);
            this.RaisePropertyChanged(x => x.Cumulative_Actual_DisplayName);
            this.RaisePropertyChanged(x => x.Cumulative_Current_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Planned_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Earned_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Burned_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Actual_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Current_DisplayName);
            this.RaisePropertyChanged(x => x.Summary_Display_Format);
            this.RaisePropertyChanged(x => x.BarSeriesValueDataMember);
            this.RaisePropertyChanged(x => x.LineSeriesValueDataMember);
            this.RaisePropertyChanged(x => x.BarSeriesCrosshairPattern);
            this.RaisePropertyChanged(x => x.AxisYPrimaryLabel);
            this.RaisePropertyChanged(x => x.AxisYSecondaryLabel);
            this.RaisePropertyChanged(x => x.AxisYSecondaryTextPattern);
            this.RaisePropertyChanged(x => x.LineSeriesBudgetDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesCurrentDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesBudgetLateDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesRemainingDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesRemainingActualDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesEarnedDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesTenderEarnedDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesBurnedDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesActualDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesLabelPattern);

            this.RaisePropertyChanged(x => x.Total_Budgeted_Quantity);
            this.RaisePropertyChanged(x => x.Total_Current_Quantity);
            this.RaisePropertyChanged(x => x.Cumulative_Remaining_Quantity);
            this.RaisePropertyChanged(x => x.CumulativeEarnedVsBurned);
            this.RaisePropertyChanged(x => x.PeriodEarnedVsBurned);
            this.RaisePropertyChanged(x => x.CumulativePerformanceRatio);
            this.RaisePropertyChanged(x => x.PeriodPerformanceRatio);
            this.RaisePropertyChanged(x => x.AdjustedRemaining);
            this.RaisePropertyChanged(x => x.AdjustedDifference);

            this.RaisePropertyChanged(x => x.Header_RemainingQuantity);
            this.RaisePropertyChanged(x => x.Header_CumulativeEarnedVsBurned);
            this.RaisePropertyChanged(x => x.Header_CumulativePerformanceRatio);
            this.RaisePropertyChanged(x => x.Header_AdjustedRemaining);
            this.RaisePropertyChanged(x => x.Header_AdjustedDifference);
            this.RaisePropertyChanged(x => x.Header_PeriodEarnedVsBurned);
            this.RaisePropertyChanged(x => x.Header_PeriodPerformanceRatio);

            this.RaisePropertyChanged(x => x.Header_Total_Budgeted_Quantity);
            this.RaisePropertyChanged(x => x.Header_Total_Current_Quantity);

            this.RaisePropertyChanged(x => x.Header_Cumulative_Current_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Header_Cumulative_Budgeted_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Header_Period_Current_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Header_Period_Budgeted_Earned_Percentage);

            this.RaisePropertyChanged(x => x.Header_Budgeted_Period);
            this.RaisePropertyChanged(x => x.Header_Current_Period);
            this.RaisePropertyChanged(x => x.Header_BudgetedLate_Period);
            this.RaisePropertyChanged(x => x.Header_Earned_Period);
            this.RaisePropertyChanged(x => x.Header_Burned_Period);
            this.RaisePropertyChanged(x => x.Header_Actual_Period);

            this.RaisePropertyChanged(x => x.Cumulative_Earned_Quantity);
            this.RaisePropertyChanged(x => x.Period_Earned_Quantity);

            this.RaisePropertyChanged(x => x.Header_Cumulative_Earned_Quantity);
            this.RaisePropertyChanged(x => x.Header_Period_Earned_Quantity);

            this.RaisePropertyChanged(x => x.Header_Remaining_Period);
            this.RaisePropertyChanged(x => x.Header_RemainingActual_Period);
        }
        #endregion
    }

    public interface ISupportStatsSwitching
    {
        string Field_Mask { get; set; }
        string Total_Budgeted { get; set; }
        string Total_Current { get; set; }
        string Total_Budgeted_Quantity { get; set; }
        string Total_Current_Quantity { get; set; }
        string Cumulative_Current_Earned_Percentage { get; set; }
        string Cumulative_Budgeted_Earned_Percentage { get; set; }
        string Cumulative_Planned_Units { get; set; }
        string Cumulative_Current_Units { get; set; }
        string Cumulative_PlannedLate_Units { get; set; }
        string Cumulative_Earned_Units { get; set; }
        string Cumulative_Earned_Quantity { get; set; }
        string Cumulative_Burned_Units { get; set; }
        string Cumulative_Actual_Units { get; set; }
        string Cumulative_Remaining_Units { get; set; }
        string Cumulative_Remaining_Quantity { get; set; }
        string Period_Current_Earned_Percentage { get; set; }
        string Period_Budgeted_Earned_Percentage { get; set; }
        string Period_Planned_Units { get; set; }
        string Period_Current_Units { get; set; }
        string Period_PlannedLate_Units { get; set; }
        string Period_Earned_Units { get; set; }
        string Period_Earned_Quantity { get; set; }
        string Period_Burned_Units { get; set; }
        string Period_Actual_Units { get; set; }
        string Header_Total_Budgeted { get; set; }
        string Header_Total_Current { get; set; }
        string Header_Total_Budgeted_Quantity { get; set; }
        string Header_Total_Current_Quantity { get; set; }
        string Header_Budgeted_ToDate { get; set; }
        string Header_Current_ToDate { get; set; }
        string Header_BudgetedLate_ToDate { get; set; }
        string Header_Earned_ToDate { get; set; }
        string Header_Burned_ToDate { get; set; }
        string Header_Actual_ToDate { get; set; }
        string Header_Budgeted_Period { get; set; }
        string Header_Current_Period { get; set; }
        string Header_BudgetedLate_Period { get; set; }
        string Header_Earned_Period { get; set; }
        string Header_Burned_Period { get; set; }
        string Header_Actual_Period { get; set; }
        string Header_Remaining_ToDate { get; set; }
        string Header_RemainingActual_ToDate { get; set; }
        string Header_Remaining_Period { get; set; }
        string Header_RemainingActual_Period { get; set; }
        string Header_RemainingQuantity { get; set; }
        string Header_CumulativeEarnedVsBurned { get; set; }
        string Header_CumulativePerformanceRatio { get; set; }
        string Header_AdjustedRemaining { get; set; }
        string Header_AdjustedDifference { get; set; }
        string Header_PeriodEarnedVsBurned { get; set; }
        string Header_PeriodPerformanceRatio { get; set; }
        string Header_Cumulative_Current_Earned_Percentage { get; set; }
        string Header_Cumulative_Budgeted_Earned_Percentage { get; set; }
        string Header_Period_Current_Earned_Percentage { get; set; }
        string Header_Period_Budgeted_Earned_Percentage { get; set; }
        string CumulativeEarnedVsBurned { get; set; }
        string PeriodEarnedVsBurned { get; set; }
        string CumulativePerformanceRatio { get; set; }
        string PeriodPerformanceRatio { get; set; }
        string AdjustedRemaining { get; set; }
        string AdjustedDifference { get; set; }
        bool IsActualVisible { get; set; }
        string Cumulative_Planned_DisplayName { get; set; }
        string Cumulative_Current_DisplayName { get; set; }
        string Cumulative_Earned_DisplayName { get; set; }
        string Cumulative_Burned_DisplayName { get; set; }
        string Cumulative_Actual_DisplayName { get; set; }
        string Periodic_Current_DisplayName { get; set; }
        string Periodic_Planned_DisplayName { get; set; }
        string Periodic_Earned_DisplayName { get; set; }
        string Periodic_Burned_DisplayName { get; set; }
        string Periodic_Actual_DisplayName { get; set; }
        string Summary_Display_Format { get; set; }
        string BarSeriesValueDataMember { get; set; }
        string LineSeriesValueDataMember { get; set; }
        string BarSeriesCrosshairPattern { get; set; }
        string AxisYPrimaryLabel { get; set; }
        string AxisYSecondaryLabel { get; set; }
        string AxisYSecondaryTextPattern { get; set; }
        string LineSeriesBudgetDisplayName { get; set; }
        string LineSeriesCurrentDisplayName { get; set; }
        string LineSeriesBudgetLateDisplayName { get; set; }
        string LineSeriesRemainingDisplayName { get; set; }
        string LineSeriesRemainingActualDisplayName { get; set; }
        string LineSeriesEarnedDisplayName { get; set; }
        string LineSeriesTenderEarnedDisplayName { get; set; }
        string LineSeriesBurnedDisplayName { get; set; }
        string LineSeriesActualDisplayName { get; set; }
        string LineSeriesLabelPattern { get; set; }
        string Header_Cumulative_Earned_Quantity { get; set; }
        string Header_Period_Earned_Quantity { get; set; }

        void StatsUpdate();
    }

    public enum StatsSwitchType
    {
        Units = 0,
        Costs = 1,
        Quantity = 2
    }

    public static class ISupportStatsSwitchingExtension
    {
        public static void SwitchBinding(this ISupportStatsSwitching stats_switch, DashboardViewType viewType, bool? isPercentage, IGridControlService gridControlService, bool switchChartOnly = false)
        {
            bool usePercentage;
            if(isPercentage == null)
            {
                usePercentage = stats_switch.LineSeriesValueDataMember == null ? true : stats_switch.LineSeriesValueDataMember.ToUpper().Contains("PERCENTAGE");
            }

            else
            {
                usePercentage = (bool)isPercentage;
            }

            if(!switchChartOnly)
            {
                stats_switch.IsActualVisible = viewType == DashboardViewType.Costs ? true : false;
                stats_switch.Field_Mask = viewType == DashboardViewType.Costs ? "c" : "n";
                stats_switch.Summary_Display_Format = viewType == DashboardViewType.Costs ? "{}{0:c}" : "{}{0:n}";
                string stats_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}";

                string current_period_cumulative_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}." + BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint) + ".";
                string current_period_current_cumulative_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}." + BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint) + ".";

                string current_period_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}." + BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodDataPoint) + ".";

                string units_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.Units);
                string cost_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs);
                string quantity_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.Quantity);

                string units_percentage_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.UnitsPercentage);
                string cost_percentage_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.CostsPercentage);

                PROJECT_Dashboard dashboard = new PROJECT_Dashboard();
                string planned_string = "Planned";
                string planned_late_string = "Late Planned";
                string budgeted_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Budgeted);
                string budgeted_late_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).BudgetedLate);
                string current_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Current);
                string earned_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Earned);
                string tender_earned_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).TenderEarned);
                string burned_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Burned);
                string actual_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Actual);
                string remaining_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Remaining);
                string remaining_actual_string = "Actual " + BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Remaining);

                string field_selection_string = viewType == DashboardViewType.Costs ? cost_string : viewType == DashboardViewType.Quantity ? quantity_string : units_string;
                string field_percentage_selection_string = viewType == DashboardViewType.Costs ? cost_percentage_string : units_percentage_string;

                string total_budgeted_convention = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}";
                stats_switch.Total_Current = viewType == DashboardViewType.Costs ? String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.TotalCosts)) : viewType == DashboardViewType.Quantity ? String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.TotalQty)) : String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.TotalUnits));
                stats_switch.Total_Budgeted = viewType == DashboardViewType.Costs ? String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.BudgetedCosts)) : viewType == DashboardViewType.Quantity ? String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.BudgetedQty)) : String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.BudgetedUnits));
                stats_switch.Total_Budgeted_Quantity = String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.TotalQty));
                stats_switch.Total_Current_Quantity = String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.BudgetedQty));

                stats_switch.Cumulative_Current_Earned_Percentage = String.Format(current_period_cumulative_string, earned_string) + field_percentage_selection_string;
                stats_switch.Cumulative_Budgeted_Earned_Percentage = String.Format(current_period_cumulative_string, tender_earned_string) + field_percentage_selection_string;
                stats_switch.Cumulative_Planned_Units = String.Format(current_period_cumulative_string, budgeted_string) + field_selection_string;
                stats_switch.Cumulative_PlannedLate_Units = String.Format(current_period_cumulative_string, budgeted_late_string) + field_selection_string;
                stats_switch.Cumulative_Current_Units = String.Format(current_period_cumulative_string, current_string) + field_selection_string;
                stats_switch.Cumulative_Earned_Units = String.Format(current_period_cumulative_string, earned_string) + field_selection_string;
                stats_switch.Cumulative_Earned_Quantity = String.Format(current_period_cumulative_string, earned_string) + quantity_string;
                stats_switch.Cumulative_Burned_Units = String.Format(current_period_cumulative_string, burned_string) + field_selection_string;
                stats_switch.Cumulative_Actual_Units = String.Format(current_period_cumulative_string, actual_string) + field_selection_string;
                stats_switch.Cumulative_Remaining_Units = viewType == DashboardViewType.Costs ? String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).Remaining_Costs))) : viewType == DashboardViewType.Quantity ? String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).Remaining_Quantity))) : String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).Remaining_Units)));
                stats_switch.Cumulative_Remaining_Quantity = String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).Remaining_Quantity)));
                stats_switch.CumulativeEarnedVsBurned = String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).CumulativeEarnedVsBurned_Units))).Replace(units_string, field_selection_string);
                stats_switch.CumulativePerformanceRatio = String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).CumulativePerformanceRatio_Units))).Replace(units_string, field_selection_string);
                stats_switch.AdjustedRemaining = String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).AdjustedRemaining_Units))).Replace(units_string, field_selection_string);
                stats_switch.AdjustedDifference = String.Format(stats_string, BindableBase.GetPropertyName(() => (((SummaryStats)new PROJECT_Dashboard().Stats).AdjustedDifference_Units))).Replace(units_string, field_selection_string);

                stats_switch.Period_Current_Earned_Percentage = String.Format(current_period_string, earned_string) + field_percentage_selection_string;
                stats_switch.Period_Budgeted_Earned_Percentage = String.Format(current_period_string, tender_earned_string) + field_percentage_selection_string;
                stats_switch.Period_Planned_Units = String.Format(current_period_string, budgeted_string) + field_selection_string;
                stats_switch.Period_PlannedLate_Units = String.Format(current_period_string, budgeted_late_string) + field_selection_string;
                stats_switch.Period_Current_Units = String.Format(current_period_string, current_string) + field_selection_string;
                stats_switch.Period_Earned_Units = String.Format(current_period_string, earned_string) + field_selection_string;
                stats_switch.Period_Earned_Quantity = String.Format(current_period_string, earned_string) + quantity_string;
                stats_switch.Period_Burned_Units = String.Format(current_period_string, burned_string) + field_selection_string;
                stats_switch.Period_Actual_Units = String.Format(current_period_string, actual_string) + field_selection_string;
                stats_switch.PeriodEarnedVsBurned = String.Format(stats_string, BindableBase.GetPropertyName(() => ((SummaryStats)new PROJECT_Dashboard().Stats).PeriodEarnedVsBurned_Units).Replace(units_string, field_selection_string));
                stats_switch.PeriodPerformanceRatio = String.Format(stats_string, BindableBase.GetPropertyName(() => ((SummaryStats)new PROJECT_Dashboard().Stats).PeriodPerformanceRatio_Units).Replace(units_string, field_selection_string));

                stats_switch.Header_Cumulative_Current_Earned_Percentage = string.Format("{0} Earned % To Date v Current", current_string);
                stats_switch.Header_Cumulative_Budgeted_Earned_Percentage = string.Format("{0} Earned % To Date v Baseline", planned_string);
                stats_switch.Header_Period_Current_Earned_Percentage = string.Format("{0} Earned % This Period v Current", current_string);
                stats_switch.Header_Period_Budgeted_Earned_Percentage = string.Format("{0} Earned % This Period v Baseline", planned_string);

                gridControlService.ClearSummary();
                string summaryPercentageString = "{0:p2}";
                string summaryDecimalString = "{0:0.00}";
                if (viewType == DashboardViewType.Costs)
                    summaryDecimalString = "{0:c2}";

                gridControlService.AddSummary("SubjobCode", SummaryItemType.Count, "Total {0} Records");
                gridControlService.AddSummary(stats_switch.Total_Current, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Total_Budgeted, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Total_Budgeted_Quantity, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Total_Current_Quantity, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_Earned_Quantity, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Period_Earned_Quantity, SummaryItemType.Sum, summaryDecimalString);

                gridControlService.AddSummary(stats_switch.PeriodEarnedVsBurned, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.CumulativeEarnedVsBurned, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.PeriodPerformanceRatio, SummaryItemType.Custom, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.CumulativePerformanceRatio, SummaryItemType.Custom, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.AdjustedRemaining, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.AdjustedDifference, SummaryItemType.Sum, summaryDecimalString);


                gridControlService.AddSummary(stats_switch.Cumulative_Current_Earned_Percentage, SummaryItemType.Custom, summaryPercentageString);
                gridControlService.AddSummary(stats_switch.Cumulative_Budgeted_Earned_Percentage, SummaryItemType.Custom, summaryPercentageString);
                gridControlService.AddSummary(stats_switch.Cumulative_Planned_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_PlannedLate_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_Current_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_Earned_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_Burned_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_Actual_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_Remaining_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Cumulative_Remaining_Quantity, SummaryItemType.Sum, summaryDecimalString);

                gridControlService.AddSummary(stats_switch.Period_Current_Earned_Percentage, SummaryItemType.Custom, summaryPercentageString);
                gridControlService.AddSummary(stats_switch.Period_Budgeted_Earned_Percentage, SummaryItemType.Custom, summaryPercentageString);
                gridControlService.AddSummary(stats_switch.Period_Planned_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Period_PlannedLate_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Period_Current_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Period_Earned_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Period_Burned_Units, SummaryItemType.Sum, summaryDecimalString);
                gridControlService.AddSummary(stats_switch.Period_Actual_Units, SummaryItemType.Sum, summaryDecimalString);

                string header_convention = "{0} {1}";
                string toDate_convention = "{0} {1} to date";
                string period_convention = "{0} {1} this period";
                string quantity_display_string = "Qty";
                string units_display_string = "Units";
                string cost_display_string = "$";
                string display_selection_string = viewType == DashboardViewType.Costs ? cost_display_string : viewType == DashboardViewType.Quantity ? quantity_display_string : units_display_string;

                stats_switch.Header_Total_Current = String.Format(header_convention, "Total " + current_string, display_selection_string);
                stats_switch.Header_Total_Budgeted = String.Format(header_convention, "Total " + planned_string, display_selection_string);
                stats_switch.Header_Budgeted_ToDate = String.Format(toDate_convention, planned_string, display_selection_string);
                stats_switch.Header_BudgetedLate_ToDate = String.Format(toDate_convention, planned_late_string, display_selection_string);
                stats_switch.Header_Current_ToDate = String.Format(toDate_convention, current_string, display_selection_string);
                stats_switch.Header_Earned_ToDate = String.Format(toDate_convention, earned_string, display_selection_string);
                stats_switch.Header_Burned_ToDate = String.Format(toDate_convention, burned_string, display_selection_string);
                stats_switch.Header_Actual_ToDate = String.Format(toDate_convention, actual_string, display_selection_string);
                stats_switch.Header_Budgeted_Period = String.Format(period_convention, planned_string, display_selection_string);
                stats_switch.Header_BudgetedLate_Period = String.Format(period_convention, planned_late_string, display_selection_string);
                stats_switch.Header_Current_Period = String.Format(period_convention, current_string, display_selection_string);
                stats_switch.Header_Earned_Period = String.Format(period_convention, earned_string, display_selection_string);
                stats_switch.Header_Burned_Period = String.Format(period_convention, burned_string, display_selection_string);
                stats_switch.Header_Actual_Period = String.Format(period_convention, actual_string, display_selection_string);

                stats_switch.Header_Remaining_ToDate = String.Format(toDate_convention, remaining_string, display_selection_string);
                stats_switch.Header_RemainingActual_ToDate = String.Format(toDate_convention, remaining_actual_string, display_selection_string);

                stats_switch.Header_Remaining_Period = String.Format(period_convention, remaining_string, display_selection_string);
                stats_switch.Header_RemainingActual_Period = String.Format(period_convention, remaining_actual_string, display_selection_string);

                stats_switch.Header_RemainingQuantity = String.Format(header_convention, remaining_string, quantity_string);
                stats_switch.Header_CumulativeEarnedVsBurned = String.Format(header_convention, "To Date Actual vs Earn", display_selection_string);
                stats_switch.Header_CumulativePerformanceRatio = String.Format(header_convention, "PF To Date", display_selection_string);
                stats_switch.Header_AdjustedRemaining = String.Format("Direct {0} adjusted by PF to date", display_selection_string);
                stats_switch.Header_AdjustedDifference = String.Format("Direct {0} PF Adjust. Vs M/Hrs to Go", display_selection_string);
                stats_switch.Header_PeriodEarnedVsBurned = String.Format(header_convention, "This Period Actual vs Earn", display_selection_string);
                stats_switch.Header_PeriodPerformanceRatio = String.Format(header_convention, "PF This Period", display_selection_string);

                stats_switch.Header_Total_Budgeted_Quantity = String.Format(header_convention, "Total " + current_string, display_selection_string);
                stats_switch.Header_Total_Current_Quantity = String.Format(header_convention, "PF This Period", display_selection_string);

                stats_switch.Header_Cumulative_Earned_Quantity = String.Format(toDate_convention, earned_string, quantity_string);
                stats_switch.Header_Period_Earned_Quantity = String.Format(period_convention, earned_string, quantity_string);
            }

            stats_switch.BarSeriesValueDataMember = viewType == DashboardViewType.Costs ? "Costs" : viewType == DashboardViewType.Quantity ? "Quantity" : "Units";
            if(usePercentage)
            {
                stats_switch.LineSeriesValueDataMember = viewType == DashboardViewType.Costs ? "CostsPercentage" : "UnitsPercentage";
                stats_switch.AxisYSecondaryLabel = viewType == DashboardViewType.Costs ? "Costs % Complete" : viewType == DashboardViewType.Quantity ? "Quantity % Complete" : "Units % Complete";
                stats_switch.AxisYSecondaryTextPattern = "{V:0%}";
                stats_switch.LineSeriesBudgetDisplayName = "Budgeted %";
                stats_switch.LineSeriesCurrentDisplayName = "Current %";
                stats_switch.LineSeriesBudgetLateDisplayName = "Budgeted Late %";
                stats_switch.LineSeriesRemainingDisplayName = "Remaining %";
                stats_switch.LineSeriesRemainingActualDisplayName = "Remaining Actual %";
                stats_switch.LineSeriesEarnedDisplayName = "Current Earned %";
                stats_switch.LineSeriesTenderEarnedDisplayName = "Budgeted Earned %";
                stats_switch.LineSeriesBurnedDisplayName = "Burned %";
                stats_switch.LineSeriesActualDisplayName = "Actual %";
                stats_switch.LineSeriesLabelPattern = "{S} - [{V:p2}]";
            }
            else
            {
                stats_switch.LineSeriesValueDataMember = viewType == DashboardViewType.Costs ? "Costs" : viewType == DashboardViewType.Quantity ? "Quantity" : "Units";
                stats_switch.AxisYSecondaryLabel = viewType == DashboardViewType.Costs ? "Cumulative Costs" : viewType == DashboardViewType.Quantity ? "Cumulative Qty" : "Cumulative Units";
                stats_switch.AxisYSecondaryTextPattern = viewType == DashboardViewType.Costs ? "{V:$0}" : "{V:0}";
                stats_switch.LineSeriesBudgetDisplayName = viewType == DashboardViewType.Costs ? "Budgeted Costs" : viewType == DashboardViewType.Quantity ? "Budgeted Qty" : "Budgeted Units";
                stats_switch.LineSeriesCurrentDisplayName = viewType == DashboardViewType.Costs ? "Current Costs" : viewType == DashboardViewType.Quantity ? "Current Qty" : "Current Units";
                stats_switch.LineSeriesBudgetLateDisplayName = viewType == DashboardViewType.Costs ? "Budgeted Late Costs" : viewType == DashboardViewType.Quantity ? "Budgeted Late Qty" : "Budgeted Late Units";
                stats_switch.LineSeriesRemainingDisplayName = viewType == DashboardViewType.Costs ? "Remaining Costs" : viewType == DashboardViewType.Quantity ? "Remaining Qty" : "Remaining Units";
                stats_switch.LineSeriesRemainingActualDisplayName = "Remaining Actual Costs";
                stats_switch.LineSeriesEarnedDisplayName = viewType == DashboardViewType.Costs ? "Current Earned Costs" : viewType == DashboardViewType.Quantity ? "Current Earned Qty" : "Current Earned Units";
                stats_switch.LineSeriesTenderEarnedDisplayName = viewType == DashboardViewType.Costs ? "Budgeted Earned Costs" : viewType == DashboardViewType.Quantity ? "Budgeted Earned Qty" : "Budgeted Earned Units";
                stats_switch.LineSeriesBurnedDisplayName = viewType == DashboardViewType.Costs ? "Burned Costs" : viewType == DashboardViewType.Quantity ? "Burned Qty" : "Burned Units";
                stats_switch.LineSeriesActualDisplayName = viewType == DashboardViewType.Costs ? "Actual Costs" : viewType == DashboardViewType.Quantity ? "Actual Qty" : "Actual Units";
                stats_switch.LineSeriesLabelPattern = "{S} - [{V:n2}]";
            }

            stats_switch.BarSeriesCrosshairPattern = viewType == DashboardViewType.Costs ? "{S} - [{V:c}]" : "{S} - [{V:n}]";
            stats_switch.AxisYPrimaryLabel = viewType == DashboardViewType.Costs ? "Costs" : viewType == DashboardViewType.Quantity ? "Quantity" :  "Units";

            stats_switch.StatsUpdate();
        }
    }

}