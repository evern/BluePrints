namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MINUTE_AGENDA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MINUTE_AGENDA()
        {
            MEETING_COMMENT = new HashSet<MEETING_COMMENT>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_MINUTE_TITLE { get; set; }

        public Guid GUID_MEETING { get; set; }

        [Required]
        [StringLength(4000)]
        public string NAME { get; set; }

        public int STATUS { get; set; }

        public Guid? GUID_ACTION_BY_USER { get; set; }

        public Guid? GUID_ACTION_BY_CLIENT { get; set; }

        public DateTime? DUE_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual CLIENT CLIENT { get; set; }

        public virtual MEETING MEETING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MEETING_COMMENT> MEETING_COMMENT { get; set; }

        public virtual MINUTE_TITLE MINUTE_TITLE { get; set; }

        public virtual USER USER { get; set; }
    }
}
