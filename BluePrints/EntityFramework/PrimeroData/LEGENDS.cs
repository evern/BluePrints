namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LEGENDS
    {
        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(50)]
        public string TABLENAME { get; set; }

        [Required]
        [StringLength(50)]
        public string FIELDNAME { get; set; }

        [Required]
        [StringLength(10)]
        public string FIELDVALUE { get; set; }

        [Required]
        [StringLength(50)]
        public string FIELDLABEL { get; set; }

        [Required]
        [StringLength(255)]
        public string FIELDNOTES { get; set; }

        public int FCOLOUR { get; set; }

        public int BCOLOUR { get; set; }
    }
}