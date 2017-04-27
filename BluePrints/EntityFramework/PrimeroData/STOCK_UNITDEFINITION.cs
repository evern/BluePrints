namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_UNITDEFINITION
    {
        [Key]
        [StringLength(10)]
        public string UNITCODE { get; set; }

        [StringLength(30)]
        public string UNITDESCRIPTION { get; set; }
    }
}