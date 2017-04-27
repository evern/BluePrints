namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SUPPLIER_STOCK_ITEMS
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(23)]
        public string SUPPLIERCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? LATESTCOST { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCNO { get; set; }

        public double? ECONORDERQTY { get; set; }

        public double PURCHPACKQUANT { get; set; }

        public double PURCHPACKPRICE { get; set; }

        [StringLength(20)]
        public string PACKREFERENCE { get; set; }

        public DateTime? LAST_UPDATE { get; set; }

        public double? DISCOUNT { get; set; }

        [Required]
        [StringLength(1)]
        public string IS_DEFAULT { get; set; }
    }
}