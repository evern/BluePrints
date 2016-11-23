namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SU_WARRANTY
    {
        [Key]
        public int SEQNO { get; set; }

        public int SU_SEQNO { get; set; }

        [StringLength(50)]
        public string WARRANTY_REF { get; set; }

        public int? WARRANTYTYPE_SEQ { get; set; }

        [StringLength(50)]
        public string SERIALNO { get; set; }

        public DateTime? EXPDATE { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }
    }
}
