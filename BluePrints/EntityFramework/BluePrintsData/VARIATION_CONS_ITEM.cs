namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VARIATION_CONS_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_VARIATION { get; set; }

        public ConstructionVariationItemType TYPE { get; set; }

        [Required]
        [StringLength(50)]
        public string ITEM_ID { get; set; }

        public int? SUBJOB_JOBNO { get; set; }

        public int? COSTGROUP_SEQNO { get; set; }

        public int? COSTTYPE_SEQNO { get; set; }

        [Required]
        [StringLength(50)]
        public string STOCKCODE { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [StringLength(500)]
        public string NOTES { get; set; }

        [StringLength(500)]
        public string COMMENTS { get; set; }

        public decimal HOURS { get; set; }

        public decimal RATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual VARIATION_CONS VARIATION_CONS { get; set; }
    }
}
