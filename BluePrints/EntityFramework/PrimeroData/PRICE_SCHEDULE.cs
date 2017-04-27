namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class PRICE_SCHEDULE
    {
        [Key]
        public int SEQNO { get; set; }

        public int STATUS { get; set; }

        public int ENTEREDBY { get; set; }

        public int PRICETYPE { get; set; }

        public DateTime CHANGEDATE { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public int ACCGROUP { get; set; }

        [StringLength(60)]
        public string NEWPRICESQL { get; set; }
    }
}