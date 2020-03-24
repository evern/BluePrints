namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_ITEMS
    {
        [NotMapped]
        public bool IsValid { get; set; }
    }
}