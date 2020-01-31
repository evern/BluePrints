namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ROLE_PERMISSION
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ROLE { get; set; }

        [Required]
        [StringLength(500)]
        public string PERMISSION { get; set; }

        public bool ISREADONLY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ROLE ROLE { get; set; }
    }
}
