namespace BluePrints.Data
{
    using BluePrints.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

    public partial class COMMODITY_CODE
    {
        public COMMODITY_CODE()
        {
            COMMODITY = new HashSet<COMMODITY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid GUID { get; set; }

        public Guid GUID_PARENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_DEPARTMENT { get; set; }

        [Required]
        public CommodityCodeType COMMODITYCODETYPE { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(100)]
        public string TYPE { get; set; }

        [StringLength(100)]
        public string SPEC { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(50)]
        public string CODE { get; set; }

        [Required]
        [StringLength(100)]
        public string FULLCODE { get; set; }

        [StringLength(50)]
        public string UOM { get; set; }

        public int SORTORDER { get; set; }

        public bool ISEXPANDED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ICollection<COMMODITY> COMMODITY { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }
    }
}
