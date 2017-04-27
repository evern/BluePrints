namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;

    [ConstraintAttributes("GUID_PARENT, GUID_COMMODITYCODE")]
    public partial class COMMODITY_GROUP_DIRECT : IHaveGUID
    {
    }
}