using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Grid;
using BluePrints.Common.ViewModel.Filtering;
using DevExpress.Mvvm;
using System.Linq.Expressions;
using BluePrints.Common;
using BluePrints.Data.Helpers;
using BluePrints.P6EntitiesDataModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using BluePrints.Common.ViewModel.Reporting;
using System.Windows.Threading;
using DevExpress.Xpf.Bars;
using BluePrints.ViewModels;

namespace BluePrints.Common.ViewModel
{
    public abstract class DashboardViewModelWrapper<TEntity, TProjection, TPrimaryKey, TUnitOfWork> :
        CollectionViewModelsWrapper
        <TEntity, TProjection, TPrimaryKey, TUnitOfWork,
            CollectionViewModel<TEntity, TProjection, TPrimaryKey, TUnitOfWork>>
        where TEntity : class, IHaveGUID
        where TUnitOfWork : IUnitOfWork
        where TProjection : class, IHaveGUID, IHaveStats, new()
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
            if(DisplaySelectedEntities.Count() > 0)
                DisplaySelectedEntity = DisplaySelectedEntities.First();

            OnSelectedEntityChanged(DisplaySelectedEntities);
            dispatchTimer.Stop();
        }

        public virtual TProjection SummaryEntity { get; set; }

        public void OnSelectedEntityChanged(IEnumerable<TProjection> entities)
        {
            if (!entities.Any())
                return;

            if (entities.Count() == 1)
            {
                SummaryEntity = entities.First();
            }
            else
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
            //foreach (var summaryEntity in MainViewModel.Entities)
            //    summaryEntity.RecalculateStats(calculationType == DashboardViewType.Costs);
        }

        #region P6 Affinity
        //public PROGRESS SelectionLivePROGRESS
        //{
        //    get
        //    {
        //        if (DisplaySelectedEntity == null)
        //            return null;

        //        var collection = GetEntities<PROGRESS>();
        //        if (collection == null)
        //            return null;

        //        return collection.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == DisplaySelectedEntity.GUID);
        //    }
        //}

        //public BASELINE SelectionLiveBASELINE
        //{
        //    get
        //    {
        //        if (DisplaySelectedEntity == null)
        //            return null;

        //        var collection = GetEntities<BASELINE>();
        //        if (collection == null)
        //            return null;

        //        return collection.FirstOrDefault(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == DisplaySelectedEntity.GUID);
        //    }
        //}

        public bool CanShowP6Errors()
        {
            //if (DisplaySelectedEntity == null || DisplaySelectedEntity.ReportableObjects == null)
            //    return false;
            ////if (SelectionLiveBASELINE == null || SelectionLivePROGRESS == null)
            ////    return false;

            ////if (SelectionLiveBASELINE.P6BASELINE_NAME == string.Empty && SelectionLivePROGRESS.P6PROGRESS_NAME == string.Empty)
            ////    return false;

            //if (!DisplaySelectedEntity.ReportableObjects.Any(x => x.isPlannedDataPointsFromP6))
            //    return true;

            ////Earned is always generated based on progress_item earned_date
            ////if (!DisplaySelectedEntity.ReportableObjects.Any(x => x.isEarnedDataPointsFromP6))
            ////    return true;

            //if (!DisplaySelectedEntity.ReportableObjects.Any(x => x.isRemainingDataPointsFromP6))
            //    return true;

            return false;
        }

        public void ShowP6Errors()
        {
            //DialogCollectionViewModel<PROGRESS_ITEMProjection> viewModel = DialogCollectionViewModel<PROGRESS_ITEMProjection>.Create(DisplaySelectedEntity.ReportableObjects);
            //IssuesDialogService.ShowDialog(MessageButton.OK, "P6 Affinity Report", "PrimaveraAffinityReport", viewModel);
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

            ProjectSummaryStats projectSummary = DisplaySelectedEntity as ProjectSummaryStats;
            
            if (projectSummary == null || projectSummary.ExoMissingWORKPACKS == null || projectSummary.ExoMissingWORKPACKS.Count == 0)
                return false;

            return true;
        }

        public void ShowExoErrors()
        {
            ProjectSummaryStats projectSummary = DisplaySelectedEntity as ProjectSummaryStats;
            DialogCollectionViewModel<WORKPACK> viewModel = DialogCollectionViewModel<WORKPACK>.Create(projectSummary.ExoMissingWORKPACKS);
            IssuesDialogService.ShowDialog(MessageButton.OK, "Exo Affinity Report", "ExoAffinityReport", viewModel);
        }
        #endregion
    }
}