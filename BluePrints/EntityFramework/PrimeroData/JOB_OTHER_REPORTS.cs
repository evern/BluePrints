namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOB_OTHER_REPORTS
    {
        [Key]
        public int REPORTNO { get; set; }

        [StringLength(40)]
        public string REPORTDESC { get; set; }

        [StringLength(200)]
        public string REPORT_PARAMS { get; set; }
    }
}