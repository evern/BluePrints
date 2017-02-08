namespace BluePrints.Data
{
    using Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DELIVERABLES_STATUS
    {
        public DELIVERABLES_STATUS()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(500)]
        public string NAME { get; set; }

        public decimal MAX_PERCENTAGE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }
    }
}