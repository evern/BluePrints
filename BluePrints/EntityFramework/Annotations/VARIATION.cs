namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;

    [ConstraintAttributes("GUID_PROJECT, NAME")]
    public partial class VARIATION : IHaveGUID
    {
    }
}