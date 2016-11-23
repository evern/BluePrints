namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLTRANS_ARCHIVE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int ACCNO { get; set; }

        public int? SUBACCNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int COMPANYNO { get; set; }

        public int BRANCHNO { get; set; }

        public int BATCHNO { get; set; }

        [StringLength(3)]
        public string INITIALS { get; set; }

        [StringLength(12)]
        public string CHQNO { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        [StringLength(30)]
        public string DETAILS { get; set; }

        public int? RECONCILED { get; set; }

        [StringLength(1)]
        public string SOURCE { get; set; }

        public double? AMOUNT { get; set; }

        public int? SOURCE_ACCNO { get; set; }

        public int? SOURCE_SEQ { get; set; }

        public double? FCAMOUNT { get; set; }

        [StringLength(1)]
        public string AUTO_REVERSE { get; set; }

        public int? ASSETNO { get; set; }

        public int? RECONCILENO { get; set; }

        [StringLength(15)]
        public string TAXRETCODE { get; set; }

        public int? SESSION_ID { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? PERIODNO { get; set; }
    }
}
