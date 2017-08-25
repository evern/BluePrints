namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MINUTE_TITLE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MINUTE_TITLE()
        {
            MINUTE_AGENDA = new HashSet<MINUTE_AGENDA>();
            MINUTE_TITLE1 = new HashSet<MINUTE_TITLE>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PARENT { get; set; }

        public int SORTORDER { get; set; }

        public bool ISEXPANDED { get; set; }

        [Required]
        [StringLength(50)]
        public string NUMBER { get; set; }

        [Required]
        [StringLength(1000)]
        public string NAME { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MINUTE_AGENDA> MINUTE_AGENDA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MINUTE_TITLE> MINUTE_TITLE1 { get; set; }

        public virtual MINUTE_TITLE MINUTE_TITLE2 { get; set; }
    }
}
