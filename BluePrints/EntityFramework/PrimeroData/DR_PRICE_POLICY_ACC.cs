namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DR_PRICE_POLICY_ACC
    {
        [Key]
        public int SEQNO { get; set; }

        public int POLICY_HDR { get; set; }

        public int? ACCNO { get; set; }

        public int? ACCGROUP { get; set; }
    }
}
