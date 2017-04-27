namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_REQUESTTYPES
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(40)]
        public string DISPLAY_NAME { get; set; }
    }
}