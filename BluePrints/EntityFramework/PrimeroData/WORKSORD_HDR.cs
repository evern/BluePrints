namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class WORKSORD_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(23)]
        public string BILLCODE { get; set; }

        [StringLength(23)]
        public string PRODCODE { get; set; }

        [StringLength(15)]
        public string BATCHCODE { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public DateTime? PRODDATE { get; set; }

        public DateTime? DUEDATE { get; set; }

        public int? ORDSTATUS { get; set; }

        public int? SALESORDNO { get; set; }

        [StringLength(1024)]
        public string NOTES { get; set; }

        public double? PRODQTY { get; set; }

        public double? ACTUALQTY { get; set; }

        public int? PRODLOCNO { get; set; }

        [StringLength(30)]
        public string REFERENCE { get; set; }

        public int? STAFFNO { get; set; }

        public int? COMPONENTLOCNO { get; set; }

        public DateTime? EXPIRY_DATE { get; set; }
    }
}
