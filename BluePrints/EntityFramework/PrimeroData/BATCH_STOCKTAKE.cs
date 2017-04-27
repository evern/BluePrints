namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class BATCH_STOCKTAKE
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        [StringLength(12)]
        public string BINCODE { get; set; }

        public int? LOCATION { get; set; }

        public double? SYSQTY { get; set; }

        public int? STOCKGROUP { get; set; }

        public double? COUNTQTY { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? VARIANCE { get; set; }
    }
}