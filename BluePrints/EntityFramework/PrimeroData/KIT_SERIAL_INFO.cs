namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class KIT_SERIAL_INFO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int KITSEQNO { get; set; }

        [StringLength(50)]
        public string KIT_SERIAL_NO { get; set; }

        public DateTime? EXPIRY { get; set; }

        [StringLength(50)]
        public string REFERENCE { get; set; }
    }
}
