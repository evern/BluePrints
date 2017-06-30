using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace BluePrints.Common.ViewModel
{
    public abstract class DashboardViewModelWrapper<TEntity, TProjection, TPrimaryKey, TUnitOfWork> :
        BluePrintsEntitiesCollectionWrapper
        <TEntity, TProjection, TPrimaryKey, TUnitOfWork>
        where TEntity : class, IGuidEntityKey, new()
        where TUnitOfWork : IUnitOfWork
        where TProjection : class, IGuidEntityKey, IHaveStats, new()
    {
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> UnitOfWorkFactory;
        private DispatcherTimer dispatchTimer;

        public DashboardViewModelWrapper()
        {
            DoNotAutoRefresh = true;
            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<TProjection> entities)
        {
            //because dashboards are generally heavy we just want manual refreshes to take place
            MainViewModel.ManualUnregisterMessageHandler();
            OnSelectedEntitiesChangedCallBack = DisplaySelectedEntities_CollectionChanged;
            return base.OnMainViewModelLoaded(entities);
        }

        protected void DisplaySelectedEntities_CollectionChanged()
        {
            dispatchTimer.Tick -= dispatchTimer_Tick;
            dispatchTimer.Tick += dispatchTimer_Tick;
            dispatchTimer.Start();
        }

        private void dispatchTimer_Tick(object sender, EventArgs e)
        {
            dispatchTimer.Stop();
            if (DisplaySelectedEntities.Count() > 0)
                DisplaySelectedEntity = DisplaySelectedEntities.First();

            OnSelectedEntitiesChanged(DisplaySelectedEntities);
        }

        public virtual TProjection SummaryEntity { get; set; }
        protected bool isMasterDetailView { get; set; }

        public void OnSelectedEntitiesChanged(IEnumerable<TProjection> entities)
        {
            if (MainViewModel == null)
                return;

            if (!entities.Any())
            {
                if(MainViewModel.Entities.Count > 0)
                {
                    List<TProjection> firstEntity = new List<TProjection>();
                    firstEntity.Add(MainViewModel.Entities.First());
                    entities = firstEntity;
                }
            }

            //Cannot use this anymore because during master detail view the stats will be recalculated when detail entities are selected
            if (entities.Count() == 1 && !isMasterDetailView)
            {
                SummaryEntity = entities.First();
            }
            else if (entities.Count() > 0)
            {
                SummaryEntity = ViewModelSource.Create(() => new TProjection());
                ProgressStats progressStats = entities.First().Stats as ProgressStats;
                SummaryStats summaryStats = entities.First().Stats as SummaryStats;

                if(summaryStats != null)
                {
                    IEnumerable<SummaryStats> entitiesSummary = entities.Select(x => (SummaryStats)x.Stats);
                    SummaryEntity.Stats = new SummaryStats(entitiesSummary);
                }
                else if(progressStats != null)
                {
                    IEnumerable<ProgressStats> entitiesSummary = entities.Select(x => (ProgressStats)x.Stats);
                    SummaryEntity.Stats = new ProgressStats(entitiesSummary);
                }
            }

            this.RaisePropertyChanged(x => x.SummaryEntity);
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
            ChangeViewMemberFieldNames?.Invoke(calculationType);

            IHaveSummary IHaveSummary = SummaryEntity as IHaveSummary;
            if (IHaveSummary != null)
                IHaveSummary.RecalculateStats(calculationType == DashboardViewType.Costs);
        }

        public override void OnAfterAffectingEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
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
        private IDialogService IssuesDialogService
        {
            get { return this.GetRequiredService<IDialogService>("IssuesDialogService"); }
        }

        public bool CanShowExoErrors()
        {
            if (DisplaySelectedEntity == null)
                return false;

            ProjectSummaryStats projectSummary = DisplaySelectedEntity.Stats as ProjectSummaryStats;
            if (projectSummary == null || projectSummary.ExoMissingWORKPACKS == null || projectSummary.ExoMissingWORKPACKS.Count == 0)
                return false;

            return true;
        }

        public void ShowExoErrors()
        {
            ProjectSummaryStats projectSummary = DisplaySelectedEntity.Stats as ProjectSummaryStats;
            if (projectSummary == null || projectSummary.ExoMissingWORKPACKS == null || projectSummary.ExoMissingWORKPACKS.Count == 0)
                return;

            DialogCollectionViewModel<WORKPACK> viewModel = DialogCollectionViewModel<WORKPACK>.Create(projectSummary.ExoMissingWORKPACKS);
            IssuesDialogService.ShowDialog(MessageButton.OK, "Exo Affinity Report", "ExoAffinityReport", viewModel);
        }
        #endregion
    }
}