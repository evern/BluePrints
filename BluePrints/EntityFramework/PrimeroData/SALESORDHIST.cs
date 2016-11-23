namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SALESORDHIST")]
    public partial class SALESORDHIST
    {
        [Key]
        public int SEQNO { get; set; }

        public int? LINES_SOURCE_SEQ { get; set; }

        public int? HEADER_SOURCE_SEQ { get; set; }

        public int? HISTORYNO { get; set; }

        [Required]
        [StringLength(1)]
        public string EVENT_TYPE { get; set; }

        public DateTime? HISTDATETIME { get; set; }

        public int? DR_INVLINES_SEQ { get; set; }

        public int? ST_TRANS_SEQ { get; set; }

        public int? MANIFEST_NO { get; set; }

        [StringLength(100)]
        public string FILEURL { get; set; }

        public double? QTY { get; set; }

        public int? SALESNO { get; set; }

        public int? PHYS_STAFF { get; set; }

        public int? PHYS_BRANCH { get; set; }
    }
}
