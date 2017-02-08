namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOBCOST_RESOURCE
    {
        [Key]
        public int SEQNO { get; set; }

        public double? COSTRATE0 { get; set; }

        public double? COSTRATE1 { get; set; }

        public double? COSTRATE2 { get; set; }

        public double? COSTRATE3 { get; set; }

        public double? SELLRATE0 { get; set; }

        public double? SELLRATE1 { get; set; }

        public double? SELLRATE2 { get; set; }

        public double? SELLRATE3 { get; set; }

        public DateTime? REVIEWDATE { get; set; }

        public double? NORMALHOURS { get; set; }

        public int? STAFFNO { get; set; }

        [Required]
        [StringLength(30)]
        public string RESOURCENAME { get; set; }

        [StringLength(30)]
        public string TITLE { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(23)]
        public string DEFAULT_STOCKCODE { get; set; }

        [StringLength(3)]
        public string SHORTCODE { get; set; }

        [StringLength(40)]
        public string EMAIL_ADDRESS { get; set; }

        [StringLength(100)]
        public string FILTERSQL { get; set; }
    }
}