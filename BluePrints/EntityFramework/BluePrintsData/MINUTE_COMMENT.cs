namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MINUTE_COMMENT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_AGENDA { get; set; }

        [Required]
        [StringLength(2000)]
        public string COMMENTS { get; set; }

        public Guid? COMMENTS_BY { get; set; }

        public DateTime DATE_RAISED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual MINUTE_AGENDA MINUTE_AGENDA { get; set; }
    }
}
