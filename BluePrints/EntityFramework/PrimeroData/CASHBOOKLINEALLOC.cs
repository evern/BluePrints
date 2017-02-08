namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CASHBOOKLINEALLOC")]
    public partial class CASHBOOKLINEALLOC
    {
        [Key]
        public int SEQNO { get; set; }

        public int LINE_SEQNO { get; set; }

        public int TRANS_SEQNO { get; set; }

        public double AMOUNT { get; set; }
    }
}