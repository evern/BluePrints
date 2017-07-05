using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class ProgressDisplay : BindableBase, IGuidEntityKey, ICanUpdate
    {
        public Guid GUID
        {
            get { return ProgressItem.EntityKey; }
            set { ProgressItem.EntityKey = value; }
        }

        public Guid EntityKey { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
        public DisplayReportable ProgressItem { get; set; }

        //Need to use IHaveStockCode or else view cannot display fields
        public IEnumerable<DisplayReportable> Reportables
        {
            get
            {
                if (isSetNull)
                    return null;

                GroupDisplayReportable reportable = ProgressItem as GroupDisplayReportable;
                if (reportable != null)
                    return reportable.ChildReportables;

                return null;
            }
        }

        public bool IsExpandable
        {
            get
            {
                GroupDisplayReportable reportable = ProgressItem as GroupDisplayReportable;
                return reportable != null;
            }
        }

        public void Update()
        {
            RaisePropertyChanged(() => ProgressItem);
            RefreshChild();
        }

        bool isSetNull;
        private void RefreshChild()
        {
            isSetNull = true;
            GroupDisplayReportable reportable = ProgressItem as GroupDisplayReportable;
            if (reportable != null)
            {
                RaisePropertyChanged(() => Reportables);
                isSetNull = false;
                RaisePropertyChanged(() => Reportables);
            }
        }
    }

    public class GroupDisplayReportable : DisplayReportable
    {
        public IEnumerable<DisplayReportable> ChildReportables;
        public GroupDisplayReportable(IQuantityReportableGroup reportableGroup)
            : base(reportableGroup)
        {
            this.ChildReportables = reportableGroup.Deliverables.Select(x => new DisplayReportable(x));
        }
    }

    public class DisplayReportable : BindableBase, IReportable, ICanUpdate
    {
        readonly IDeliverable deliverable;

        //For bindableBase property name usage only
        public DisplayReportable()
        {

        }

        public DisplayReportable(IDeliverable deliverable)
        {
            this.deliverable = deliverable;
        }

        public IDeliverable Deliverable
        {
            get { return deliverable; }
        }

        public string ReportableItem_Name
        {
            get
            {
                ISortableDeliverable deliverableProjection = getSortableDeliverable();
                if (deliverableProjection != null)
                    return deliverableProjection.ReportableItem_Name;

                return string.Empty;
            }
        }

        public string Commodity_Code
        {
            get
            {
                ISortableDeliverable basicDeliverable = deliverable as ISortableDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.Commodity_Code;

                return string.Empty;
            }
        }

        public Guid? Workpack_Guid
        {
            get
            {
                ISortableDeliverable basicDeliverable = deliverable as ISortableDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.Workpack_Guid;

                return null;
            }
        }

        public Guid OriginalEntityKey
        {
            get
            {
                ISortableDeliverable basicDeliverable = deliverable as ISortableDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.OriginalEntityKey;

                throw new NotImplementedException();
            }
            set
            {
                ISortableDeliverable basicDeliverable = deliverable as ISortableDeliverable;
                if (basicDeliverable != null)
                {
                    basicDeliverable.OriginalEntityKey = value;
                    return;
                }


                throw new NotImplementedException();
            }
        }

        public string Stock_Code => deliverable.Stock_Code;

        public Guid? Area_Guid => deliverable.Area_Guid;

        public Guid? SubArea_Guid => deliverable.SubArea_Guid;

        public decimal TotalHoursIncludeByDuration => deliverable.TotalHoursIncludeByDuration;

        public decimal EstimatedHours => deliverable.EstimatedHours;

        public decimal TotalHours => deliverable.TotalHours;

        public decimal ItemRate
        {
            get
            {
                IHaveCosts costProjection = getCostProjection();
                return costProjection == null ? 0 : costProjection.ItemRate;
            }
        }

        public decimal EstimatedCosts
        {
            get
            {
                IHaveCosts costProjection = getCostProjection();
                return costProjection == null ? 0 : costProjection.EstimatedCosts;
            }
        }

        public decimal TotalCosts
        {
            get
            {
                IHaveCosts costProjection = getCostProjection();
                return costProjection == null ? 0 : costProjection.TotalCosts;
            }
        }

        public bool? Track
        {
            get
            {
                ICanTrack trackableProjection = getTrackableProjection();
                return trackableProjection == null ? null : trackableProjection.Track;
            }
        }

        private ISortableDeliverable getSortableDeliverable()
        {
            IDeliverable deliverableProjection = getDeliverableProjection();
            if (deliverableProjection != null)
            {
                ISortableDeliverable sortableProjection = deliverableProjection as ISortableDeliverable;
                if (sortableProjection != null)
                    return sortableProjection;
            }

            return null;
        }

        private ICanTrack getTrackableProjection()
        {
            IDeliverable deliverableProjection = getDeliverableProjection();
            if (deliverableProjection != null)
            {
                ICanTrack TrackableProjection = deliverableProjection as ICanTrack;
                if (TrackableProjection != null)
                    return TrackableProjection;
            }

            return null;
        }

        private IHaveCosts getCostProjection()
        {
            IDeliverable deliverableProjection = getDeliverableProjection();
            if (deliverableProjection != null)
            {
                IHaveCosts costProjection = deliverableProjection as IHaveCosts;
                if (costProjection != null)
                    return costProjection;
            }

            return null;
        }

        private IDeliverable getDeliverableProjection()
        {
            IQuantityReportableGroup groupProjection = deliverable as IQuantityReportableGroup;
            if (groupProjection != null)
                return groupProjection.Deliverable;
            else
            {
                IQuantityReportable reportableProjection = deliverable as IQuantityReportable;
                if (reportableProjection != null)
                    return reportableProjection.Deliverable;
            }

            return null;
        }

        public decimal Estimated_Quantity
        {
            get
            {
                IHaveQuantity quantityDeliverable = deliverable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.Estimated_Quantity;

                return 0;
            }
        }

        public decimal Total_Quantity
        {
            get
            {
                IHaveQuantity quantityDeliverable = deliverable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.Total_Quantity;

                return 0;
            }
        }

        public string UOM
        {
            get
            {
                IHaveQuantity quantityDeliverable = deliverable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.UOM;

                return string.Empty;
            }
        }

        public Guid EntityKey { get => deliverable.EntityKey; set => deliverable.EntityKey = value; }

        public decimal QuantityPerHour
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.QuantityPerHour;

                return 0;
            }
        }

        public decimal CurrentPeriodHours
        {
            get
            {
                return QuantityPerHour * CurrentTotalInstalledQuantity;
            }
        }

        public decimal CurrentPeriodCosts
        {
            get
            {
                return CurrentPeriodHours * ItemRate;
            }
        }

        public decimal TotalPercentage
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.TotalPercentage;

                return 0;
            }
        }

        public decimal PastInstalledQuantity
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.PastInstalledQuantity;

                return 0;
            }
        }

        decimal? currentTotalInstalledQuantity { get; set; }
        public decimal CurrentTotalInstalledQuantity
        {
            get { return getActualCurrentTotalInstalledQuantity(); }
            set { currentTotalInstalledQuantity = value; }
        }

        public decimal getActualCurrentTotalInstalledQuantity()
        {
            if(currentTotalInstalledQuantity == null)
            {
                ICanProgressByQuantity quantityProjection = deliverable as ICanProgressByQuantity;
                if (quantityProjection != null)
                    currentTotalInstalledQuantity = quantityProjection.CurrentTotalInstalledQuantity;
                else
                    currentTotalInstalledQuantity = 0;
            }

            return (decimal)currentTotalInstalledQuantity;
        }

        public decimal TotalInstalledQuantity
        {
            get
            {
                return PastInstalledQuantity + CurrentTotalInstalledQuantity;
            }
        }

        public DateTime ReportingDataDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<PROGRESS_ITEM> PROGRESS_ITEMS { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate
        {
            get
            {
                IReportable reportable = deliverable as IReportable;
                if (reportable != null)
                    return reportable.PROGRESS_ITEM_BeforeDataDate;

                return new List<PROGRESS_ITEM>();
            }
        }

        public PROGRESS_ITEM PROGRESS_ITEM_Current
        {
            get
            {
                IReportable reportable = deliverable as IReportable;
                if (reportable != null)
                    return reportable.PROGRESS_ITEM_Current;

                return null;
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate
        {
            get
            {
                IReportable reportable = deliverable as IReportable;
                if (reportable != null)
                    return reportable.PROGRESS_ITEM_UpToCurrentDataDate;

                return new List<PROGRESS_ITEM>();
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate
        {
            get
            {
                IReportable reportable = deliverable as IReportable;
                if (reportable != null)
                    return reportable.PROGRESS_ITEM_AfterDataDate;

                return new List<PROGRESS_ITEM>();
            }
        }

        public ProgressStats Stats
        {
            get
            {
                IReportable reportable = deliverable as IReportable;
                if (reportable != null)
                    return reportable.Stats;

                return null;
            }
            set
            {
                IReportable reportable = deliverable as IReportable;
                if (reportable != null)
                    reportable.Stats = value;

            }
        }

        public decimal GetCurrentPeriodHours(decimal newPeriodPercentage)
        {
            ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
            if (quantityDeliverable != null)
                return quantityDeliverable.GetCurrentPeriodHours(newPeriodPercentage);

            return 0;
        }

        public decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
            if (quantityDeliverable != null)
                return quantityDeliverable.GetCurrentPeriodPercentage(newTotalQuantity);

            return 0;
        }

        public void Update()
        {
            GroupDisplayReportable groupReportable = this as GroupDisplayReportable;
            if (groupReportable != null)
            {
                foreach (DisplayReportable reportable in groupReportable.ChildReportables)
                {
                    reportable.Update();
                }
            }

            currentTotalInstalledQuantity = getActualCurrentTotalInstalledQuantity();
            RaisePropertyChanged(() => CurrentTotalInstalledQuantity);
        }
    }

    public class Stock_CodeProgress : BluePrintsProgressableByQuantityProjectionBase<STOCK_CODEProjection>, IQuantityReportableGroup
    {
        public IEnumerable<IQuantityReportable> Deliverables { get; set; }

        public override List<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get { return Deliverables.SelectMany(x => x.PROGRESS_ITEMS).ToList(); }
            set { }
        }

        public override decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            return base.GetCurrentPeriodPercentage(newTotalQuantity);
        }
    }

    public class Estimation_Direct_ItemProgress : BluePrintsProgressableByQuantityProjectionBase<ESTIMATION_DIRECT_ITEMProjection>
    {

    }

    public abstract class BluePrintsProgressableByQuantityProjectionBase<TEntity> : BluePrintsProgressableProjectionBase<TEntity>, IQuantityReportable
        where TEntity : class, IQuantityDeliverableProjection, ICanUpdate, new()
    {
        public decimal QuantityPerHour
        {
            get
            {
                if (TotalHours == 0)
                    return 0;

                return Total_Quantity / TotalHours;
            }
        }

        public decimal TotalPercentage
        {
            get
            {
                if (PROGRESS_ITEM_UpToCurrentDataDate.Count() == 0)
                    return 0;

                return PROGRESS_ITEM_UpToCurrentDataDate.Sum(x => x.EARNED_UNITS) / Entity.TotalHours;
            }
        }

        public decimal CurrentTotalInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_UpToCurrentDataDate.Count() == 0 || QuantityPerHour == 0)
                    return 0;

                return PROGRESS_ITEM_UpToCurrentDataDate.Sum(x => x.EARNED_UNITS) * QuantityPerHour;
            }
            set
            {
                //dummy set
            }
        }

        public decimal PastInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_BeforeDataDate.Count() == 0 || QuantityPerHour == 0)
                    return 0;

                return PROGRESS_ITEM_BeforeDataDate.Sum(x => x.EARNED_UNITS) / QuantityPerHour;
            }
        }

        public decimal Estimated_Quantity => Entity.Estimated_Quantity;

        public decimal Total_Quantity
        {
            get
            {
                IQuantityDeliverableGroupProjection quantityGroup = Entity as IQuantityDeliverableGroupProjection;
                if (quantityGroup != null)
                {
                    return quantityGroup.Reportables.Where(x => (bool)x.Track).Sum(x => x.TotalHours);
                }
                else
                    return Entity.TotalHours;
            }
        }

        public string UOM => Entity.UOM;

        public bool? Track
        {
            get
            {
                ICanTrack trackableEntity = Entity as ICanTrack;
                if (trackableEntity != null)
                    return trackableEntity.Track;

                return null;
            }
        }

        public virtual decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            if (Total_Quantity == 0)
                return 0;

            return (newTotalQuantity - PastInstalledQuantity) / Total_Quantity;
        }

        public decimal GetCurrentPeriodHours(decimal currentPeriodPercentage)
        {
            return currentPeriodPercentage * TotalHours;
        }
    }

    public abstract class BluePrintsProgressableProjectionBase<TEntity> : BluePrintsProjectionBase<TEntity>, IReportable
        where TEntity : class, ISortableDeliverableProjection, ICanUpdate, new()
    {
        public IDeliverable Deliverable => Entity;

        public void SetPROGRESS_ITEMS(IEnumerable<PROGRESS_ITEM> progress_items)
        {
            PROGRESS_ITEMS = progress_items.ToList();
        }

        public void SetReportingDataDate(DateTime reportingDataDate)
        {
            ReportingDataDate = reportingDataDate;
        }

        public void Update()
        {
            IQuantityDeliverableGroupProjection quantityGroup = Entity as IQuantityDeliverableGroupProjection;
            if(quantityGroup != null)
            {
                foreach(IQuantityReportable reportables in quantityGroup.Reportables)
                {
                    reportables.Update();
                }
            }

            Entity.Update();
            RaisePropertiesChanged();
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate
        {
            get
            {
                return PROGRESS_ITEMS.Where(y => y.EARNED_DATE < ReportingDataDate);
            }
        }

        public PROGRESS_ITEM PROGRESS_ITEM_Current
        {
            get
            {
                ISortableDeliverable reportableItem = Entity as ISortableDeliverable;
                if(reportableItem != null)
                    return PROGRESS_ITEMS.FirstOrDefault(y => y.GUID_ORIBASEITEM == reportableItem.OriginalEntityKey && y.EARNED_DATE == ReportingDataDate);

                return null;
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate
        {
            get
            {
                return PROGRESS_ITEMS.Where(y => y.EARNED_DATE <= ReportingDataDate);
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate
        {
            get
            {
                return PROGRESS_ITEMS.Where(y => y.EARNED_DATE > ReportingDataDate);
            }
        }

        public DateTime ReportingDataDate { get; set; }

        public virtual List<PROGRESS_ITEM> PROGRESS_ITEMS { get; set; }

        public ProgressStats Stats { get; set; }

        public string Stock_Code
        {
            get { return Entity.Stock_Code; }
        }

        public decimal TotalHoursIncludeByDuration => Entity.TotalHoursIncludeByDuration;

        public decimal EstimatedHours => Entity.EstimatedHours;

        public decimal TotalHours => Entity.TotalHours;

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public string ReportableItem_Name => Entity.ReportableItem_Name;

        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Workpack_Guid => Entity.Workpack_Guid;

        public Guid OriginalEntityKey { get => Entity.OriginalEntityKey; set => Entity.OriginalEntityKey = value; }

        public decimal ItemRate => Entity.ItemRate;

        public decimal EstimatedCosts => Entity.EstimatedCosts;

        public decimal TotalCosts => Entity.TotalCosts;
    }

    public static class ProgressItemQueries
    {
        public static IQueryable<ProgressDisplay> SiteDirectProgressItemTransformation(
            IQueryable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<ESTIMATION_DIRECT_ITEM> projectESTIMATION_DIRECT_ITEMS, IEnumerable<RATE> projectRATES, IEnumerable<COMMODITY_CODE> projectCOMMODITY_CODES, DateTime reportingDataDate)
        {
            IEnumerable<PROGRESS_ITEM> arrPROGRESS_ITEMS = PROGRESS_ITEMS.ToArray();
            List<ProgressDisplay> progressItems = new List<ProgressDisplay>();
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });
            
            IEnumerable<ESTIMATION_DIRECT_ITEMProjection> ESTIMATION_DIRECT_ITEMProjection = 
                ESTIMATION_DIRECT_ITEMProjectionQueries.ESTIMATION_DIRECT_ITEMProjectionQuery(projectESTIMATION_DIRECT_ITEMS.AsQueryable(), 
                                                                                                projectRATES, 
                                                                                                projectCOMMODITY_CODES, 
                                                                                                projectSTOCK_CODES).AsEnumerable();

            List<Estimation_Direct_ItemProgress> estimationDirectItemProgress = new List<Estimation_Direct_ItemProgress>();
            foreach (ESTIMATION_DIRECT_ITEMProjection ESTIMATION_DIRECT_ITEM in ESTIMATION_DIRECT_ITEMProjection)
            {
                Estimation_Direct_ItemProgress newEstimation_Direct_itemProgress = new Estimation_Direct_ItemProgress();
                newEstimation_Direct_itemProgress.Entity = ESTIMATION_DIRECT_ITEM;
                newEstimation_Direct_itemProgress.SetReportingDataDate(reportingDataDate);
                SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                estimationDirectItemProgress.Add(newEstimation_Direct_itemProgress);
            }

            var estimationDirectProgressByStockCode = estimationDirectItemProgress.Where(x => !x.Entity.Entity.STANDALONE)
                .GroupBy(x => x.Entity.Entity.GUID_STOCK_CODE).Select(group => new { StockCodeGuid = group.Key, Estimation_Direct_ItemProgress = group.ToList() });

            foreach (STOCK_CODE STOCK_CODE in projectSTOCK_CODES)
            {
                Stock_CodeProgress newStock_CodeProgress = new Stock_CodeProgress();
                newStock_CodeProgress.Entity.Entity = STOCK_CODE;
                
                var currentStockCodeProgresses = estimationDirectProgressByStockCode.FirstOrDefault(x => x.StockCodeGuid == STOCK_CODE.GUID);
                if(currentStockCodeProgresses != null)
                {
                    newStock_CodeProgress.Entity.Reportables = currentStockCodeProgresses.Estimation_Direct_ItemProgress;
                    newStock_CodeProgress.Deliverables = currentStockCodeProgresses.Estimation_Direct_ItemProgress.ToList();
                    newStock_CodeProgress.SetReportingDataDate(reportingDataDate);
                    ProgressDisplay newProgressDisplay = new ProgressDisplay();
                    newProgressDisplay.ProgressItem = new GroupDisplayReportable(newStock_CodeProgress);
                    progressItems.Add(newProgressDisplay);
                }
            }

            progressItems.AddRange(estimationDirectItemProgress.Where(x => x.Entity.Entity.STANDALONE).Select(x => new ProgressDisplay() { ProgressItem = new DisplayReportable(x) }));

            return progressItems.AsQueryable();
        }

        private static void SetReportablePROGRESS_ITEM(IReportable reportable, IQueryable<dynamic> PROGRESS_ITEMSByOriginalGuid)
        {
            foreach(dynamic item in PROGRESS_ITEMSByOriginalGuid)
            {
                ISortableDeliverable basicDeliverable = reportable.Deliverable as ISortableDeliverable;
                if (basicDeliverable == null)
                    break;

                if (item.OriginalGuid == basicDeliverable.OriginalEntityKey)
                {
                    reportable.PROGRESS_ITEMS = item.Progresses;
                    return;
                }
            }

            reportable.PROGRESS_ITEMS = new List<PROGRESS_ITEM>();
        }
    }
}