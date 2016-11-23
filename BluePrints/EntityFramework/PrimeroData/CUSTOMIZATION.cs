namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CUSTOMIZATION")]
    public partial class CUSTOMIZATION
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(50)]
        public string FORMNAME { get; set; }

        [StringLength(50)]
        public string TYPE { get; set; }

        [Column(TypeName = "image")]
        public byte[] CHANGES { get; set; }
    }
}
