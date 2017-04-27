namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ACCS_BALANCE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCNO { get; set; }

        [StringLength(15)]
        public string ALPHACODE { get; set; }

        [StringLength(60)]
        public string NAME { get; set; }

        public int? ACCGROUP { get; set; }

        public int? CURRENCYNO { get; set; }

        public int? HEAD_ACCNO { get; set; }

        [StringLength(1)]
        public string ISHEADOFFICE { get; set; }

        public double? AGEDBAL0 { get; set; }

        public double? AGEDBAL1 { get; set; }

        public double? AGEDBAL2 { get; set; }

        public double? AGEDBAL3 { get; set; }

        public double? BALANCE { get; set; }

        public double? PRIOR_AGEDBAL0 { get; set; }

        public double? PRIOR_AGEDBAL1 { get; set; }

        public double? PRIOR_AGEDBAL2 { get; set; }

        public double? PRIOR_AGEDBAL3 { get; set; }

        public double? PRIOR_BALANCE { get; set; }

        [StringLength(12)]
        public string POST_CODE { get; set; }
    }
}