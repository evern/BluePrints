namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HSE")]
    public partial class HSE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HSE()
        {
            HSE_INCIDENT = new HashSet<HSE_INCIDENT>();
            HSE_INJURY = new HashSet<HSE_INJURY>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public DateTime HSE_DATE { get; set; }

        public decimal QTY_STAFF { get; set; }

        public decimal QTY_MGMT { get; set; }

        public decimal QTY_HSE { get; set; }

        public decimal QTY_CONTRACTOR { get; set; }

        public decimal QTY_DAYSONSITE { get; set; }

        public decimal QTY_HRSADAY { get; set; }

        public decimal QTY_DIESEL { get; set; }

        public decimal QTY_PETROL { get; set; }

        public decimal QTY_ELECTRIC { get; set; }

        public decimal QTY_WATER { get; set; }

        [StringLength(2500)]
        public string HSE_ADVISOR_COMMENT { get; set; }

        public int? SITE_MGMT_STAFFNO { get; set; }

        [StringLength(2500)]
        public string SITE_MGMT_COMMENT { get; set; }

        public DateTime? SITE_MGMT_COMMENT_DATE { get; set; }

        public decimal INJURIES_REC_LTI { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_LTI_COMMENT { get; set; }

        public decimal INJURIES_REC_RWI { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_RWI_COMMENT { get; set; }

        public decimal INJURIES_REC_MTI { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_MTI_COMMENT { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_TOTAL_COMMENT { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_FREQ_COMMENT { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_ALL_COMMENT { get; set; }

        public decimal INJURIES_OTH_FAI { get; set; }

        [StringLength(500)]
        public string INJURIES_OTH_FAI_COMMENT { get; set; }

        public decimal INJURIES_OTH_NWR { get; set; }

        [StringLength(500)]
        public string INJURIES_OTH_NWR_COMMENT { get; set; }

        public decimal INCIDENT_ENV { get; set; }

        [StringLength(500)]
        public string INCIDENT_ENV_COMMENT { get; set; }

        public decimal INCIDENT_DAM { get; set; }

        [StringLength(500)]
        public string INCIDENT_DAM_COMMENT { get; set; }

        public decimal INCIDENT_PDT { get; set; }

        [StringLength(500)]
        public string INCIDENT_PDT_COMMENT { get; set; }

        public decimal INCIDENT_BAC { get; set; }

        [StringLength(500)]
        public string INCIDENT_BAC_COMMENT { get; set; }

        public decimal INCIDENT_HSE_BREACH { get; set; }

        [StringLength(500)]
        public string INCIDENT_HSE_BREACH_COMMENT { get; set; }

        public decimal INCIDENT_NOTICE { get; set; }

        [StringLength(500)]
        public string INCIDENT_NOTICE_COMMENT { get; set; }

        [StringLength(500)]
        public string INCIDENT_TOTAL_COMMENT { get; set; }

        public decimal KPI_NM { get; set; }

        [StringLength(500)]
        public string KPI_NM_COMMENT { get; set; }

        public decimal KPI_PRESTART { get; set; }

        public int KPI_PRESTART_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_PRESTART_COMMENT { get; set; }

        public decimal KPI_TOOLBOX { get; set; }

        public int KPI_TOOLBOX_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_TOOLBOX_COMMENT { get; set; }

        public decimal KPI_HAZOB { get; set; }

        public int KPI_HAZOB_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_HAZOB_COMMENT { get; set; }

        public decimal KPI_SWO { get; set; }

        public int KPI_SWO_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_SWO_COMMENT { get; set; }

        public decimal KPI_TAKE5 { get; set; }

        [StringLength(500)]
        public string KPI_TAKE5_COMMENT { get; set; }

        public decimal KPI_DRILL { get; set; }

        public int KPI_DRILL_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_DRILL_COMMENT { get; set; }

        public decimal KPI_INSPECTION { get; set; }

        public int KPI_INSPECTION_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_INSPECTION_COMMENT { get; set; }

        public decimal KPI_SUPERVISOR_PRIMER { get; set; }

        public int KPI_SUPERVISOR_PRIMER_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_SUPERVISOR_PRIMER_COMMENT { get; set; }

        public decimal KPI_HSE_PRIMER { get; set; }

        public int KPI_HSE_PRIMER_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_HSE_PRIMER_COMMENT { get; set; }

        public decimal KPI_CORRECTIVE_ACT { get; set; }

        public int KPI_CORRECTIVE_ACT_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_CORRECTIVE_ACT_COMMENT { get; set; }

        public decimal KPI_CORRECTIVE_ACT_CLOSED { get; set; }

        public int KPI_CORRECTIVE_ACT_CLOSED_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_CORRECTIVE_ACT_CLOSED_COMMENT { get; set; }

        public decimal KPI_HSE_RECOGNITION { get; set; }

        public int KPI_HSE_RECOGNITION_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_HSE_RECOGNITION_COMMENT { get; set; }

        public decimal KPI_RISK_REGISTER { get; set; }

        public int KPI_RISK_REGISTER_CRITERIA { get; set; }

        [StringLength(500)]
        public string KPI_RISK_REGISTER_COMMENT { get; set; }

        public decimal TRAIN_COMPLIANCE { get; set; }

        public int TRAIN_COMPLIANCE_CRITERIA { get; set; }

        [StringLength(500)]
        public string TRAIN_COMPLIANCE_COMMENT { get; set; }

        public decimal TRAIN_VOC { get; set; }

        [StringLength(500)]
        public string TRAIN_VOC_COMMENT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HSE_INCIDENT> HSE_INCIDENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HSE_INJURY> HSE_INJURY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
