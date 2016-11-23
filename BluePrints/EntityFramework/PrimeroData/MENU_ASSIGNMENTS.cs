namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MENU_ASSIGNMENTS
    {
        [Key]
        [StringLength(50)]
        public string USERNAME { get; set; }

        public int? MENU_NO { get; set; }

        public int? STAFFNO { get; set; }
    }
}
