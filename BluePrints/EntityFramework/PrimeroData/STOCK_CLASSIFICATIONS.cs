namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_CLASSIFICATIONS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CLASSNO { get; set; }

        [Required]
        [StringLength(100)]
        public string CLASSNAME { get; set; }
    }
}