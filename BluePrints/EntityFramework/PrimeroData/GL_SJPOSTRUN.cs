namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_SJPOSTRUN
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(80)]
        public string DESCRIPTION { get; set; }

        public int? HDRTYPE { get; set; }

        [StringLength(3)]
        public string INITIALS { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(1)]
        public string STOCKLEDGER { get; set; }

        [StringLength(1)]
        public string DEBTORSLEDGER { get; set; }

        [StringLength(1)]
        public string CREDITORSLEDGER { get; set; }

        [StringLength(1)]
        public string EXCLUDEDRPMTS { get; set; }

        public int SOURCERANGE { get; set; }

        [StringLength(20)]
        public string STARTRANGE { get; set; }

        [StringLength(20)]
        public string ENDRANGE { get; set; }

        public int DESTRANGE { get; set; }

        public int DESTPERIOD { get; set; }

        public DateTime? CREATEDDATE { get; set; }
    }
}