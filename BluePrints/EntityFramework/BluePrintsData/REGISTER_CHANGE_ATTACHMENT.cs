namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_CHANGE_ATTACHMENT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_REGISTER_CHANGE { get; set; }

        [Required]
        [StringLength(100)]
        public string ATTACHMENT_NAME { get; set; }

        [Required]
        [StringLength(4000)]
        public string ATTACHMENT_PATH { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual REGISTER_CHANGE REGISTER_CHANGE { get; set; }
    }
}
