using BaseModel.Misc;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ProgressDisplay : BindableBase, IGuidEntityKey, ICanUpdate
    {
        public Guid GUID { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
        public Guid EntityKey { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
        public DisplayReportable ProgressItem { get; set; }

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
        }

        public void SetOriginalEntityKey(Guid newGuid) { }

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
                return CurrentTotalInstalledQuantity / QuantityPerHour;
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

        public decimal MaxCurrentQuantity
        {
            get
            {
                return Total_Quantity - PastInstalledQuantity;
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
            if (currentTotalInstalledQuantity == null)
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
}
