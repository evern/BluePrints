namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CAMPAIGN_TYPE
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPT { get; set; }
    }
}