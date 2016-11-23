namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_ER_SETUP
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string LOGINNAME { get; set; }

        [StringLength(50)]
        public string REPORTNAME { get; set; }

        [StringLength(50)]
        public string REPORTDESCRIPTION { get; set; }

        public int? MONTHDP { get; set; }

        [Required]
        [StringLength(1)]
        public string HAS_SUBTOTAL_BELOW { get; set; }

        [Required]
        [StringLength(1)]
        public string SHOW_CENTS { get; set; }

        [Required]
        [StringLength(1)]
        public string SALES_FROM_BRANCHES { get; set; }

        [Required]
        [StringLength(1)]
        public string SHOWQTYONLY { get; set; }

        [Required]
        [StringLength(1)]
        public string SHOWDOLLARSONLY { get; set; }

        [StringLength(50)]
        public string MONTH1DESC { get; set; }

        [StringLength(50)]
        public string MONTH2DESC { get; set; }

        [StringLength(50)]
        public string MONTH3DESC { get; set; }

        [StringLength(50)]
        public string MONTH4DESC { get; set; }

        [StringLength(50)]
        public string MONTH5DESC { get; set; }

        [StringLength(50)]
        public string MONTH6DESC { get; set; }

        [StringLength(50)]
        public string MONTH7DESC { get; set; }

        [StringLength(50)]
        public string MONTH8DESC { get; set; }

        [StringLength(50)]
        public string MONTH9DESC { get; set; }

        [StringLength(50)]
        public string MONTH10DESC { get; set; }

        [StringLength(50)]
        public string MONTH11DESC { get; set; }

        [StringLength(50)]
        public string MONTH12DESC { get; set; }
    }
}
