namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_JOB_TRANSACTIONS_DETAIL_V2
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(15)]
        public string MASTERJOB_CODE { get; set; }

        [StringLength(15)]
        public string SUBJOB_CODE { get; set; }

        [StringLength(50)]
        public string DEPARTMENT_NAME { get; set; }

        [StringLength(4)]
        public string DISCIPLINE_CODE { get; set; }

        [StringLength(4)]
        public string COMMODITY_CODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string X_VARIATIONCODE { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(1)]
        public string TRANSTYPE { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITPRICE { get; set; }

        public double? LINECOST { get; set; }

        [StringLength(50)]
        public string COSTTYPEDESC { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PO_NUMBER { get; set; }

        [StringLength(4096)]
        public string NARRATIVE { get; set; }

        [StringLength(60)]
        public string NAME { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int INVSEQNO { get; set; }

        [Key]
        [Column(Order = 4)]
        public double INVOICED { get; set; }

        public DateTime? INVOICEDATE { get; set; }

        public int? Q_WEEKNO { get; set; }

        public int? Q_YEAR { get; set; }
    }
}
