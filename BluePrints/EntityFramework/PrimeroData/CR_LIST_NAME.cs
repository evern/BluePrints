namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CR_LIST_NAME
    {
        [Key]
        public int LIST_NO { get; set; }

        [StringLength(50)]
        public string LIST_NAME { get; set; }
    }
}
