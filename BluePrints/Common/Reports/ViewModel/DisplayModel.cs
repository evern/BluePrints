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
    public class ProgressDisplay : BindableBase, IGuidEntityKey
    {
        public Guid GUID { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
        public Guid EntityKey { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
        public StandaloneDisplayReportable ProgressItem { get; set; }

        public IEnumerable<StandaloneDisplayReportable> Reportables
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

    public class GroupDisplayReportable : StandaloneDisplayReportable
    {
        public IEnumerable<StandaloneDisplayReportable> ChildReportables;
        public GroupDisplayReportable(IQuantityReportableGroup reportableGroup)
            : base(reportableGroup)
        {
            this.ChildReportables = reportableGroup.Deliverables.Select(x => new StandaloneDisplayReportable(x));
        }
    }

    public class StandaloneDisplayReportable : BindableBase, IReportable
    {
        readonly IReportable reportable;

        //For bindableBase property name usage only
        public StandaloneDisplayReportable()
        {

        }

        public StandaloneDisplayReportable(IReportable deliverable)
        {
            this.reportable = deliverable;
        }

        public IDeliverable Deliverable
        {
            get { return reportable.Deliverable; }
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
                return reportable.Commodity_Code;
            }
        }

        public Guid? Workpack_Guid
        {
            get
            {
                ISortableDeliverable basicDeliverable = reportable as ISortableDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.Workpack_Guid;

                return null;
            }
        }

        public Guid OriginalEntityKey
        {
            get
            {
                ISortableDeliverable basicDeliverable = reportable as ISortableDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.OriginalEntityKey;

                throw new NotImplementedException();
            }
        }

        public void SetOriginalEntityKey(Guid newGuid) { }

        public Guid? Area_Guid => reportable.Area_Guid;

        public Guid? SubArea_Guid => reportable.SubArea_Guid;

        public decimal TotalUnitsIncludeByDuration => reportable.TotalUnitsIncludeByDuration;

        public decimal EstimatedUnits => reportable.EstimatedUnits;

        public decimal TotalUnits => reportable.TotalUnits;

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
                IHaveQuantity quantityDeliverable = reportable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.Estimated_Quantity;

                return 0;
            }
        }

        public decimal Total_Quantity
        {
            get
            {
                IHaveQuantity quantityDeliverable = reportable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.Total_Quantity;

                return 0;
            }
        }

        public string UOM
        {
            get
            {
                IHaveQuantity quantityDeliverable = reportable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.UOM;

                return string.Empty;
            }
        }

        public Guid EntityKey { get => reportable.EntityKey; set => reportable.EntityKey = value; }

        public decimal QuantityPerHour
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = reportable as ICanProgressByQuantity;
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
                ICanProgressByQuantity quantityDeliverable = reportable as ICanProgressByQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.TotalPercentage;

                return 0;
            }
        }

        public decimal PastInstalledQuantity
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = reportable as ICanProgressByQuantity;
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
                ICanProgressByQuantity quantityProjection = reportable as ICanProgressByQuantity;
                if (quantityProjection != null)
                    currentTotalInstalledQuantity = quantityProjection.CurrentTotalInstalledQuantity;
                else
                    currentTotalInstalledQuantity = 0;
            }

            return (decimal)currentTotalInstalledQuantity;
        }

        public decimal TotalInstalledQuantity => PastInstalledQuantity + CurrentTotalInstalledQuantity;

        public DateTime ReportingDataDate => reportable.ReportingDataDate;

        public List<PROGRESS_ITEM> PROGRESS_ITEMS => reportable.PROGRESS_ITEMS;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => reportable.PROGRESS_ITEM_BeforeDataDate;

        public PROGRESS_ITEM PROGRESS_ITEM_Current => reportable.PROGRESS_ITEM_Current;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => reportable.PROGRESS_ITEM_UpToCurrentDataDate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => reportable.PROGRESS_ITEM_AfterDataDate;

        public ProgressStats Stats { get => reportable.Stats; set => reportable.Stats = value; }

        public decimal VariationUnits => reportable.VariationUnits;

        public decimal GetCurrentPeriodHours(decimal newPeriodPercentage)
        {
            ICanProgressByQuantity quantityDeliverable = reportable as ICanProgressByQuantity;
            if (quantityDeliverable != null)
                return quantityDeliverable.GetCurrentPeriodHours(newPeriodPercentage);

            return 0;
        }

        public decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            ICanProgressByQuantity quantityDeliverable = reportable as ICanProgressByQuantity;
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
            IQuantityReportableGroup groupProjection = reportable as IQuantityReportableGroup;
            if (groupProjection != null)
                return groupProjection.Deliverable;
            else
            {
                IQuantityReportable reportableProjection = reportable as IQuantityReportable;
                if (reportableProjection != null)
                    return reportableProjection.Deliverable;
            }

            return null;
        }
    }
}
