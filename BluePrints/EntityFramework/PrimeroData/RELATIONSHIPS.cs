namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RELATIONSHIPS
    {
        [Key]
        public int SEQNO { get; set; }

        public int REL_SEQNO { get; set; }

        public int SOURCE_ENT_SEQNO { get; set; }

        public int DEST_ENT_SEQNO { get; set; }

        public int RELSETSEQNO { get; set; }
    }
}
