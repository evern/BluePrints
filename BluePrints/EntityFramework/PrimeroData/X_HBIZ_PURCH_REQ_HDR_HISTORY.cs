namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class X_HBIZ_PURCH_REQ_HDR_HISTORY
    {
        [Key]
        public int SEQNO { get; set; }

        public int? STAFFNO { get; set; }

        [StringLength(255)]
        public string REASON { get; set; }

        public DateTime? TIME { get; set; }

        public bool? IS_APPROVED { get; set; }

        public int REQ_HDR_SEQNO { get; set; }

        public int? STATUS { get; set; }
    }
}