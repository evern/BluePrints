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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DISCIPLINE()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            STOCK_CODE = new HashSet<STOCK_CODE>();
            COMMODITY_CODE = new HashSet<COMMODITY_CODE>();
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
            RATE = new HashSet<RATE>();
            REGISTER_LL = new HashSet<REGISTER_LL>();
            REGISTER_NC = new HashSet<REGISTER_NC>();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STOCK_CODE> STOCK_CODE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<COMMODITY_CODE> COMMODITY_CODE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RATE> RATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_LL> REGISTER_LL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_NC> REGISTER_NC { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WORKPACK> WORKPACK { get; set; }
    }
}
