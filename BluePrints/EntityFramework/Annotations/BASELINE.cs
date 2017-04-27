namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;

    [ConstraintAttributes("REVISION")]
    [BulkEditDisabledAttributes("P6BASELINE_NAME, P6MODBASELINE_NAME")]
    public partial class BASELINE : IHaveGUID
    {
    }
}