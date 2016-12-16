namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DISCIPLINE")]
    public partial class DISCIPLINE
    {
        public DISCIPLINE()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            COMMODITY_CODE = new HashSet<COMMODITY_CODE>();
            COMMODITY_GROUP_DIRECT = new HashSet<COMMODITY_GROUP_DIRECT>();
            ESTIMATION_ITEM = new HashSet<ESTIMATION_ITEM>();
            RATE = new HashSet<RATE>();
            WORKPACK = new HashSet<WORKPACK>();
        }

        [Key]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(50)]
        public string CODE { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        public virtual ICollection<COMMODITY_CODE> COMMODITY_CODE { get; set; }

        public virtual ICollection<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECT { get; set; }

        public virtual ICollection<ESTIMATION_ITEM> ESTIMATION_ITEM { get; set; }

        public virtual ICollection<RATE> RATE { get; set; }

        public virtual ICollection<WORKPACK> WORKPACK { get; set; }
    }
}
