namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PAYMENT_DENOMINATIONS
    {
        [Key]
        public int SEQNO { get; set; }

        public int PTNO { get; set; }

        public int PDSORTORDER { get; set; }

        [Required]
        [StringLength(15)]
        public string PDNAME { get; set; }

        public double PDFACTOR { get; set; }

        public int? CURRENCYNO { get; set; }
    }
}
