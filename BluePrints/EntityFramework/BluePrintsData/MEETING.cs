namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MEETING")]
    public partial class MEETING
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid GUID_MEETING_TYPE { get; set; }

        [StringLength(50)]
        public string NUMBER { get; set; }

        public string VENUE { get; set; }

        public DateTime MEETING_DATE { get; set; }

        public Guid? CHAIRED_BY { get; set; }

        public DateTime? MEETING_START { get; set; }

        public DateTime? MEETING_END { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual MEETING_TYPE MEETING_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MEETING_USER> MEETING_USER { get; set; }
    }
}
