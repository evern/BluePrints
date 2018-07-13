namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ROSTER_STAFF_STATUS
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ROSTER_STAFF { get; set; }

        public DateTime STATUS_DATE { get; set; }

        public RosterStatus STATUS_NO { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ROSTER_STAFF ROSTER_STAFF { get; set; }
    }
}
