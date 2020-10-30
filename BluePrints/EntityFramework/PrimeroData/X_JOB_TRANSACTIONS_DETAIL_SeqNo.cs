namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_JOB_TRANSACTIONS_DETAIL_SeqNo
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? jobno { get; set; }

        [StringLength(50)]
        public string X_VARIATIONCODE { get; set; }

        public string NARRATIVE { get; set; }

        public int? master_jobno { get; set; }

        public double? EXCHRATE { get; set; }

        [StringLength(15)]
        public string jobcode { get; set; }

        public DateTime? transdate { get; set; }

        [StringLength(1)]
        public string transtype { get; set; }

        [StringLength(23)]
        public string stockcode { get; set; }

        [StringLength(60)]
        public string description { get; set; }

        public double? quantity { get; set; }

        public double? unitcost { get; set; }

        public double? UNITPRICE { get; set; }

        public double? LINECOST { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int linecharge { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LINETOTAL { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LINETOTAL_INCTAX { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LINETOTAL_TAX { get; set; }

        [StringLength(30)]
        public string LINE_STATUS { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CostType { get; set; }

        [StringLength(50)]
        public string CostTypeDesc { get; set; }

        [StringLength(30)]
        public string Typeshortcode { get; set; }

        public int? COST_GROUP { get; set; }

        [StringLength(50)]
        public string CostGroupDesc { get; set; }

        [StringLength(30)]
        public string GroupShortcode { get; set; }

        public int? branchno { get; set; }

        [StringLength(10)]
        public string LINE_SOURCE { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SOURCE_SEQNO { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PO_LINESEQNO { get; set; }

        public int? POno { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int invseqno { get; set; }

        [StringLength(30)]
        public string refno { get; set; }

        [StringLength(60)]
        public string name { get; set; }

        [StringLength(20)]
        public string invno { get; set; }

        public double? CostActual { get; set; }

        public int? glcode { get; set; }

        public int? accno { get; set; }

        [Key]
        [Column(Order = 9)]
        public double INVOICED { get; set; }

        public DateTime? INVOICEDATE { get; set; }

        [NotMapped]
        public bool QtyEdited { get; set; }
    }
}
