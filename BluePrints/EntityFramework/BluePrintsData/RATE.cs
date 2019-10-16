namespace BluePrints.Data
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RATE")]
    public partial class RATE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_PHASE { get; set; }

        public CostType COST_TYPE { get; set; }

        public PhaseType PHASE_TYPE { get; set; }

        public ChargeType CHARGE_TYPE { get; set; }

        public Guid? GUID_DEPARTMENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_DOCTYPE { get; set; }

        public Guid? GUID_COMMODITY { get; set; }

        public decimal? MANAGER_RATE { get; set; }

        public decimal? PRINCIPAL_RATE { get; set; }

        public decimal? LEAD_RATE { get; set; }

        public decimal? SENIOR_RATE { get; set; }

        public decimal? ENGINEER_RATE { get; set; }

        public decimal? GRADUATE_RATE { get; set; }

        public decimal? UNDERGRADUATE_RATE { get; set; }

        public decimal? MANAGER_PERCENT { get; set; }

        public decimal? PRINCIPAL_PERCENT { get; set; }

        public decimal? LEAD_PERCENT { get; set; }

        public decimal? SENIOR_PERCENT { get; set; }

        public decimal? ENGINEER_PERCENT { get; set; }

        public decimal? GRADUATE_PERCENT { get; set; }

        public decimal? UNDERGRADUATE_PERCENT { get; set; }

        [Column("RATE")]
        public decimal? RATE1 { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PHASE PHASE { get; set; }

        public virtual DOCTYPE DOCTYPE { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }
    }
}
