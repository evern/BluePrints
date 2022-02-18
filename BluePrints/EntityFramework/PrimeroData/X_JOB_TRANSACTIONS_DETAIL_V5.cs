namespace BluePrints.PrimeroData
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_JOB_TRANSACTIONS_DETAIL_V5
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? MASTER_JOBNO { get; set; }

        public int? JOBNO { get; set; }

        [StringLength(15)]
        public string MASTER_JOBCODE { get; set; }

        [StringLength(15)]
        public string SUB_JOBCODE { get; set; }

        [StringLength(4)]
        public string DISCIPLINE_CODE { get; set; }

        public int? COST_GROUP_NO { get; set; }

        [StringLength(4)]
        public string COMMODITY_CODE { get; set; }

        public int? COST_TYPE_NO { get; set; }

        [StringLength(50)]
        public string DEPARTMENT_NAME { get; set; }

        [StringLength(50)]
        public string VARIATION_CODE { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(1)]
        public string TRANSTYPE { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITCOST { get; set; }

        public double? UNITPRICE { get; set; }

        public double? LINECOST { get; set; }

        public double? TOTALPRICE { get; set; }

        [StringLength(50)]
        public string COMMODITY_CODE_DESC { get; set; }

        public int? PO_NUMBER { get; set; }

        [StringLength(4096)]
        public string NARRATIVE { get; set; }

        [StringLength(60)]
        public string RESOURCE_NAME { get; set; }

        [StringLength(60)]
        public string SUPPLIER_NAME { get; set; }

        [StringLength(30)]
        public string LINE_STATUS { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        public double? INVOICED { get; set; }

        public DateTime? INVOICEDATE { get; set; }

        [StringLength(30)]
        public string RESOURCE_TITLE { get; set; }

        public int? ACCNO { get; set; }

        public int? PURCH_GL_NO { get; set; }

        [StringLength(40)]
        public string PURCH_GL_NAME { get; set; }

        public int? COST_GL_NO { get; set; }

        [StringLength(40)]
        public string COST_GL_NAME { get; set; }

        public int? Q_WEEKNO { get; set; }

        public int? Q_YEAR { get; set; }

        public int? STOCKGROUP { get; set; }

        public int? STOCKGROUP2 { get; set; }

        public int? NEW_JOBNO { get; set; }

        [StringLength(15)]
        public string OLD_JOBCODE { get; set; }

        public int? NEW_COST_GROUP_NO { get; set; }

        public int? NEW_COST_TYPE_NO { get; set; }

        [StringLength(20)]
        public string NEW_STOCK_CODE { get; set; }

        [NotMapped]
        public int? ViewJOBNO
        {
            get
            {
                if (NEW_JOBNO != null)
                    return NEW_JOBNO;

                return JOBNO;
            }
            set
            {
                //prevent IsTransactionFinalised to register false value
                if (value != JOBNO)
                    NEW_JOBNO = value;
                else
                    JOBNO = value;
            }
        }

        public TransactionAttributeStatus SubJobCodeStatus
        {
            get
            {
                if (NEW_JOBNO == null)
                    return TransactionAttributeStatus.Original;
                else if (NEW_JOBNO == JOBNO)
                    return TransactionAttributeStatus.Approved;
                else
                    return TransactionAttributeStatus.Pending;
            }
        }

        public string SubJobToolTip => SubJobCodeStatus != TransactionAttributeStatus.Original ? "Previous Job code : " + OLD_JOBCODE : null;

        public bool IsJobNoFinalised
        {
            get
            {
                return NEW_JOBNO == null || NEW_JOBNO == JOBNO;
            }
        }
    }
}
