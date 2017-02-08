namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TAX_RETURN_POINT
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string TAXRETURNCODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string KEY_POINT { get; set; }

        public double? AMOUNT { get; set; }

        [Required]
        [StringLength(1)]
        public string MANUALLY_CHANGED { get; set; }
    }
}