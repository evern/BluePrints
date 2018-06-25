namespace BluePrints.Data
{
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

        [Required]
        [StringLength(50)]
        public string STATUS_NAME { get; set; }

        public int? JOBNO { get; set; }

        public int? COSTGROUPNO { get; set; }

        public int? COSTTYPENO { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }
    }
}
