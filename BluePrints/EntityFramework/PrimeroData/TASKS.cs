namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TASKS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? PARENTSEQNO { get; set; }

        public int? EVENTTYPE { get; set; }

        public DateTime? START_DATETIME { get; set; }

        public DateTime? END_DATETIME { get; set; }

        public int? OPTIONS { get; set; }

        [StringLength(60)]
        public string SUBJECT { get; set; }

        [StringLength(50)]
        public string COMPANYID { get; set; }

        public int? CONTACTSEQNO { get; set; }

        public int? OPPORTUNITYSEQNO { get; set; }

        public int? RECURRENCEINDEX { get; set; }

        [Column(TypeName = "image")]
        public byte[] RECURRENCEINFO { get; set; }

        public int? ASSIGNED_TO { get; set; }

        public int? ASSIGNED_BY { get; set; }

        public DateTime? REMINDERDATE { get; set; }

        public int? REMINDERMINUTES { get; set; }

        [Required]
        [StringLength(1)]
        public string COMPLETED { get; set; }

        public DateTime? COMPLETED_DATETIME { get; set; }

        public int? STATE { get; set; }

        public int? TYPE { get; set; }

        public int? STATUS { get; set; }

        [StringLength(20)]
        public string PRIORITY { get; set; }

        public int? LABELCOLOR { get; set; }

        public DateTime? ACTUALSTART { get; set; }

        public DateTime? ACTUALFINISH { get; set; }

        [StringLength(255)]
        public string OUTLOOKENTRYID { get; set; }

        public int? CREATEDBY { get; set; }

        public DateTime CREATEDATE { get; set; }

        public int? MODIFIEDBY { get; set; }

        public DateTime? MODIFIEDDATE { get; set; }

        public int ACTIVITY_TYPE { get; set; }

        public double COMPLETED_PERCENT { get; set; }

        [Required]
        [StringLength(1)]
        public string DELETED_FLAG { get; set; }

        [Required]
        [StringLength(1)]
        public string SYNC_ACTIVITY { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }

        public int? CAMPAIGN_SEQNO { get; set; }

        public int JOBNO { get; set; }

        public int SU_SEQNO { get; set; }

        public int SUBS_HDR_SEQNO { get; set; }

        public int? RESOURCEALLOC_SEQNO { get; set; }

        [StringLength(5500)]
        public string DETAILS { get; set; }
    }
}
