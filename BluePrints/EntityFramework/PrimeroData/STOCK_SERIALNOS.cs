namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_SERIALNOS
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        [StringLength(30)]
        public string REFERENCE { get; set; }

        public DateTime EXPIRY_DATE { get; set; }

        public int LOCNO { get; set; }

        public double ACTUAL_COST { get; set; }

        public int PURCHORDNO { get; set; }

        public int SALESORDNO { get; set; }

        public int CRINVSEQNO { get; set; }

        public int INVSEQNO { get; set; }

        public int JOBNO { get; set; }

        public int STOCKINSEQNO { get; set; }

        public int STOCKOUTSEQNO { get; set; }

        public int CR_ACCNO { get; set; }

        public int DR_ACCNO { get; set; }

        public int IGRLINESEQNO { get; set; }

        public int JOBLINESEQNO { get; set; }

        public int SALESORDLINESEQNO { get; set; }

        public int PURCHORDLINESEQNO { get; set; }

        public int CRINVLINESEQNO { get; set; }

        public int DRINVLINESEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string INSTOCK { get; set; }

        public int UNITNO { get; set; }

        public DateTime LASTUPDATED { get; set; }

        public int COMPNO { get; set; }

        [StringLength(50)]
        public string KITCODE { get; set; }

        [StringLength(50)]
        public string KITID_SERIAL { get; set; }

        public int? IGRSEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string ISASSIGNED { get; set; }

        public int SO_INVLINESEQNO { get; set; }

        public int SO_INVSEQNO { get; set; }

        public int PO_INVLINESEQNO { get; set; }

        public int PO_INVSEQNO { get; set; }

        public int ASSIGNED_SEQNO { get; set; }

        public int ASSIGNED_LINESEQNO { get; set; }

        public int? REQUESTSEQNO { get; set; }

        public int? REQUESTLINESEQNO { get; set; }

        public int? RMASEQNO { get; set; }

        public int? RMALINESEQNO { get; set; }

        public int? STKMOVSEQNO { get; set; }

        public int? STKMOVLINESEQNO { get; set; }

        public int? WORKSORDSEQNO { get; set; }

        public int? WORKSORDLINESEQNO { get; set; }

        public int? SU_SEQNO { get; set; }
    }
}