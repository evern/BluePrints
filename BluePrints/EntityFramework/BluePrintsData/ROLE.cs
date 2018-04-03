namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ROLE")]
    public partial class ROLE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ROLE()
        {
            ROLE_COMMODITY = new HashSet<ROLE_COMMODITY>();
            ROLE_PERMISSION = new HashSet<ROLE_PERMISSION>();
            USER = new HashSet<USER>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid PARENTGUID { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        public bool ISMANAGER { get; set; }

        public bool ISEXPANDED { get; set; }
        [NotMapped]
        public int? OLDSORTORDER { get; set; }


        public int SORTORDER { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ROLE_PERMISSION> ROLE_PERMISSION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ROLE_COMMODITY> ROLE_COMMODITY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USER> USER { get; set; }
    }
}
