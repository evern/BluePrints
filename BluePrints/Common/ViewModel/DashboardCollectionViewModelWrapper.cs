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

namespace BluePrints.Common.ViewModel
{
    public abstract class DashboardViewModelWrapper<TEntity, TProjection, TPrimaryKey, TUnitOfWork> :
        CollectionViewModelsWrapper
        <TEntity, TProjection, TPrimaryKey, TUnitOfWork,
            CollectionViewModel<TEntity, TProjection, TPrimaryKey, TUnitOfWork>>
        where TEntity : class, IHaveGUID
        where TUnitOfWork : IUnitOfWork
        where TProjection : SummarizableObject, IHaveGUID, new()
    {
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> UnitOfWorkFactory;
        private DispatcherTimer dispatchTimer;

        public DashboardViewModelWrapper()
        {
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
            OnSelectedEntityChanged(DisplaySelectedEntities);
            dispatchTimer.Stop();
        }

        public virtual TProjection SummaryEntity { get; set; }

        public void OnSelectedEntityChanged(IEnumerable<TProjection> entities)
        {
            if (!entities.Any())
                if (MainViewModel != null && MainViewModel.Entities.Count() > 0)
                    entities = MainViewModel.Entities;
                else
                    return;

            if (entities.Count() == 1)
            {
                SummaryEntity = entities.First();
            }
            else
            {
                var newEntity = ViewModelSource.Create(() => new TProjection());
                var earliestDataDate = entities.Min(x => x.ReportingDataDate);
                var earliestLiveProgress =
                    entities.First(x => x.LivePROGRESS.DATA_DATE == earliestDataDate).LivePROGRESS;
                newEntity.LivePROGRESS = earliestLiveProgress;
                newEntity.IntervalPeriod =
                    ISupportProgressReportingExtensions.ConvertProgressIntervalToPeriod(earliestLiveProgress);
                newEntity.ReportableObjects = entities.SelectMany(x => x.ReportableObjects);
                newEntity.FirstAlignedDataDate = entities.Min(x => x.FirstAlignedDataDate);
                newEntity.ReportingDataDate = earliestLiveProgress.DATA_DATE;
                newEntity.NonCumulative_VariationAdjustments =
                    new ObservableCollection<VariationAdjustment>(
                        entities.SelectMany(x => x.NonCumulative_VariationAdjustments));
                newEntity.NonCumulative_ActualDataPoints =
                    new ObservableCollection<ProgressInfo>(entities.SelectMany(x => x.NonCumulative_ActualDataPoints));
                newEntity.NonCumulative_BurnedDataPoints =
                    new ObservableCollection<ProgressInfo>(entities.SelectMany(x => x.NonCumulative_BurnedDataPoints));
                newEntity.NonCumulative_EarnedDataPoints =
                    new ObservableCollection<ProgressInfo>(entities.SelectMany(x => x.NonCumulative_EarnedDataPoints));
                newEntity.NonCumulative_OriginalDataPoints =
                    new ObservableCollection<ProgressInfo>(entities.SelectMany(x => x.NonCumulative_OriginalDataPoints));
                newEntity.NonCumulative_PlannedDataPoints =
                    new ObservableCollection<ProgressInfo>(entities.SelectMany(x => x.NonCumulative_PlannedDataPoints));
                newEntity.NonCumulative_RemainingPlannedDataPoints =
                    new ObservableCollection<ProgressInfo>(
                        entities.SelectMany(x => x.NonCumulative_RemainingPlannedDataPoints));
                ISupportProgressReportingExtensions.GenerateCumulativeSummaryDataPoints(newEntity);
                SummaryEntity = newEntity;
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
            if (ChangeViewMemberFieldNames != null)
                ChangeViewMemberFieldNames(calculationType);

            foreach (var summaryEntity in MainViewModel.Entities)
                summaryEntity.RecalculateStats(calculationType == DashboardViewType.Costs);
        }
    }
}