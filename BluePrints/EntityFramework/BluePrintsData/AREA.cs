namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AREA")]
    public partial class AREA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AREA()
        {
            AREA1 = new HashSet<AREA>();
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            BASELINE_ITEM1 = new HashSet<BASELINE_ITEM>();
            CONSTRUCTION_JOB = new HashSet<CONSTRUCTION_JOB>();
            CONSTRUCTION_JOB1 = new HashSet<CONSTRUCTION_JOB>();
            REGISTER_CHANGE = new HashSet<REGISTER_CHANGE>();
            REGISTER_HOLD = new HashSet<REGISTER_HOLD>();
            REGISTER_ISSUE = new HashSet<REGISTER_ISSUE>();
            REGISTER_LL = new HashSet<REGISTER_LL>();
            REGISTER_NC = new HashSet<REGISTER_NC>();
            REGISTER_RISK = new HashSet<REGISTER_RISK>();
            SUBJOB = new HashSet<SUBJOB>();
            SUBJOB1 = new HashSet<SUBJOB>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_PARENT { get; set; }

        [Required]
        [StringLength(3)]
        public string INTERNAL_NUM { get; set; }

        [StringLength(100)]
        public string CLIENT_NUM { get; set; }

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
        public virtual ICollection<AREA> AREA1 { get; set; }

        public virtual AREA AREA2 { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONSTRUCTION_JOB> CONSTRUCTION_JOB { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONSTRUCTION_JOB> CONSTRUCTION_JOB1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_CHANGE> REGISTER_CHANGE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_HOLD> REGISTER_HOLD { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_ISSUE> REGISTER_ISSUE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_LL> REGISTER_LL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_NC> REGISTER_NC { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_RISK> REGISTER_RISK { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SUBJOB> SUBJOB { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SUBJOB> SUBJOB1 { get; set; }
    }
}
