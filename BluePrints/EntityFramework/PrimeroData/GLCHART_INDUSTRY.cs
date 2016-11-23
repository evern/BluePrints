namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLCHART_INDUSTRY
    {
        [Key]
        public int INDUSTRYNO { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }
    }
}
