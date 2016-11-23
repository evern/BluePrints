namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PRINT_LOG
    {
        [Key]
        public int SEQNO { get; set; }

        public int DOC_TYPE { get; set; }

        public int HDR_SEQNO { get; set; }

        public DateTime PRINT_TIME { get; set; }

        [StringLength(15)]
        public string REFERENCE { get; set; }

        [StringLength(20)]
        public string SENT_TO { get; set; }
    }
}
