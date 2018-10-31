namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DSTATUS_DOCTYPE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_STATUS { get; set; }

        public Guid? GUID_DOCTYPE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DELIVERABLES_STATUS DELIVERABLES_STATUS { get; set; }

        public virtual DOCTYPE DOCTYPE { get; set; }
    }
}
