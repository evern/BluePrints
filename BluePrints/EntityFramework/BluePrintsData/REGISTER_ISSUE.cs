namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_ISSUE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_CHANGE { get; set; }

        [StringLength(150)]
        public string NUMBER { get; set; }

        [StringLength(300)]
        public string TITLE { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        [StringLength(1000)]
        public string PROPOSED_SOLUTION { get; set; }

        [StringLength(1000)]
        public string FINAL_RESOLUTION { get; set; }

        [StringLength(300)]
        public string CLIENT_NOTIFICATION { get; set; }

        [StringLength(300)]
        public string NOTIFIED_PERSON { get; set; }

        [StringLength(300)]
        public string WAY_CLOSED { get; set; }

        [StringLength(300)]
        public string CLOSED_NOTIFIED_PERSON { get; set; }

        public DateTime? DATE_RAISED { get; set; }

        public DateTime? DATE_CLOSED { get; set; }

        public ActionedOnDrawing? ACTIONED_ON_DWG { get; set; }

        public ScheduleImpact? SCHEDULE_IMPACT { get; set; }

        public ScheduleImpact? COST_IMPACT { get; set; }

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
