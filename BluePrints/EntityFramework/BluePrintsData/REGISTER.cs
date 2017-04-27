namespace BluePrints.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("REGISTER")]
    public partial class REGISTER
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_AREA { get; set; }

        [StringLength(50)]
        public string UNIQUE_DI_NUM { get; set; }

        [StringLength(50)]
        public string UNIQUE_DC_NUM { get; set; }

        [StringLength(50)]
        public string UNIQUE_R_NUM { get; set; }

        [StringLength(10)]
        public string UNIQUE_H_NUM { get; set; }

        [Required]
        [StringLength(150)]
        public string TITLE { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION_H { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        [StringLength(1000)]
        public string FINAL_RESOLUTION { get; set; }

        [StringLength(1000)]
        public string CLIENT_NOTIFICATION { get; set; }

        [StringLength(50)]
        public string NOTIFIED_PERSON { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATERAISED_DI { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATERAISED_DC { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATERAISED_H { get; set; }

        [StringLength(50)]
        public string RAISEDBY_DI { get; set; }

        [StringLength(50)]
        public string RAISEDBY_DC { get; set; }

        [StringLength(50)]
        public string RAISEDBY_H { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATECLOSED_DI { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATECLOSED_DC { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATECLOSED_H { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEAPPROVED_DC { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEIDENTIFIED_R { get; set; }

        [StringLength(150)]
        public string CLOSING_MECHANISM { get; set; }

        public bool DWG_ACTIONED { get; set; }

        public bool SCHEDULE_IMPACT { get; set; }

        public bool COST_IMPACT { get; set; }

        public int IMPACT_TYPE { get; set; }

        public bool CHANGE_APPROVED { get; set; }

        public bool INTERDISC_CHECKED { get; set; }

        public int? HAZARD_GROUP { get; set; }

        [StringLength(500)]
        public string HAZARD_TYPE { get; set; }

        [StringLength(500)]
        public string HAZARD_CAUSE { get; set; }

        public int? RISK_LIKELIHOOD { get; set; }

        public int? RISK_CONSEQ { get; set; }

        [StringLength(500)]
        public string CONTROL_MEASURES { get; set; }

        public int? RRISK_LIKELIHOOD { get; set; }

        public int? RRISK_CONSEQ { get; set; }

        [StringLength(500)]
        public string RHAZARD { get; set; }

        [StringLength(500)]
        public string FURTHER_ACTION { get; set; }

        [StringLength(500)]
        public string REFERENCE { get; set; }

        [StringLength(500)]
        public string ACTION { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
