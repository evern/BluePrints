namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CUSTOM_REPORTS
    {
        [Key]
        [StringLength(15)]
        public string REPT_CODE { get; set; }

        [StringLength(50)]
        public string REPT_NAME { get; set; }

        [StringLength(4096)]
        public string REPT_SQL { get; set; }

        [StringLength(1)]
        public string REPT_LOCKED { get; set; }

        [StringLength(1)]
        public string AUTO_TOTAL { get; set; }

        [StringLength(1)]
        public string BLANK_ZEROS { get; set; }

        public int? LINESPERPAGE { get; set; }

        [StringLength(1)]
        public string LANDSCAPE { get; set; }

        [StringLength(1)]
        public string SHOWMAXIMISED { get; set; }

        [StringLength(3)]
        public string CLICK_TO { get; set; }

        [StringLength(1)]
        public string SUBGROUP { get; set; }

        [StringLength(255)]
        public string COLUMN_WIDTHS { get; set; }

        [StringLength(255)]
        public string COLUMN_NAMES { get; set; }

        [StringLength(255)]
        public string GROUP_HEADER_SQL { get; set; }
    }
}
