namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_ITEMS
    {
        public STOCK_ITEMS()
        {
            //CRM_BUDGET_LINE = new HashSet<CRM_BUDGET_LINE>();
            //CRM_BUDGET = new HashSet<CRM_BUDGET>();
        }

        [Key]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public int? STOCKGROUP { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public double? SELLPRICE1 { get; set; }

        public double? SELLPRICE2 { get; set; }

        public double? SELLPRICE3 { get; set; }

        public double? SELLPRICE4 { get; set; }

        public double? SELLPRICE5 { get; set; }

        public double? SELLPRICE6 { get; set; }

        public double? SELLPRICE7 { get; set; }

        public double? SELLPRICE8 { get; set; }

        public double? SELLPRICE9 { get; set; }

        public double? SELLPRICE10 { get; set; }

        public double? LATESTCOST { get; set; }

        public double? AVECOST { get; set; }

        public double? MINSTOCK { get; set; }

        public double? MAXSTOCK { get; set; }

        public int? SUPPLIERNO { get; set; }

        public double? MONTHUNITS { get; set; }

        public double? YEARUNITS { get; set; }

        public double? LASTYEARUNITS { get; set; }

        public double? MONTHVALUE { get; set; }

        public double? YEARVALUE { get; set; }

        public double? LASTYEARVALUE { get; set; }

        [StringLength(12)]
        public string BINCODE { get; set; }

        public int? DISCOUNTLEVEL { get; set; }

        public int? DEFDAYS { get; set; }

        [StringLength(30)]
        public string BARCODE1 { get; set; }

        [StringLength(30)]
        public string BARCODE2 { get; set; }

        [StringLength(30)]
        public string BARCODE3 { get; set; }

        public double? LASTMONTHVALUE { get; set; }

        public double? LASTMONTHUNITS { get; set; }

        public int? SALES_GL_CODE { get; set; }

        public int? PURCH_GL_CODE { get; set; }

        [StringLength(1)]
        public string WEB_SHOW { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public double? WEIGHT { get; set; }

        public double? CUBIC { get; set; }

        [StringLength(60)]
        public string ALERT { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        public double? PQTY { get; set; }

        [StringLength(10)]
        public string PACK { get; set; }

        [StringLength(1)]
        public string HAS_SN { get; set; }

        public double? STDCOST { get; set; }

        public int? SUPPLIERNO2 { get; set; }

        public int? SUPPLIERNO3 { get; set; }

        public int? SALES_GLSUBCODE { get; set; }

        public int? PURCH_GLSUBCODE { get; set; }

        public int? BRANCHNO { get; set; }

        public int? SALESTAXRATE { get; set; }

        public int? PURCHTAXRATE { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        [StringLength(23)]
        public string UPDATEITEM_CODE { get; set; }

        public double? UPDATEITEM_QTY { get; set; }

        public int? COS_GL_CODE { get; set; }

        public int? COS_GLSUBCODE { get; set; }

        public int? STOCKPRICEGROUP { get; set; }

        public double SUPPLIERCOST { get; set; }

        public double? ECONORDERQTY { get; set; }

        [StringLength(23)]
        public string LINKED_BILLCODE { get; set; }

        public int STOCK_CLASSIFICATION { get; set; }

        public int? STOCKGROUP2 { get; set; }

        public double TOTALSTOCK { get; set; }

        [StringLength(1)]
        public string HAS_BN { get; set; }

        [Required]
        [StringLength(1)]
        public string HAS_EXPIRY { get; set; }

        public int? EXPIRY_DAYS { get; set; }

        public double DUTY { get; set; }

        public int SERIALNO_TYPE { get; set; }

        public int LABEL_QTY { get; set; }

        [Required]
        [StringLength(1)]
        public string IS_DISCOUNTABLE { get; set; }

        [Required]
        [StringLength(1)]
        public string RESTRICTED_ITEM { get; set; }

        public int NUMDECIMALS { get; set; }

        public int COGSMETHOD { get; set; }

        public int DEFAULTWARRANTYNO { get; set; }

        public int DIMENSIONS { get; set; }

        public int? AUTO_NARRATIVE { get; set; }

        public int? X_SIZEID { get; set; }

        public int? X_COLOURID { get; set; }

        [StringLength(50)]
        public string X_DEPARTMENT { get; set; }

        [Required]
        [StringLength(1)]
        public string VARIABLECOST { get; set; }

        public double? PRICEQTY { get; set; }

        public double? PRICEPERKG { get; set; }

        public int COSTTYPE { get; set; }

        public int COSTGROUP { get; set; }

        public char? LOOKUP_RECOVERABLE { get; set; }
        
        public char? X_PAYTYPE { get; set; }

        public int? X_ALLOWNO { get; set; }

        //public virtual ICollection<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }

        //public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }
    }
}