namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROSPECT_CONTACTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? PROSPECT_SEQNO { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string DEFCONTACT { get; set; }

        [Required]
        [StringLength(1)]
        public string DEFACCOUNT { get; set; }
    }
}