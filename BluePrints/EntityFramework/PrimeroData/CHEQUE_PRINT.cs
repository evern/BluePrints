namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHEQUE_PRINT
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ChequeNo { get; set; }

        public int? AccNo { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(64)]
        public string InvNo { get; set; }

        [StringLength(256)]
        public string AmountText { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        [StringLength(512)]
        public string Details { get; set; }

        [StringLength(64)]
        public string Ref1 { get; set; }

        [StringLength(64)]
        public string Ref2 { get; set; }

        public DateTime? ChequeDate { get; set; }

        public double? Amount { get; set; }

        public double? GrossAmount { get; set; }

        public double? DiscAmount { get; set; }

        public double? SubTotal { get; set; }

        public double? TaxTotal { get; set; }

        public DateTime? INVOICEDATE { get; set; }

        [Key]
        [Column(Order = 2)]
        public double WHTAXRATE { get; set; }

        [Key]
        [Column(Order = 3)]
        public double WHTAXAMOUNT { get; set; }

        public int? CRTRANS_SEQNO { get; set; }
    }
}
