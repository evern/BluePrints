namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AUDITOR_SAVEPROCS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BRANCHNO { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SAVEID { get; set; }

        [StringLength(255)]
        public string LOCATION { get; set; }

        public int? VERSION { get; set; }

        public int? SAVETYPE { get; set; }

        public int? RETRIES { get; set; }

        [StringLength(1)]
        public string PURGE { get; set; }

        public DateTime? PURGED { get; set; }

        public int? PURGEINTERVAL { get; set; }

        [StringLength(1)]
        public string ENABLED { get; set; }
    }
}