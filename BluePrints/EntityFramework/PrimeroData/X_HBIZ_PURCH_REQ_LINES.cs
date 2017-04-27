namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class X_HBIZ_PURCH_REQ_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDR_SEQNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Column(TypeName = "text")]
        public string DESCRIPTION { get; set; }

        [Column(TypeName = "money")]
        public decimal? UNITPRICE { get; set; }

        public decimal? QUANTITY { get; set; }

        public int? CR_ACCNO { get; set; }

        [StringLength(60)]
        public string CR_NAME { get; set; }

        public int? JOBNO { get; set; }

        [StringLength(12)]
        public string ANALYSIS { get; set; }

        [StringLength(50)]
        public string SUPPLIERCODE { get; set; }

        public bool? APPROVED { get; set; }

        public int? APPROVED_BY_STAFFNO { get; set; }

        public DateTime? DATE_APPROVED { get; set; }

        public int? PO_SEQNO { get; set; }

        public int? POLINEID { get; set; }

        public int? COST_TYPE { get; set; }

        public int? COST_GROUP { get; set; }

        [Column(TypeName = "text")]
        public string NARRATIVE { get; set; }
    }
}