namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AUTO_PURCH
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public int? STOCKGROUP { get; set; }

        public double? MINSTOCK { get; set; }

        public double? MAXSTOCK { get; set; }

        public double? INSTOCKQTY { get; set; }

        public double? PURCHORDQTY { get; set; }

        public double? SALESORDQTY { get; set; }

        public double? VIRTSTOCK { get; set; }

        public double? CALCREORD { get; set; }

        public double? ACTUALREORD { get; set; }

        public double? SALES0 { get; set; }

        public double? SALES1 { get; set; }

        public double? SALES2 { get; set; }

        public double? SALES3 { get; set; }

        public double? SALES4 { get; set; }

        public double? SALES5 { get; set; }

        public double? SALES6 { get; set; }

        public double? SALES7 { get; set; }

        public double? SALES8 { get; set; }

        public double? SALES9 { get; set; }

        public double? SALES10 { get; set; }

        public double? SALES11 { get; set; }

        public double? SALES12 { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPUTER_SEQNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCNO { get; set; }
    }
}