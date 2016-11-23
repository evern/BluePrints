namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_GROUPS
    {
        public STOCK_GROUPS()
        {
            CRM_BUDGET_LINE = new HashSet<CRM_BUDGET_LINE>();
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
        }

        [Key]
        public int GROUPNO { get; set; }

        [StringLength(30)]
        public string GROUPNAME { get; set; }

        public int? BRANCHNO { get; set; }

        [StringLength(10)]
        public string AUTOCODE { get; set; }

        public int? AUTOCODENO { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(50)]
        public string FILENAME { get; set; }

        public int? GROUP2_SEQNO { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }

        public double? EXPECTEDPROFIT { get; set; }

        public double? PROFITVARIANCE { get; set; }

        public int? SALES_GL_CODE { get; set; }

        public int? SALES_GLSUBCODE { get; set; }

        public int? PURCH_GL_CODE { get; set; }

        public int? PURCH_GLSUBCODE { get; set; }

        public int? COS_GL_CODE { get; set; }

        public int? COS_GLSUBCODE { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(1)]
        public string X_ISSTYLE { get; set; }

        [StringLength(255)]
        public string X_SIZEIDS { get; set; }

        [StringLength(255)]
        public string X_COLOURIDS { get; set; }

        [StringLength(1)]
        public string X_PRICEGROUP { get; set; }

        [StringLength(23)]
        public string X_STOCKCODE_FORMULA { get; set; }

        [StringLength(40)]
        public string X_DESCRIPTION_FORMULA { get; set; }

        public int? X_SUPPLIERNO { get; set; }

        public double? X_SUPPLIERCOST { get; set; }

        public double? X_MINSTOCK { get; set; }

        public double? X_MAXSTOCK { get; set; }

        [StringLength(12)]
        public string X_BINCODE { get; set; }

        public double? X_SELLPRICE1 { get; set; }

        public double? X_SELLPRICE2 { get; set; }

        public double? X_SELLPRICE3 { get; set; }

        public double? X_SELLPRICE4 { get; set; }

        public double? X_SELLPRICE5 { get; set; }

        public double? X_SELLPRICE6 { get; set; }

        public double? X_SELLPRICE7 { get; set; }

        public double? X_SELLPRICE8 { get; set; }

        public double? X_SELLPRICE9 { get; set; }

        public double? X_SELLPRICE10 { get; set; }

        public virtual ICollection<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }
    }
}
