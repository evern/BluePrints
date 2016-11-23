namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCKTAKE_EVENTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int LOCNO { get; set; }

        public DateTime POSTTIME { get; set; }

        public int CLASSNO { get; set; }

        public int REASONSEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string FROM_LEDGER { get; set; }

        public int FROM_HEADER { get; set; }

        public int LINE_SOURCE { get; set; }

        public int PHYS_STAFF { get; set; }

        public int PHYS_BRANCH { get; set; }

        public int SUPER_STAFF { get; set; }

        [Column(TypeName = "text")]
        public string MEMO { get; set; }

        [Required]
        [StringLength(50)]
        public string OLD_VAL { get; set; }

        [Required]
        [StringLength(50)]
        public string NEW_VAL { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public int CR_ACCNO { get; set; }

        public int DR_ACCNO { get; set; }

        public int PROSPECT_SEQNO { get; set; }

        public int CONTACT_SEQNO { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public int SU_SEQNO { get; set; }

        [StringLength(25)]
        public string REF1 { get; set; }

        [StringLength(1)]
        public string NEEDREVISIT { get; set; }
    }
}
