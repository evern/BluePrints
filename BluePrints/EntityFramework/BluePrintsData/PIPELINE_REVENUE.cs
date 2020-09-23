namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PIPELINE_REVENUE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PIPELINE { get; set; }

        public decimal? REVENUE { get; set; }

        public DateTime DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PIPELINE PIPELINE { get; set; }
    }
}
