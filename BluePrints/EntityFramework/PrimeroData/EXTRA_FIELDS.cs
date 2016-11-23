namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class EXTRA_FIELDS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string TABLE_NAME { get; set; }

        [StringLength(60)]
        public string FIELD_NAME { get; set; }

        [StringLength(60)]
        public string DISPLAY_NAME { get; set; }

        public int? CONTROL_NO { get; set; }

        public int? CONTROL_SIZE { get; set; }

        public int? CONTROL_TYPE { get; set; }

        [StringLength(100)]
        public string LOOKUP_TABLE { get; set; }

        [StringLength(100)]
        public string LOOKUP_FIELD { get; set; }

        [StringLength(100)]
        public string DATA_FIELD { get; set; }

        public int? DATA_SIZE { get; set; }

        [StringLength(20)]
        public string DATA_TYPE { get; set; }

        [StringLength(1)]
        public string MIXEDCASE { get; set; }

        [Required]
        [StringLength(1)]
        public string EDITABLE { get; set; }

        [StringLength(100)]
        public string TOOLTIP { get; set; }

        [StringLength(200)]
        public string EVENT_FIELD_NAME { get; set; }

        [StringLength(5000)]
        public string EVENT_SQL { get; set; }
    }
}
