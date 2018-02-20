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
            get => IsChartLoading;
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
            first_loaded_dispatchTimer.Interval = new TimeSpan(0, 0, 0, 1);
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
                this.SwitchBinding(false, null, GridControlService);
                SummaryEntity = MainViewModel.Entities.First();
                this.RaisePropertyChanged(x => x.SummaryEntity);
            }
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
                SummaryEntity.Stats = null;

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

        public virtual void ChangeStatsType(object checkButton)
        {
            var button = (BarCheckItem) checkButton;
            var calculationType = button.Name.ToUpper().Contains("COSTS")
                ? DashboardViewType.Costs
                : DashboardViewType.Units;
            this.SwitchBinding(calculationType == DashboardViewType.Costs, null, GridControlService);
            ChangeViewMemberFieldNames?.Invoke(calculationType);

            IHaveSummary IHaveSummary = SummaryEntity as IHaveSummary;
            if (IHaveSummary != null)
                IHaveSummary.RecalculateStats(calculationType == DashboardViewType.Costs);
        }

        public virtual void ChangeStatsPercentageType(object checkButton)
        {
            var button = (BarCheckItem)checkButton;
            bool usePercentage = button.Name.ToUpper().Contains("PERCENT");
            //pass in cost as true by default but when percentage is used isCost will be determine by what is currently used
            this.SwitchBinding(true, usePercentage, GridControlService);
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

        public bool CanViewReport()
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
        public string Cumulative_Earned_Percentage { get; set; }
        public string Cumulative_Planned_Units { get; set; }
        public string Cumulative_PlannedLate_Units { get; set; }
        public string Cumulative_Earned_Units { get; set; }
        public string Cumulative_Burned_Units { get; set; }
        public string Cumulative_Actual_Units { get; set; }
        public string Cumulative_Remaining_Units { get; set; }
        public string Period_Earned_Percentage { get; set; }
        public string Period_Planned_Units { get; set; }
        public string Period_PlannedLate_Units { get; set; }
        public string Period_Earned_Units { get; set; }
        public string Period_Burned_Units { get; set; }
        public string Period_Actual_Units { get; set; }
        public bool IsActualVisible { get; set; }
        public string Header_Total_Budgeted { get; set; }
        public string Header_Budgeted { get; set; }
        public string Header_BudgetedLate { get; set; }
        public string Header_Earned { get; set; }
        public string Header_Burned { get; set; }
        public string Header_Actual { get; set; }
        public string Cumulative_Planned_DisplayName { get; set; }
        public string Cumulative_Earned_DisplayName { get; set; }
        public string Cumulative_Burned_DisplayName { get; set; }
        public string Cumulative_Actual_DisplayName { get; set; }
        public string Periodic_Planned_DisplayName { get; set; }
        public string Periodic_Earned_DisplayName { get; set; }
        public string Periodic_Burned_DisplayName { get; set; }
        public string Periodic_Actual_DisplayName { get; set; }
        public string Summary_Display_Format { get; set; }
        public string Header_Remaining { get; set; }
        public string Header_RemainingLate { get; set; }
        public string BarSeriesValueDataMember { get; set; }
        public string LineSeriesValueDataMember { get; set; }
        public string BarSeriesCrosshairPattern { get; set; }
        public string AxisYPrimaryLabel { get; set; }
        public string AxisYSecondaryLabel { get; set; }
        public string AxisYSecondaryTextPattern { get; set; }
        public string LineSeriesBudgetDisplayName { get; set; }
        public string LineSeriesBudgetLateDisplayName { get; set; }
        public string LineSeriesRemainingDisplayName { get; set; }
        public string LineSeriesEarnedDisplayName { get; set; }
        public string LineSeriesBurnedDisplayName { get; set; }
        public string LineSeriesActualDisplayName { get; set; }
        public string LineSeriesLabelPattern { get; set; }

        public void StatsUpdate()
        {
            this.RaisePropertyChanged(x => x.Field_Mask);
            this.RaisePropertyChanged(x => x.Total_Budgeted);
            this.RaisePropertyChanged(x => x.Cumulative_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Cumulative_Planned_Units);
            this.RaisePropertyChanged(x => x.Cumulative_PlannedLate_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Earned_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Burned_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Actual_Units);
            this.RaisePropertyChanged(x => x.Cumulative_Remaining_Units);
            this.RaisePropertyChanged(x => x.Period_Earned_Percentage);
            this.RaisePropertyChanged(x => x.Period_Planned_Units);
            this.RaisePropertyChanged(x => x.Period_PlannedLate_Units);
            this.RaisePropertyChanged(x => x.Period_Earned_Units);
            this.RaisePropertyChanged(x => x.Period_Burned_Units);
            this.RaisePropertyChanged(x => x.Period_Actual_Units);
            this.RaisePropertyChanged(x => x.IsActualVisible);
            this.RaisePropertyChanged(x => x.Header_Total_Budgeted);
            this.RaisePropertyChanged(x => x.Header_Budgeted);
            this.RaisePropertyChanged(x => x.Header_BudgetedLate);
            this.RaisePropertyChanged(x => x.Header_Earned);
            this.RaisePropertyChanged(x => x.Header_Burned);
            this.RaisePropertyChanged(x => x.Header_Actual);
            this.RaisePropertyChanged(x => x.Header_Remaining);
            this.RaisePropertyChanged(x => x.Header_RemainingLate);
            this.RaisePropertyChanged(x => x.Cumulative_Planned_DisplayName);
            this.RaisePropertyChanged(x => x.Cumulative_Earned_DisplayName);
            this.RaisePropertyChanged(x => x.Cumulative_Burned_DisplayName);
            this.RaisePropertyChanged(x => x.Cumulative_Actual_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Planned_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Earned_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Burned_DisplayName);
            this.RaisePropertyChanged(x => x.Periodic_Actual_DisplayName);
            this.RaisePropertyChanged(x => x.Summary_Display_Format);
            this.RaisePropertyChanged(x => x.BarSeriesValueDataMember);
            this.RaisePropertyChanged(x => x.LineSeriesValueDataMember);
            this.RaisePropertyChanged(x => x.BarSeriesCrosshairPattern);
            this.RaisePropertyChanged(x => x.AxisYPrimaryLabel);
            this.RaisePropertyChanged(x => x.AxisYSecondaryLabel);
            this.RaisePropertyChanged(x => x.AxisYSecondaryTextPattern);
            this.RaisePropertyChanged(x => x.LineSeriesBudgetDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesBudgetLateDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesRemainingDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesEarnedDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesBurnedDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesActualDisplayName);
            this.RaisePropertyChanged(x => x.LineSeriesLabelPattern);
        }
        #endregion
    }

    public interface ISupportStatsSwitching
    {
        string Field_Mask { get; set; }
        string Total_Budgeted { get; set; }
        string Cumulative_Earned_Percentage { get; set; }
        string Cumulative_Planned_Units { get; set; }
        string Cumulative_PlannedLate_Units { get; set; }
        string Cumulative_Earned_Units { get; set; }
        string Cumulative_Burned_Units { get; set; }
        string Cumulative_Actual_Units { get; set; }
        string Cumulative_Remaining_Units { get; set; }
        string Period_Earned_Percentage { get; set; }
        string Period_Planned_Units { get; set; }
        string Period_PlannedLate_Units { get; set; }
        string Period_Earned_Units { get; set; }
        string Period_Burned_Units { get; set; }
        string Period_Actual_Units { get; set; }
        string Header_Total_Budgeted { get; set; }
        string Header_Budgeted { get; set; }
        string Header_BudgetedLate { get; set; }
        string Header_Earned { get; set; }
        string Header_Burned { get; set; }
        string Header_Actual { get; set; }
        string Header_Remaining { get; set; }
        string Header_RemainingLate { get; set; }
        bool IsActualVisible { get; set; }
        string Cumulative_Planned_DisplayName { get; set; }
        string Cumulative_Earned_DisplayName { get; set; }
        string Cumulative_Burned_DisplayName { get; set; }
        string Cumulative_Actual_DisplayName { get; set; }
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
        string LineSeriesBudgetLateDisplayName { get; set; }
        string LineSeriesRemainingDisplayName { get; set; }
        string LineSeriesEarnedDisplayName { get; set; }
        string LineSeriesBurnedDisplayName { get; set; }
        string LineSeriesActualDisplayName { get; set; }
        string LineSeriesLabelPattern { get; set; }
        void StatsUpdate();
    }

    public static class ISupportStatsSwitchingExtension
    {
        public static void SwitchBinding(this ISupportStatsSwitching stats_switch, bool isCost, bool? isPercentage, IGridControlService gridControlService)
        {
            bool usePercentage;
            if(isPercentage == null)
            {
                usePercentage = stats_switch.LineSeriesValueDataMember == null ? true : stats_switch.LineSeriesValueDataMember.ToUpper().Contains("PERCENTAGE");
            }

            else
            {
                isCost = stats_switch.Cumulative_Earned_Percentage.ToUpper().Contains("COST");
                usePercentage = (bool)isPercentage;
            }

            stats_switch.IsActualVisible = isCost ? true : false;
            stats_switch.Field_Mask = isCost ? "c" : "n";
            stats_switch.Summary_Display_Format = isCost ? "{}{0:c}" : "{}{0:n}";
            string current_period_cumulative_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}." + BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint) + ".";
            string current_period_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}." + BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodDataPoint) + ".";

            string units_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.Units);
            string cost_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs);

            string units_percentage_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.UnitsPercentage);
            string cost_percentage_string = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.Earned.CurrentPeriodCumulativeDataPoint.CostsPercentage);

            PROJECT_Dashboard dashboard = new PROJECT_Dashboard();
            string planned_string = "Planned";
            string planned_late_string = "Late Planned";
            string budgeted_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Budgeted);
            string budgeted_late_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).BudgetedLate);
            string earned_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Earned);
            string burned_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Burned);
            string actual_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Actual);
            string remaining_string = BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Remaining);
            string remaining_late_string = "Late " + BindableBase.GetPropertyName(() => ((SummaryStats)dashboard.Stats).Remaining);

            string field_selection_string = isCost ? cost_string : units_string;
            string field_percentage_selection_string = isCost ? cost_percentage_string : units_percentage_string;

            string total_budgeted_convention = BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats) + ".{0}";
            stats_switch.Total_Budgeted = isCost ? String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.TotalCosts)) : String.Format(total_budgeted_convention, BindableBase.GetPropertyName(() => new PROJECT_Dashboard().Stats.TotalUnits));

            stats_switch.Cumulative_Earned_Percentage = String.Format(current_period_cumulative_string, earned_string) + field_percentage_selection_string;
            stats_switch.Cumulative_Planned_Units = String.Format(current_period_cumulative_string, budgeted_string) + field_selection_string;
            stats_switch.Cumulative_PlannedLate_Units = String.Format(current_period_cumulative_string, budgeted_late_string) + field_selection_string;
            stats_switch.Cumulative_Earned_Units = String.Format(current_period_cumulative_string, earned_string) + field_selection_string;
            stats_switch.Cumulative_Burned_Units = String.Format(current_period_cumulative_string, burned_string) + field_selection_string;
            stats_switch.Cumulative_Actual_Units = String.Format(current_period_cumulative_string, actual_string) + field_selection_string;
            stats_switch.Cumulative_Remaining_Units = isCost ? "Stats.Remaining.Remaining_Costs": "Stats.Remaining.Remaining_Units";

            stats_switch.Period_Earned_Percentage = String.Format(current_period_string, earned_string) + field_percentage_selection_string;
            stats_switch.Period_Planned_Units = String.Format(current_period_string, budgeted_string) + field_selection_string;
            stats_switch.Period_PlannedLate_Units = String.Format(current_period_string, budgeted_late_string) + field_selection_string;
            stats_switch.Period_Earned_Units = String.Format(current_period_string, earned_string) + field_selection_string;
            stats_switch.Period_Burned_Units = String.Format(current_period_string, burned_string) + field_selection_string;
            stats_switch.Period_Actual_Units = String.Format(current_period_string, actual_string) + field_selection_string;

            gridControlService.ClearSummary();
            string summaryPercentageString = "{0:p2}";
            string summaryDecimalString = "{0:0.00}";
            if (isCost)
                summaryDecimalString = "{0:c2}";

            gridControlService.AddSummary("SubjobCode", SummaryItemType.Count, "Total {0} Records");
            gridControlService.AddSummary(stats_switch.Total_Budgeted, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Cumulative_Earned_Percentage, SummaryItemType.Custom, summaryPercentageString);
            gridControlService.AddSummary(stats_switch.Cumulative_Planned_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Cumulative_PlannedLate_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Cumulative_Earned_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Cumulative_Burned_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Cumulative_Actual_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Cumulative_Remaining_Units, SummaryItemType.Sum, summaryDecimalString);

            gridControlService.AddSummary(stats_switch.Period_Earned_Percentage, SummaryItemType.Custom, summaryPercentageString);
            gridControlService.AddSummary(stats_switch.Period_Planned_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Period_PlannedLate_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Period_Earned_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Period_Burned_Units, SummaryItemType.Sum, summaryDecimalString);
            gridControlService.AddSummary(stats_switch.Period_Actual_Units, SummaryItemType.Sum, summaryDecimalString);

            string header_convention = "{0} {1}";
            string units_display_string = "Units";
            string cost_display_string = "$";
            string display_selection_string = isCost ? cost_display_string : units_display_string;

            stats_switch.Header_Total_Budgeted = String.Format(header_convention, "Total", display_selection_string);
            stats_switch.Header_Budgeted = String.Format(header_convention, planned_string, display_selection_string);
            stats_switch.Header_BudgetedLate = String.Format(header_convention, planned_late_string, display_selection_string);
            stats_switch.Header_Earned = String.Format(header_convention, earned_string, display_selection_string);
            stats_switch.Header_Burned = String.Format(header_convention, burned_string, display_selection_string);
            stats_switch.Header_Actual = String.Format(header_convention, actual_string, display_selection_string);
            stats_switch.Header_Remaining = String.Format(header_convention, remaining_string, display_selection_string);
            stats_switch.Header_RemainingLate = String.Format(header_convention, remaining_late_string, display_selection_string);

            stats_switch.BarSeriesValueDataMember = isCost ? "Costs" : "Units";
            if(usePercentage)
            {
                stats_switch.LineSeriesValueDataMember = isCost ? "CostsPercentage" : "UnitsPercentage";
                stats_switch.AxisYSecondaryLabel = isCost ? "Costs % Complete" : "Units % Complete";
                stats_switch.AxisYSecondaryTextPattern = "{V:0%}";
                stats_switch.LineSeriesBudgetDisplayName = "Budgeted %";
                stats_switch.LineSeriesBudgetLateDisplayName = "Budgeted Late %";
                stats_switch.LineSeriesRemainingDisplayName = "Remaining %";
                stats_switch.LineSeriesEarnedDisplayName = "Earned %";
                stats_switch.LineSeriesBurnedDisplayName = "Burned %";
                stats_switch.LineSeriesActualDisplayName = "Actual %";
                stats_switch.LineSeriesLabelPattern = "{S} - [{V:p2}]";
            }
            else
            {
                stats_switch.LineSeriesValueDataMember = isCost ? "Costs" : "Units";
                stats_switch.AxisYSecondaryLabel = isCost ? "Cumulative Costs" : "Cumulative Units";
                stats_switch.AxisYSecondaryTextPattern = isCost? "{V:$0}" : "{V:0}";
                stats_switch.LineSeriesBudgetDisplayName = isCost ? "Budgeted Costs" : "Budgeted Units";
                stats_switch.LineSeriesBudgetLateDisplayName = isCost ? "Budgeted Late Costs" : "Budgeted Late Units";
                stats_switch.LineSeriesRemainingDisplayName = isCost ? "Remaining Costs" : "Remaining Units";
                stats_switch.LineSeriesEarnedDisplayName = isCost ? "Earned Costs" : "Earned Units";
                stats_switch.LineSeriesBurnedDisplayName = isCost ? "Burned Costs" : "Burned Units";
                stats_switch.LineSeriesActualDisplayName = isCost ? "Actual Costs" : "Actual Units";
                stats_switch.LineSeriesLabelPattern = "{S} - [{V:n2}]";
            }

            stats_switch.BarSeriesCrosshairPattern = isCost ? "{S} - [{V:c}]" : "{S} - [{V:n}]";
            stats_switch.AxisYPrimaryLabel = isCost ? "Costs" : "Units";

            stats_switch.StatsUpdate();
        }
    }

}