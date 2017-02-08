namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class INV_FIELD_NAMES
    {
        [Key]
        public int INVTYPE { get; set; }

        [StringLength(20)]
        public string FIELDNAME { get; set; }
    }
}