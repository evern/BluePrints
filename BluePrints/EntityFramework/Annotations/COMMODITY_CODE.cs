namespace BluePrints.Data
{
    using Common.ViewModel;

    public partial class COMMODITY_CODE : IHaveGUID
    {
        public override string ToString()
        {
            return FULLCODE;
        }
    }
}