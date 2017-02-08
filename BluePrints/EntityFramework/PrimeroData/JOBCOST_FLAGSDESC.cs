namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOBCOST_FLAGSDESC
    {
        [Key]
        [StringLength(8)]
        public string FLAGCODE { get; set; }

        [StringLength(60)]
        public string FLAGDESC { get; set; }
    }
}