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

        [StringLength(150)]
        public string TQ_NUMBER { get; set; }

        [StringLength(150)]
        public string COMMENTS { get; set; }

        [StringLength(150)]
        public string VAR_REF { get; set; }

        public Guid? GUID_RAISEDBY { get; set; }

        public decimal? EPCM_HOURS_IMPACT { get; set; }

        public decimal? CAPEX_IMPACT { get; set; }

        public decimal? AVG_HR_RATE { get; set; }

        [StringLength(300)]
        public string TITLE { get; set; }

        [StringLength(4000)]
        public string DESCRIPTION { get; set; }

        public ScheduleImpact? SCHEDULE_IMPACT { get; set; }

        public ScheduleImpact? COST_IMPACT { get; set; }

        public Register_ImpactType IMPACT_TYPE { get; set; }

        public bool INTERDISC_CHECK_COMPLETE { get; set; }

        public bool? APPROVED { get; set; }

        [StringLength(500)]
        public string CHANGE_PATH { get; set; }

        public DateTime? DATE_RAISED { get; set; }

        public DateTime? DATE_CLOSED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual USER USER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_CHANGE_ATTACHMENT> REGISTER_CHANGE_ATTACHMENT { get; set; }
    }
}
