namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class PERIOD_STATUS
    {
        public PERIOD_STATUS()
        {
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
            CRM_BUDGET1 = new HashSet<CRM_BUDGET>();
        }

        [Key]
        public int SEQNO { get; set; }

        public int AGE { get; set; }

        [Required]
        [StringLength(1)]
        public string LEDGER { get; set; }

        [StringLength(1)]
        public string LOCKED { get; set; }

        public int PERIOD_SEQNO { get; set; }

        [StringLength(20)]
        public string PERIODNAME { get; set; }

        [StringLength(8)]
        public string PERIOD_SHORTNAME { get; set; }

        [StringLength(8)]
        public string REPORTCODE { get; set; }

        public int YEARAGE { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? STOPDATE { get; set; }

        public int? MINSTOCKSEQNO { get; set; }

        public int? MINGLSEQNO { get; set; }

        public int? MINTRANSEQNO { get; set; }

        public int? MINTRANLINESEQNO { get; set; }

        public int? MINORDSEQNO { get; set; }

        public int? MINORDLINESEQNO { get; set; }

        public int FIN_QTR { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET1 { get; set; }
    }
}