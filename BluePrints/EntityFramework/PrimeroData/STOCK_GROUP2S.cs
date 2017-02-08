namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_GROUP2S
    {
        public STOCK_GROUP2S()
        {
            CRM_BUDGET_LINE = new HashSet<CRM_BUDGET_LINE>();
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
        }

        [Key]
        public int GROUPNO { get; set; }

        [StringLength(30)]
        public string GROUPNAME { get; set; }

        public int? BRANCHNO { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(50)]
        public string FILENAME { get; set; }

        [Required]
        [StringLength(1)]
        public string DATAX_EXCHANGE_FLAG { get; set; }

        public int DATAX_SOURCE_SITE { get; set; }

        public int DATAX_EXCHNO { get; set; }

        public int DATAX_SITE_NO2 { get; set; }

        [StringLength(10)]
        public string AUTOCODE { get; set; }

        public int? AUTOCODENO { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }

        public virtual ICollection<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }
    }
}