namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RA_GUIDE_PROMPT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RA_GUIDE_PROMPT()
        {
            RA_GUIDE_SUBPROMPT = new HashSet<RA_GUIDE_SUBPROMPT>();
            RA_STUDY_DATA = new HashSet<RA_STUDY_DATA>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_STUDY_TYPE { get; set; }

        [Required]
        [StringLength(500)]
        public string GUIDE_PROMPT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual RA_STUDY_TYPE RA_STUDY_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RA_GUIDE_SUBPROMPT> RA_GUIDE_SUBPROMPT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RA_STUDY_DATA> RA_STUDY_DATA { get; set; }
    }
}
