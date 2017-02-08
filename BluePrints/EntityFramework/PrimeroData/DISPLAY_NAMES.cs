namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DISPLAY_NAMES
    {
        [Key]
        [StringLength(30)]
        public string NAMEID { get; set; }

        [StringLength(40)]
        public string DISPLAYNAME { get; set; }

        [StringLength(100)]
        public string DISPLAYHINT { get; set; }
    }
}