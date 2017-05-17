namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_RISK
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_AREA { get; set; }

        [Required]
        [StringLength(150)]
        public string NUMBER { get; set; }

        [StringLength(300)]
        public string TITLE { get; set; }

        public Register_HazardGroup? HAZARD_GROUP { get; set; }

        [StringLength(1000)]
        public string HAZARD_TYPE { get; set; }

        [StringLength(1000)]
        public string HAZARD_CAUSE { get; set; }

        public Register_RiskLikelihood? RISK_LIKELIHOOD { get; set; }

        public Register_RiskConsequence? RISK_CONSEQUENCES { get; set; }

        public Register_RiskRanking? RISK_RANKING { get; set; }

        [StringLength(500)]
        public string CONTROL_MEASURES { get; set; }

        public Register_RiskLikelihood? RESIDUE_RISK_LIKELIHOOD { get; set; }

        public Register_RiskConsequence? RESIDUE_RISK_CONSEQUENCES { get; set; }

        public Register_RiskRanking? RESIDUE_RISK_RANKING { get; set; }

        [StringLength(1000)]
        public string RESIDUE_HAZARD { get; set; }

        [StringLength(1000)]
        public string FURTHER_ACTION { get; set; }

        public DateTime DATE_IDENTIFIED { get; set; }

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
