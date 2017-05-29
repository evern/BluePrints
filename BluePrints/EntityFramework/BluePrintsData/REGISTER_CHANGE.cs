namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_CHANGE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_AREA { get; set; }

        [StringLength(150)]
        public string NUMBER { get; set; }

        [StringLength(150)]
        public string CLIENT_NUMBER { get; set; }

        [StringLength(300)]
        public string TITLE { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        public ScheduleImpact? SCHEDULE_IMPACT { get; set; }

        public ScheduleImpact? COST_IMPACT { get; set; }

        public Register_ImpactType IMPACT_TYPE { get; set; }

        public bool INTERDISC_CHECK_COMPLETE { get; set; }

        public DateTime DATE_RAISED { get; set; }

        public bool APPROVED { get; set; }

        public DateTime? DATE_CLOSED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
