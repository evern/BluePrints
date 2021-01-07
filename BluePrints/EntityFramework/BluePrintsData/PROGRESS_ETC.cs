namespace BluePrints.Data
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROGRESS_ETC")]
    public partial class PROGRESS_ETC
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROGRESS { get; set; }

        public Guid GUID_ORIBASEITEM { get; set; }

        public decimal ETC_UNITS { get; set; }

        public DateTime ETC_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROGRESS PROGRESS { get; set; }
    }
}
