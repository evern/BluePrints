namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class SU_WARRANTYTYPE
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(50)]
        public string WARRANTYDESC { get; set; }

        public int? EXPIRYDAYS { get; set; }

        public int? PARTSEXPIRY { get; set; }

        public int? LABOUREXPIRY { get; set; }

        [StringLength(1)]
        public string SUPPLIERLIABILITY { get; set; }

        [StringLength(1)]
        public string FWDREPLACEMENT { get; set; }

        [StringLength(23)]
        public string CHARGE_STOCKCODE { get; set; }

        public int? STOCKGROUP { get; set; }

        public int? STOCKGROUP2 { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }
    }
}