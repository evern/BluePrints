namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class COMMODITY_GROUP_DIRECT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public COMMODITY_GROUP_DIRECT()
        {
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_COMMODITYCODE { get; set; }

        public Guid? GUID_PARENT { get; set; }

        [Required]
        [StringLength(50)]
        public string GROUP_CODE { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }
    }
}
