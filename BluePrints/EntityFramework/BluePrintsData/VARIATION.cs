namespace BluePrints.Data
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("VARIATION")]
    public partial class VARIATION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VARIATION()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            VARIATION_ITEM = new HashSet<VARIATION_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_ORIBASELINE { get; set; }

        public Guid? GUID_BASELINE { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(500)]
        public string COMMENTS { get; set; }

        public VariationType TYPE { get; set; }

        public DateTime? SUBMITTED { get; set; }

        public Guid? SUBMITTEDBY { get; set; }

        public DateTime? APPROVED { get; set; }

        public Guid? APPROVEDBY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual BASELINE BASELINE { get; set; }

        public virtual BASELINE BASELINE1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VARIATION_ITEM> VARIATION_ITEM { get; set; }
    }
}
