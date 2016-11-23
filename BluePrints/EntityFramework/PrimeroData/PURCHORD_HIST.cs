namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PURCHORD_HIST
    {
        [Key]
        public int SEQNO { get; set; }

        public int? PO_HDR_SEQ { get; set; }

        public int? PO_LINE_SEQ { get; set; }

        public int? IWG_HDR_SEQ { get; set; }

        public int? IWG_LINE_SEQ { get; set; }

        public int? INV_HDR_SEQ { get; set; }

        public int? INV_LINE_SEQ { get; set; }

        public int? ST_TRANS_SEQ { get; set; }

        public int? HISTORYNO { get; set; }

        [StringLength(1)]
        public string EVENT_TYPE { get; set; }

        public DateTime? HISTDATETIME { get; set; }

        [StringLength(100)]
        public string FILEURL { get; set; }

        public double QTY { get; set; }

        public int? SALESNO { get; set; }

        public int? PHYS_STAFF { get; set; }

        public int? PHYS_BRANCH { get; set; }

        public int? EVENT_INT_DETAILS { get; set; }

        public int? RELATED_SEQ { get; set; }
    }
}
