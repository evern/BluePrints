namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLREPORT_BATCH
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BATCHNO { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public int? AGE { get; set; }
    }
}