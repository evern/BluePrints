namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MINUTE_AGENDA
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_MINUTE_TITLE { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_PARENT { get; set; }

        public Guid? GUID_ACTION { get; set; }

        [Required]
        [StringLength(4000)]
        public string NAME { get; set; }

        public int PRIORITY { get; set; }

        public Guid? GUID_RAISE_USER { get; set; }

        public Guid? GUID_ACTION_USER { get; set; }

        public DateTime? RAISE_DATE { get; set; }

        public DateTime? DUE_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual MEETING_ACTION MEETING_ACTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MINUTE_COMMENT> MINUTE_COMMENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MINUTE_AGENDA> MINUTE_AGENDA1 { get; set; }

        public virtual MINUTE_AGENDA MINUTE_AGENDA2 { get; set; }

        public virtual MINUTE_AGENDA MINUTE_AGENDA11 { get; set; }

        public virtual MINUTE_AGENDA MINUTE_AGENDA3 { get; set; }

        public virtual MINUTE_TITLE MINUTE_TITLE { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
