namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("AREA")]
    public partial class AREA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AREA()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            BASELINE_ITEM1 = new HashSet<BASELINE_ITEM>();
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
            ESTIMATION_DIRECT_ITEM1 = new HashSet<ESTIMATION_DIRECT_ITEM>();
            WORKPACK = new HashSet<WORKPACK>();
            WORKPACK1 = new HashSet<WORKPACK>();
            REGISTER_CHANGE = new HashSet<REGISTER_CHANGE>();
            REGISTER_HOLD = new HashSet<REGISTER_HOLD>();
            REGISTER_ISSUE = new HashSet<REGISTER_ISSUE>();
            REGISTER_LL = new HashSet<REGISTER_LL>();
            REGISTER_NC = new HashSet<REGISTER_NC>();
            REGISTER_RISK = new HashSet<REGISTER_RISK>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_PARENT { get; set; }

        [Required]
        [StringLength(100)]
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
        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM1 { get; set; }

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
        public virtual ICollection<WORKPACK> WORKPACK { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WORKPACK> WORKPACK1 { get; set; }
    }
}
