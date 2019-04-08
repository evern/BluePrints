namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VARIATION_CONS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VARIATION_CONS()
        {
            VARIATION_CONS_ITEM = new HashSet<VARIATION_CONS_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public ConstructionVariationStatus? STATUS { get; set; }

        public ConstructionVariationType? TYPE { get; set; }

        [Required]
        [StringLength(50)]
        public string NAME { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public DateTime? SUBMITTED { get; set; }

        public decimal? APPROVED_VALUE { get; set; }

        [StringLength(500)]
        public string CLIENT_REF { get; set; }

        [StringLength(500)]
        public string SUPPORT_DOC { get; set; }

        [StringLength(50)]
        public string P6_ACTIVITY { get; set; }

        [StringLength(500)]
        public string NOTES { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VARIATION_CONS_ITEM> VARIATION_CONS_ITEM { get; set; }
    }
}
