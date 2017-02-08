namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BILLOMAT_TEMP
    {
        [Key]
        public int SEQNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int? COMPONENTLOCATION { get; set; }

        public int? PRODUCTLOCATION { get; set; }

        [StringLength(30)]
        public string REFERENCECODE { get; set; }

        public double? BATCHQUANTITY { get; set; }

        public int STAFFNO { get; set; }

        [StringLength(23)]
        public string BILLCODE { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public double? QUANTITY { get; set; }

        public double? UNITPRICE { get; set; }

        public int? KITSEQNO { get; set; }

        public int? BOMBATCHSEQNO { get; set; }
    }
}