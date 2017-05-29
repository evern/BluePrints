namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_NC
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        [StringLength(150)]
        public string NUMBER { get; set; }

        public RegisterRaisedByType? RAISEDBY_TYPE { get; set; }

        [StringLength(300)]
        public string RAISEDBY { get; set; }

        [StringLength(300)]
        public string TITLE { get; set; }

        [StringLength(300)]
        public string DESCRIPTION { get; set; }

        [StringLength(1000)]
        public string CAUSE { get; set; }

        public CorrectiveActionType? CORRECTIVE_ACTION_TYPE { get; set; }

        [StringLength(1500)]
        public string CORRECTIVE_ACTION { get; set; }

        public decimal? ESTIMATED_RECTIFICATION_COST { get; set; }

        public DateTime? DATE_IDENTIFIED { get; set; }

        public DateTime? DATE_CLOSED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}

