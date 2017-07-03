namespace BluePrints.Data
{
    using BluePrints.Common.ViewModel.Reporting;
    using Common;
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class BASELINE_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ORIGINAL { get; set; }

        public Guid? GUID_BASELINE { get; set; }

        public Guid? GUID_VARIATION { get; set; }

        public Guid? GUID_PHASE { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_SUBAREA { get; set; }

        public Guid? GUID_WORKPACK { get; set; }

        public Guid? GUID_DEPARTMENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_DOCTYPE { get; set; }

        public Guid? GUID_STATUS { get; set; }

        public Guid? GUID_USER { get; set; }

        [StringLength(200)]
        public string INTERNAL_NUM { get; set; }

        [StringLength(200)]
        public string CLIENT_NUM { get; set; }

        public DeliverableType DELIVERABLE_TYPE { get; set; }

        [StringLength(500)]
        public string PRIMARY_TITLE { get; set; }

        [StringLength(500)]
        public string SECONDARY_TITLE { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        public decimal ESTIMATED_HOURS { get; set; }

        public decimal DC_HOURS { get; set; }

        [StringLength(50)]
        public string REVISION_NUMBER { get; set; }

        [Required]
        public int DISCIPLINE_NUM { get; set; }

        public decimal? P6_ASSIGNMENT_STARTUNIT { get; set; }

        public bool BY_DURATION { get; set; }

        public DateTime? TARGET_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? CANCELLED { get; set; }

        public Guid? CANCELLEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual AREA AREA1 { get; set; }

        public virtual BASELINE BASELINE { get; set; }

        public virtual DELIVERABLES_STATUS DELIVERABLES_STATUS { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual DOCTYPE DOCTYPE { get; set; }

        public virtual PHASE PHASE { get; set; }

        public virtual USER USER { get; set; }

        public virtual VARIATION VARIATION { get; set; }

        public virtual WORKPACK WORKPACK { get; set; }
    }
}
