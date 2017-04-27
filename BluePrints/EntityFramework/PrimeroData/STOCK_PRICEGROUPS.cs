namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_PRICEGROUPS
    {
        [Key]
        public int GROUPNO { get; set; }

        [StringLength(30)]
        public string GROUPNAME { get; set; }
    }
}