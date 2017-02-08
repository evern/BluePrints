namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHEQUE_PRINT_HDR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHEQUENO { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(60)]
        public string ACCNAME { get; set; }

        public int? TOTALINVOICES { get; set; }

        public int? INVOICESRELEASED { get; set; }

        public double? TOTALAMTPAID { get; set; }

        public DateTime? CHEQUE_DATE { get; set; }

        public int? TAXRATENO { get; set; }

        public double? TAXAMOUNT { get; set; }

        [StringLength(1)]
        public string HASWHTAX { get; set; }

        public double? WHTAXRATE { get; set; }

        public double? WHTAXTOTAL { get; set; }

        [StringLength(1)]
        public string TAXOVERRIDDEN { get; set; }
    }
}