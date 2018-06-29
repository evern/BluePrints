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
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public DateTime HSE_DATE { get; set; }

        public double? QTY_STAFF { get; set; }

        public double? QTY_MGMT { get; set; }

        public double? QTY_HSE { get; set; }

        public double? QTY_CONTRACTOR { get; set; }

        public double? QTY_DAYSONSITE { get; set; }

        public double? QTY_HRSADAY { get; set; }

        public double? QTY_DIESEL { get; set; }

        public double? QTY_PETROL { get; set; }

        public double? QTY_ELECTRIC { get; set; }

        public double? QTY_WATER { get; set; }

        [StringLength(2500)]
        public string HSE_ADVISOR_COMMENT { get; set; }

        [StringLength(2500)]
        public string SITE_MGMT_COMMENT { get; set; }

        public double? INJURIES_REC_LTI { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_LTI_COMMENT { get; set; }

        public double? INJURIES_REC_RWI { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_RWI_COMMENT { get; set; }

        public double? INJURIES_REC_MTI { get; set; }

        [StringLength(500)]
        public string INJURIES_REC_MTI_COMMENT { get; set; }

        public double? INJURIES_OTH_FAI { get; set; }

        [StringLength(500)]
        public string INJURIES_OTH_FAI_COMMENT { get; set; }

        public double? INJURIES_OTH_NWR { get; set; }

        [StringLength(500)]
        public string INJURIES_OTH_NWR_COMMENT { get; set; }

        public double? INCIDENT_DAM { get; set; }

        [StringLength(500)]
        public string INCIDENT_DAM_COMMENT { get; set; }

        public double? INCIDENT_ENV { get; set; }

        [StringLength(500)]
        public string INCIDENT_ENV_COMMENT { get; set; }

        public double? INCIDENT_FIRE { get; set; }

        [StringLength(500)]
        public string INCIDENT_FIRE_COMMENT { get; set; }

        public double? INCIDENT_MAJOR_ENV { get; set; }

        [StringLength(500)]
        public string INCIDENT_MAJOR_ENV_COMMENT { get; set; }

        public double? INCIDENT_HSE_BREACH { get; set; }

        [StringLength(500)]
        public string INCIDENT_HSE_BREACH_COMMENT { get; set; }

        public double? INCIDENT_NOTICE { get; set; }

        [StringLength(500)]
        public string INCIDENT_NOTICE_COMMENT { get; set; }

        public double? KPI_NM { get; set; }

        [StringLength(500)]
        public string KPI_NM_COMMENT { get; set; }

        public double? KPI_PRESTART { get; set; }

        [StringLength(500)]
        public string KPI_PRESTART_COMMENT { get; set; }

        public double? KPI_TOOLBOX { get; set; }

        [StringLength(500)]
        public string KPI_TOOLBOX_COMMENT { get; set; }

        public double? KPI_HSE_COMMITTEE { get; set; }

        [StringLength(500)]
        public string KPI_HSE_COMMITTEE_COMMENT { get; set; }

        public double? KPI_HAZOB { get; set; }

        [StringLength(500)]
        public string KPI_HAZOB_COMMENT { get; set; }

        public double? KPI_SWO { get; set; }

        [StringLength(500)]
        public string KPI_SWO_COMMENT { get; set; }

        public double? KPI_TAKE5 { get; set; }

        [StringLength(500)]
        public string KPI_TAKE5_COMMENT { get; set; }

        public double? KPI_DRILL { get; set; }

        [StringLength(500)]
        public string KPI_DRILL_COMMENT { get; set; }

        public double? KPI_INSPECTION { get; set; }

        [StringLength(500)]
        public string KPI_INSPECTION_COMMENT { get; set; }

        public double? KPI_INSPECTION_FREQ { get; set; }

        [StringLength(500)]
        public string KPI_INSPECTION_FREQ_COMMENT { get; set; }

        public double? KPI_CORRECTIVE_ACT { get; set; }

        [StringLength(500)]
        public string KPI_CORRECTIVE_ACT_COMMENT { get; set; }

        public double? KPI_CORRECTIVE_ACT_CLOSED { get; set; }

        [StringLength(500)]
        public string KPI_CORRECTIVE_ACT_CLOSED_COMMENT { get; set; }

        public double? KPI_WEEKLY_HSE { get; set; }

        [StringLength(500)]
        public string KPI_WEEKLY_HSE_COMMENT { get; set; }

        public double? KPI_RISK_REGISTER { get; set; }

        [StringLength(500)]
        public string KPI_RISK_REGISTER_COMMENT { get; set; }

        public double? TRAIN_COMPLIANCE { get; set; }

        [StringLength(500)]
        public string TRAIN_COMPLIANCE_COMMENT { get; set; }

        public double? TRAIN_VOC { get; set; }

        [StringLength(500)]
        public string TRAIN_VOC_COMMENT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
