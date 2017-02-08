namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCKTAKE_TOTALS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public int? LOCATION { get; set; }

        [StringLength(12)]
        public string BINCODE { get; set; }

        public double? SYSTEMQTY { get; set; }

        public double? COUNTQTY { get; set; }

        public double? VARIANCE { get; set; }

        public int? SUPPLIERNO { get; set; }

        public int? STOCKGROUP { get; set; }

        [StringLength(20)]
        public string BATCHCODE { get; set; }

        public double? UNITCOST { get; set; }

        public int? STOCKGROUP2 { get; set; }

        [Required]
        [StringLength(1)]
        public string HAS_BN { get; set; }

        public int? SERIALNO_TYPE { get; set; }

        [Required]
        [StringLength(1)]
        public string HAS_EXPIRY { get; set; }

        public int? EXPIRY_DAYS { get; set; }

        public DateTime? EXPIRY_DATE { get; set; }

        [StringLength(15)]
        public string STOCKGROUP_REPC { get; set; }

        [StringLength(15)]
        public string STOCKGROUP2_REPC { get; set; }

        [StringLength(30)]
        public string BARCODE1 { get; set; }
    }
}