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
        readonly IReportableStats reportable;
        private SingleObjectSummarizer statsSummarizer;
        public SingleObjectSummarizer StatSummarizer => statsSummarizer;

        //For bindableBase property name usage only
        public StandaloneDisplayReportable()
        {

        }

        public StandaloneDisplayReportable(IReportableStats deliverable)
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

        public decimal Estimated_Units => reportable.Estimated_Units;

        public decimal Total_Units => reportable.Total_Units;

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
                return costProjection == null ? 0 : costProjection.Total_Costs;
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
                IHaveQuantity quantityProjection = reportable as IHaveQuantity;
                return quantityProjection == null ? 0 : quantityProjection.Estimated_Quantity;
            }
        }

        public decimal Total_Quantity
        {
            get
            {
                IHaveQuantity quantityProjection = reportable as IHaveQuantity;
                return quantityProjection == null ? 0 : quantityProjection.Total_Quantity;
            }
        }

        public string UOM
        {
            get
            {
                IHaveQuantity quantityProjection = reportable as IHaveQuantity;
                return quantityProjection == null ? string.Empty : quantityProjection.UOM;
            }
        }

        public Guid EntityKey { get => reportable.EntityKey; set => reportable.EntityKey = value; }

        public decimal QuantityPerHour
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = reportable as ICanProgressByQuantity;
                return quantityDeliverable == null ? 0 : quantityDeliverable.QuantityPerHour;
            }
        }

        public decimal CurrentPeriodHours => CurrentTotalInstalledQuantity / QuantityPerHour;

        public decimal CurrentPeriodCosts => CurrentPeriodHours * ItemRate;

        public decimal PastInstalledQuantity
        {
            get
            {
                ICanProgressByQuantity quantityProjection = reportable as ICanProgressByQuantity;
                return quantityProjection == null ? 0 : quantityProjection.PastInstalledQuantity;
            }
        }

        public decimal MaxCurrentQuantity => Total_Quantity - PastInstalledQuantity;

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

        public decimal Current_Total_Percentage => throw new NotImplementedException();

        public decimal GetCurrentPeriodHours(decimal newPeriodPercentage)
        {
            return reportable.GetCurrentPeriodHours(newPeriodPercentage);
        }

        public decimal GetCurrentPeriodPercentageByQuantity(decimal newTotalQuantity)
        {
            ICanProgressByQuantity quantityDeliverable = reportable as ICanProgressByQuantity;
            if (quantityDeliverable != null)
                return quantityDeliverable.GetCurrentPeriodPercentageByQuantity(newTotalQuantity);

            return 0;
        }

        private ISortableDeliverable getSortableDeliverable()
        {
            ISortableDeliverable sortableProjection = reportable.Deliverable as ISortableDeliverable;
            if (sortableProjection != null)
                return sortableProjection;

            return null;
        }

        private ICanTrack getTrackableProjection()
        {
            ICanTrack TrackableProjection = reportable.Deliverable as ICanTrack;
            if (TrackableProjection != null)
                return TrackableProjection;

            return null;
        }

        private IHaveCosts getCostProjection()
        {
            if (reportable.Deliverable != null)
            {
                IHaveCosts costProjection = reportable.Deliverable as IHaveCosts;
                if (costProjection != null)
                    return costProjection;
            }

            return null;
        }
    }
}
