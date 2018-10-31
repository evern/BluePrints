namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DELIVERABLES_STATUS
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        [Required]
        [StringLength(500)]
        public string NAME { get; set; }

        public decimal MAX_PERCENTAGE { get; set; }

        public decimal? AUTO_PERCENTAGE { get; set; }

        public bool FOR_DELIVERABLE { get; set; }

        public bool FOR_TASK { get; set; }

        public bool FOR_NCR { get; set; }

        public bool FOR_NONDELIVERABLE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DSTATUS_DOCTYPE> DSTATUS_DOCTYPE { get; set; }
    }
}
