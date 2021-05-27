namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_ITEMS : EntityBase
    {
        [NotMapped]
        public bool IsValid { get; set; }
    }
}