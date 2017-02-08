namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_SIZE
    {
        [Key]
        public int SIZEID { get; set; }

        [Required]
        [StringLength(5)]
        public string SIZECODE { get; set; }

        [StringLength(30)]
        public string SIZENAME { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public int? SORTORDER { get; set; }
    }
}