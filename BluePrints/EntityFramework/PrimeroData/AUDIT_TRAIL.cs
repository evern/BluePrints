namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class AUDIT_TRAIL
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string TABLENAME { get; set; }

        public int? LINE_SEQNO { get; set; }

        [StringLength(60)]
        public string FIELDNAME { get; set; }

        [StringLength(100)]
        public string OLD_VALUE { get; set; }

        [StringLength(100)]
        public string NEW_VALUE { get; set; }

        [StringLength(100)]
        public string MODIFIEDBY { get; set; }

        public DateTime? MODIFIEDDATE { get; set; }
    }
}