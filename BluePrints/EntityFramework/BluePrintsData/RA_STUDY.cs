namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RA_STUDY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RA_STUDY()
        {
            RA_STUDY_USER = new HashSet<RA_STUDY_USER>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid GUID_STUDY_TYPE { get; set; }

        [Required]
        [StringLength(500)]
        public string NAME { get; set; }

        public Guid? GUID_FACILITATOR { get; set; }

        public Guid? GUID_MINUTESBY { get; set; }

        public DateTime? START_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual USER USER { get; set; }

        public virtual USER USER1 { get; set; }

        public virtual RA_STUDY_TYPE RA_STUDY_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RA_STUDY_USER> RA_STUDY_USER { get; set; }
    }
}
