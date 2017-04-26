namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATION_INDIRECT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ESTIMATION_INDIRECT()
        {
            ESTIMATION_INDIRECT_ITEM = new HashSet<ESTIMATION_INDIRECT_ITEM>();
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

        public int STATUS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATION_INDIRECT_ITEM> ESTIMATION_INDIRECT_ITEM { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
