namespace BluePrints.PrimeroData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class DR_ACCGROUP2S
    {
        public DR_ACCGROUP2S()
        {
            CRM_BUDGET_LINE = new HashSet<CRM_BUDGET_LINE>();
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
        }

        [Key]
        public int ACCGROUP { get; set; }

        [StringLength(30)]
        public string GROUPNAME { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }

        public virtual ICollection<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }
    }
}