namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MENU_COLLECTION
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public int CLASSICSEQNO { get; set; }

        public int FLOWSEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string ISENABLED { get; set; }
    }
}