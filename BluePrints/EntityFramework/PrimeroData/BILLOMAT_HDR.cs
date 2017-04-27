namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class BILLOMAT_HDR
    {
        [Key]
        [StringLength(23)]
        public string BILLCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? SELLPRICE1 { get; set; }

        public double? SELLPRICE2 { get; set; }

        public double? SELLPRICE3 { get; set; }

        public double? SELLPRICE4 { get; set; }

        public double? COSTPRICE { get; set; }

        public int? PRICING_MODE { get; set; }

        [StringLength(23)]
        public string OUTPUT_CODE { get; set; }

        public double? BATCH_QTY { get; set; }

        [StringLength(1)]
        public string HIDE_LINES { get; set; }

        public double? WASTAGE { get; set; }

        [StringLength(2048)]
        public string NOTES { get; set; }

        public double? SELLPRICE5 { get; set; }

        public double? SELLPRICE6 { get; set; }

        public double? SELLPRICE7 { get; set; }

        public double? SELLPRICE8 { get; set; }

        public double? SELLPRICE9 { get; set; }

        public double? SELLPRICE10 { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public int KIT_TYPE { get; set; }

        [Required]
        [StringLength(1)]
        public string BOMTYPE { get; set; }

        [Required]
        [StringLength(1)]
        public string AUTOBUILD { get; set; }
    }
}