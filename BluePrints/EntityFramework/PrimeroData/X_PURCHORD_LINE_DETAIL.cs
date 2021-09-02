namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class X_PURCHORD_LINE_DETAIL
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string MASTER_JOBCODE { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(15)]
        public string SUB_JOBCODE { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(4)]
        public string DISCIPLINE_CODE { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(4)]
        public string COMMODITY_CODE { get; set; }

        [StringLength(50)]
        public string COMMODITY_CODE_DESC { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PO_NUMBER { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(50)]
        public string VARIATION_CODE { get; set; }

        [Key]
        [Column(Order = 7)]
        public double ORDER_QTY { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        [StringLength(4096)]
        public string NARRATIVE { get; set; }

        [StringLength(60)]
        public string SUPPLIER_NAME { get; set; }

        public double? UNIT_PRICE { get; set; }

        public DateTime? ORDERDATE { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        [Key]
        [Column(Order = 8)]
        public double CUT_OFF_SUPPLIED { get; set; }

        [Key]
        [Column(Order = 9)]
        public double FUTURE_SUPPLY { get; set; }

        public double? LINETOTAL { get; set; }

        public int? STATUS { get; set; }

        [Key]
        [Column(Order = 10)]
        public double NormalizeOrderQty { get; set; }

        [Key]
        [Column(Order = 11)]
        public double NormalizeSupplyQty { get; set; }

        public double? NormalizeUnitPrice { get; set; }

        [Key]
        [Column(Order = 12)]
        public double RemainingQty { get; set; }

        public double? OUTSTANDING_COSTS { get; set; }
    }
}