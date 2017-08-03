namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_GROUP
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public STOCK_GROUP()
        {
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        [Required]
        [StringLength(50)]
        public string CODE { get; set; }

        [StringLength(200)]
        public string DESCRIPTION { get; set; }

        [StringLength(10)]
        public string UOM { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
