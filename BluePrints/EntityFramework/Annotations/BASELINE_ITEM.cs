namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM : IHaveGUID
    {
        public decimal TOTAL_HOURS
        {
            get { return ESTIMATED_HOURS + DC_HOURS; }
        }
    }
}