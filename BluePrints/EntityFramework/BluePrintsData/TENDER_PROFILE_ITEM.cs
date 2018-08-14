namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TENDER_PROFILE_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_TENDER_PROFILE { get; set; }

        public Guid GUID_DEPARTMENT { get; set; }

        public Guid GUID_DISCIPLINE { get; set; }

        public decimal HOURS_PERCENTAGE { get; set; }

        public decimal SCHEDULE_START_PERCENTAGE { get; set; }

        public decimal SCHEDULE_FINISH_PERCENTAGE { get; set; }

        public BellCurveShape BELLCURVESHAPE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual TENDER_PROFILE TENDER_PROFILE { get; set; }
    }
}
