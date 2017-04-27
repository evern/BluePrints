namespace BluePrints.PrimeroData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CRM_BUDGET_HDR
    {
        public CRM_BUDGET_HDR()
        {
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
        }

        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(1024)]
        public string NAME { get; set; }

        public int MANREP_PERIOD_SEQNO { get; set; }

        public bool ISPRIMARY { get; set; }

        public bool ISACTIVE { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }

        public virtual MANREP_PERIOD MANREP_PERIOD { get; set; }
    }
}