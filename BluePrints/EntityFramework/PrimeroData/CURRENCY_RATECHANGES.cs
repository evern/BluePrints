namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CURRENCY_RATECHANGES
    {
        [Key]
        public int SEQNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int CURRENCYNO { get; set; }

        [StringLength(3)]
        public string CURRCODE { get; set; }

        public double? NEWBUYRATE { get; set; }

        public double? NEWSELLRATE { get; set; }

        public int? DR_PERIOD { get; set; }

        public int? CR_PERIOD { get; set; }

        public int? STK_PERIOD { get; set; }

        public int? GL_PERIOD { get; set; }
    }
}
