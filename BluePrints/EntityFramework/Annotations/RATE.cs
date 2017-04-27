namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;

    [ConstraintAttributes("GUID_DEPARTMENT, GUID_DISCIPLINE")]
    public partial class RATE : IHaveGUID
    {
    }
}