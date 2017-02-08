namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SALESORD_STATUS
    {
        [Key]
        public int SEQNO { get; set; }

        public int STATUSNO { get; set; }

        [Required]
        [StringLength(30)]
        public string DESCRIPTION { get; set; }
    }
}