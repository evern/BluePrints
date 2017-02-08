namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CUSTOM_VIEWS
    {
        [Key]
        public int SEQNO { get; set; }

        public int STAFFNO { get; set; }

        public int ENTITY_TYPE { get; set; }

        [StringLength(40)]
        public string VIEW_NAME { get; set; }

        [StringLength(4096)]
        public string VIEW_DETAILS { get; set; }
    }
}