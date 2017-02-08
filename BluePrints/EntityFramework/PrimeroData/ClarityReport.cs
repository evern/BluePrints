namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ClarityReport")]
    public partial class ClarityReport
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(50)]
        public string FILENAME { get; set; }

        [StringLength(250)]
        public string DESCRIPTION { get; set; }

        [Column(TypeName = "image")]
        public byte[] REPORTDATA { get; set; }

        [Column(TypeName = "text")]
        public string PARAMS { get; set; }
    }
}