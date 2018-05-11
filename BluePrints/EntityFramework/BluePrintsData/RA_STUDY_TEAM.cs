namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RA_STUDY_TEAM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_STUDY { get; set; }

        public Guid GUID_USER { get; set; }

        [StringLength(500)]
        public string STUDY_ROLE { get; set; }

        [StringLength(50)]
        public string STUDY_INITIALS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual RA_STUDY RA_STUDY { get; set; }

        public virtual USER USER { get; set; }
    }
}
