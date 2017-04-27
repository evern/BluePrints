namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class TIME_BILLING
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        public int? STATUS { get; set; }

        public DateTime? WORKDATE { get; set; }

        public int? STARTTIME { get; set; }

        public int? STOPTIME { get; set; }

        public double? HOURS { get; set; }

        public double? UNITPRICE { get; set; }

        [StringLength(15)]
        public string CODE { get; set; }

        [StringLength(40)]
        public string DETAILS { get; set; }

        public int? STAFFNO { get; set; }

        [StringLength(23)]
        public string LINKED_STOCKCODE { get; set; }

        public double? LINKED_QTY { get; set; }

        public int LINETYPE { get; set; }
    }
}