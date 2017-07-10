namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PHASE")]
    public partial class PHASE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PHASE()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            WORKPACK = new HashSet<WORKPACK>();
        }

        [Key]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(100)]
        public string INTERNAL_NUM { get; set; }

        [StringLength(100)]
        public string CLIENT_NUM { get; set; }

        public PhaseType? PHASE_TYPE { get; set; }

        [Required]
        [StringLength(200)]
        public string TITLE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WORKPACK> WORKPACK { get; set; }
    }
}
