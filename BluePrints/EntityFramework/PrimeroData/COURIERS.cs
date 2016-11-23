namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class COURIERS
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(20)]
        public string COURIER_NAME { get; set; }

        public int COURNO { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(255)]
        public string UPLOADEMAIL { get; set; }

        [StringLength(255)]
        public string UNLREF { get; set; }

        public int? COURIER_MODE { get; set; }

        public int? PREFIX_SIZE { get; set; }

        public int? COURIER_DISPMETHOD { get; set; }

        [StringLength(128)]
        public string TRACK_TRACE_URL { get; set; }

        [StringLength(30)]
        public string CONNOTE { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }
    }
}
