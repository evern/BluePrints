namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DISPMETHOD")]
    public partial class DISPMETHOD
    {
        [Key]
        public int SEQ_NO { get; set; }

        [StringLength(30)]
        public string DESCRIPTION { get; set; }
    }
}
