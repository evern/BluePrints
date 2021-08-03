namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class X_PURCHORD_LINE
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [StringLength(15)]
        public string SUBJOB_CODE { get; set; }

        [StringLength(4)]
        public string COST_GROUP { get; set; }

        [StringLength(4)]
        public string COST_TYPE { get; set; }

        [StringLength(50)]
        public string VARIATION_CODE { get; set; }

        public double? TOTAL_ORD_QUANT { get; set; }

        [Key]
        [Column(Order = 1)]
        public double TOTAL_SUP_QUANT { get; set; }

        [Key]
        [Column(Order = 2)]
        public double TOTAL_FUTURE_SUPPLY { get; set; }

        [Key]
        [Column(Order = 3)]
        public double TOTAL_OUTSTANDING_COSTS { get; set; }

        [Key]
        [Column(Order = 4)]
        public double TOTAL_COSTS { get; set; }
    }
}