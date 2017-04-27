namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TERRITORYBUDGET")]
    public partial class TERRITORYBUDGET
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        public int STOCK_GROUP_SEQ { get; set; }

        public int BUDGETYEAR { get; set; }

        public int PERIODNO { get; set; }

        public double? BUDGETVAL { get; set; }

        public int? PERIOD_SEQNO { get; set; }
    }
}