namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MANREP_CLOSING_STOCK
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MANREP_SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public double? TOTALSTOCK { get; set; }

        public double? AVECOST { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? CLOSINGSTOCK { get; set; }

        public double? LATESTCOST { get; set; }

        public double? STDCOST { get; set; }

        [Required]
        [StringLength(1)]
        public string SCHEDULED { get; set; }
    }
}