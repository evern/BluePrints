namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class INV_NUMBERS
    {
        [Key]
        public int INVNO { get; set; }

        public DateTime DATE_ISSUED { get; set; }

        [Required]
        [StringLength(30)]
        public string USERID { get; set; }
    }
}
