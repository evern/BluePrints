namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class GRAPH_SERIES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? ACC1 { get; set; }

        public int? ACC2 { get; set; }

        public int? BRANCHNO { get; set; }

        [StringLength(40)]
        public string NAME { get; set; }

        public int SERIES_TYPE { get; set; }

        [StringLength(8)]
        public string BUDGETCODE { get; set; }
    }
}