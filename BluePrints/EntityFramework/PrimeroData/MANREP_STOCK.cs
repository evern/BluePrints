namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MANREP_STOCK
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MANREP_SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Location { get; set; }

        public double? AMOUNT_AVECOST { get; set; }

        public double? AMOUNT_LATESTCOST { get; set; }

        public double? AMOUNT_STDCOST { get; set; }

        [Required]
        [StringLength(1)]
        public string SCHEDULED { get; set; }
    }
}