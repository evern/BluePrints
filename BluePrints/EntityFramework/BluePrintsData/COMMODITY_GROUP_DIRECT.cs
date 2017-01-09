namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class COMMODITY_GROUP_DIRECT
    {
        public COMMODITY_GROUP_DIRECT()
        {
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        public Guid? GUID_COMMODITYCODE { get; set; }

        public Guid? GUID_PARENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_SUPPLY { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? HOURS_INSTALL { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_FREIGHT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual PROGRESS_ITEM PROGRESS_ITEM { get; set; }

        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }
    }
}
