namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_LOC_INFO
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOCATION { get; set; }

        [StringLength(12)]
        public string BINCODE { get; set; }

        public double? MINSTOCK { get; set; }

        public double? MAXSTOCK { get; set; }

        public double QTY { get; set; }
    }
}