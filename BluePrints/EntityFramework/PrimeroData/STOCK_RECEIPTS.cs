namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_RECEIPTS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RECEIPTNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int? SUPPLIERNO { get; set; }

        [StringLength(30)]
        public string PACKINGSLIP { get; set; }

        public int? LOCATION { get; set; }

        [StringLength(1)]
        public string INVOICED { get; set; }
    }
}