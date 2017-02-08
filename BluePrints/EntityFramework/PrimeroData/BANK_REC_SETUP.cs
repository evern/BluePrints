namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BANK_REC_SETUP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Code { get; set; }

        public int? TransDatePOS { get; set; }

        public int? ChqNoPos { get; set; }

        public int? AmountPOS { get; set; }

        public int? CreditPOS { get; set; }

        public int? DebitPOS { get; set; }

        public int? Reg_Details { get; set; }

        [StringLength(32)]
        public string rectype { get; set; }

        public int? MTSPOS { get; set; }

        public int? REFPOS { get; set; }

        [StringLength(120)]
        public string LASTCSVFILE { get; set; }

        public int? SKIP_HEADER_ROWS { get; set; }

        public int? SKIP_FOOTER_ROWS { get; set; }

        public int? PARTPOS { get; set; }

        public int ACCNO { get; set; }

        public int SUBACCNO { get; set; }

        [StringLength(30)]
        public string MTS_RECEIPTS { get; set; }

        [StringLength(30)]
        public string MTS_REVERSALS { get; set; }

        [StringLength(30)]
        public string DR_MATCH_FIELD { get; set; }
    }
}