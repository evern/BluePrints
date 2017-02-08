namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CR_PRICES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public int? STOCKPRICEGROUP { get; set; }

        public double? PRICE { get; set; }

        public double? DISCOUNT { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? STOPDATE { get; set; }

        public double? MINQTY { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string FIXED { get; set; }
    }
}