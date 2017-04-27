namespace BluePrints.Data
{
    using Common.ViewModel;

    public partial class ESTIMATION_DIRECT_ITEM : IHaveGUID
    {
        public ESTIMATION_DIRECT_ITEM()
        {
            ESTIMATED_QUANTITY = 1;
        }

        public decimal TOTAL_QUANTITY
        {
            get { return ESTIMATED_QUANTITY + VAR_QUANTITY; }
        }
    }
}