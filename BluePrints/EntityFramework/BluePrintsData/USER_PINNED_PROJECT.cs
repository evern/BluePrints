namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("USER_PINNED_PROJECT")]
    public partial class USER_PINNED_PROJECT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_USER { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual USER USER { get; set; }
    }
}
