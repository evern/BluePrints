namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POSTCODES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(40)]
        public string PLACE { get; set; }

        [StringLength(40)]
        public string DISTRICT { get; set; }

        [StringLength(5)]
        public string PLACE_POSTCODE { get; set; }

        [StringLength(5)]
        public string BOX_POSTCODE { get; set; }

        [StringLength(5)]
        public string BAG_POSTCODE { get; set; }

        [StringLength(5)]
        public string RD_POSTCODE { get; set; }

        [StringLength(40)]
        public string STATE { get; set; }
    }
}
