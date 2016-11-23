namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ANALYSIS")]
    public partial class ANALYSIS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string CODE { get; set; }

        [Required]
        [StringLength(50)]
        public string NAME { get; set; }

        [Required]
        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(50)]
        public string CODE_TEMPLATE { get; set; }

        public int? CODE_SET_SEQNO { get; set; }

        public int? PARENT_SEQNO { get; set; }

        [Column(TypeName = "money")]
        public decimal X_BUDGET { get; set; }
    }
}
