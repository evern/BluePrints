using System;

namespace BluePrints.Common.ViewModel.Reporting
{
    /// <summary>
    /// Period to store date ranges
    /// </summary>
    public class Period
    {
        public Period(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Store variation with deliverable original guid from project to be rolled down
    /// </summary>
    public class VariationAdjustment
    {
        public DateTime AdjustmentDate { get; set; }
        public decimal AdjustmentUnits { get; set; }
        public decimal? AdjustmentRate { get; set; }

        public readonly Guid DeliverableOriginalGuid;

        public VariationAdjustment(Guid deliverableOriginalGuid)
        {
            DeliverableOriginalGuid = deliverableOriginalGuid;
        }

        public decimal AdjustmentNativeCosts
        {
            get { return AdjustmentUnits * AdjustmentRate == null ? 0 : (decimal)AdjustmentRate; }
        }

        public decimal AdjustmentCumulativeCosts { get; set; }
    }
}