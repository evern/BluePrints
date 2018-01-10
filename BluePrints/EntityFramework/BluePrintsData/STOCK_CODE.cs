namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_CODE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ORIGINAL { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        public Guid GUID_DEPARTMENT { get; set; }

        public Guid? GUID_COMMODITY_CODE { get; set; }

        [Required]
        public Guid GUID_DISCIPLINE { get; set; }

        public StockCodeType STOCK_CODE_TYPE { get; set; }

        [Required]
        [StringLength(1000)]
        public string NAME { get; set; }

        [StringLength(1000)]
        public string TYPE { get; set; }

        [StringLength(1000)]
        public string SPEC { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(50)]
        public string CODE { get; set; }

        [StringLength(50)]
        public string UOM { get; set; }

        [Column(TypeName = "numeric")]
        public decimal RATE_SUPPLY { get; set; }

        [Column(TypeName = "numeric")]
        public decimal HOURS_INSTALL { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATE_ITEM> ESTIMATE_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATE_ITEM> ESTIMATE_ITEM1 { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }
    }
}
