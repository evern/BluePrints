namespace BluePrints.Data
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class COMMODITY_CODE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public COMMODITY_CODE()
        {
            COMMODITY_GROUP_DIRECT = new HashSet<COMMODITY_GROUP_DIRECT>();
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
            ESTIMATION_INDIRECT_ITEM = new HashSet<ESTIMATION_INDIRECT_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        public Guid GUID_PARENT { get; set; }

        public Guid? GUID_COMMODITY_GROUP_DIRECT { get; set; }

        public Guid? GUID_GROUP_PARENT { get; set; }

        public Guid? GUID_DEPARTMENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_INDIRECTTYPE { get; set; }

        public CommodityCodeType COMMODITYCODETYPE { get; set; }

        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(100)]
        public string TYPE { get; set; }

        [StringLength(100)]
        public string SPEC { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [StringLength(50)]
        public string CODE { get; set; }

        [StringLength(100)]
        public string FULLCODE { get; set; }

        [StringLength(50)]
        public string UOM { get; set; }

        public int SORTORDER { get; set; }

        public bool ISEXPANDED { get; set; }

        public bool ISQUANTIFIABLE { get; set; }

        public bool ISGROUPHEADER { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_SUPPLY { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? HOURS_INSTALL { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_FREIGHT { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_PLANT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual INDIRECT_TYPE INDIRECT_TYPE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATION_INDIRECT_ITEM> ESTIMATION_INDIRECT_ITEM { get; set; }
    }
}
