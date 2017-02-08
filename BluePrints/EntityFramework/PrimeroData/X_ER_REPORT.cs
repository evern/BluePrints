namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_ER_REPORT
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string LOGINNAME { get; set; }

        public int? GROUPNO { get; set; }

        [StringLength(70)]
        public string GROUPNAME { get; set; }

        [StringLength(70)]
        public string DESCRIPTION { get; set; }

        public double MTD { get; set; }

        public double MTDCOST { get; set; }

        public double MTDQTY { get; set; }

        public double YTD { get; set; }

        public double YTDCOST { get; set; }

        public double YTDQTY { get; set; }

        public double LYTD { get; set; }

        public double LYTDCOST { get; set; }

        public double LYTDQTY { get; set; }

        public double LYMTD { get; set; }

        public double LYMTDCOST { get; set; }

        public double LYMTDQTY { get; set; }

        public double MONTH1 { get; set; }

        public double MONTH2 { get; set; }

        public double MONTH3 { get; set; }

        public double MONTH4 { get; set; }

        public double MONTH5 { get; set; }

        public double MONTH6 { get; set; }

        public double MONTH7 { get; set; }

        public double MONTH8 { get; set; }

        public double MONTH9 { get; set; }

        public double MONTH10 { get; set; }

        public double MONTH11 { get; set; }

        public double MONTH12 { get; set; }

        [StringLength(70)]
        public string CODE { get; set; }

        public int? SALESNO { get; set; }

        public int? ACCNO { get; set; }

        public double MTDPER { get; set; }

        public double YTDPER { get; set; }

        public double LYMTDPER { get; set; }

        public double LYYTDPER { get; set; }
    }
}