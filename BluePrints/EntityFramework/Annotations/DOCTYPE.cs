namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;

    [ConstraintAttributes("GUID_DDEPARTMENT, CODE")]
    public partial class DOCTYPE : IHaveGUID
    {
    }
}