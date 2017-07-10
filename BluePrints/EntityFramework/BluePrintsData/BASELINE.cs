namespace BluePrints.Data
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BASELINE")]
    public partial class BASELINE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BASELINE()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            VARIATION = new HashSet<VARIATION>();
            VARIATION1 = new HashSet<VARIATION>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        [Required]
        [StringLength(50)]
        public string REVISION { get; set; }

        [StringLength(100)]
        public string COMMENTS { get; set; }

        public decimal? ACTUAL_UNITS { get; set; }

        public decimal? BUDGETED_UNITS { get; set; }

        public bool ALLOW_EXCEED { get; set; }

        public BaselineStatus STATUS { get; set; }

        [StringLength(20)]
        public string P6BASELINE_NAME { get; set; }

        [StringLength(20)]
        public string P6MODBASELINE_NAME { get; set; }

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
        public virtual ICollection<VARIATION> VARIATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VARIATION> VARIATION1 { get; set; }
    }
}
