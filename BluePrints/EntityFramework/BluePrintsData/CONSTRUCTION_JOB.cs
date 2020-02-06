namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CONSTRUCTION_JOB
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_PARENT { get; set; }

        public Guid? GUID_SUBJOB { get; set; }

        public Guid? GUID_WORKPACK { get; set; }

        public Guid? GUID_PHASE { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_SUBAREA { get; set; }

        public Guid? GUID_DEPARTMENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_COMMODITY_CODE { get; set; }

        public decimal? PRODUCTIVITY_OVERRIDE { get; set; }

        [Required]
        public int DISCIPLINE_NUM { get; set; }

        [StringLength(1000)]
        public string NAME { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        [StringLength(100)]
        public string P6ACTIVITYMAP { get; set; }

        [StringLength(100)]
        public string VARIATION_CODE { get; set; }

        [StringLength(50)]
        public string COMMODITY_CODE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual AREA AREA1 { get; set; }

        public virtual PHASE PHASE { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODES { get; set; }

        public virtual SUBJOB SUBJOB { get; set; }

        public virtual WORKPACK WORKPACK { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
