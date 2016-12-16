namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DEPARTMENT")]
    public partial class DEPARTMENT
    {
        public DEPARTMENT()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            COMMODITY_CODE = new HashSet<COMMODITY_CODE>();
            DOCTYPE = new HashSet<DOCTYPE>();
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

        public bool ISDESIGNCOMMODITY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        public virtual ICollection<COMMODITY_CODE> COMMODITY_CODE { get; set; }

        public virtual ICollection<DOCTYPE> DOCTYPE { get; set; }

        public virtual ICollection<RATE> RATE { get; set; }

        public virtual ICollection<WORKPACK> WORKPACK { get; set; }
    }
}
