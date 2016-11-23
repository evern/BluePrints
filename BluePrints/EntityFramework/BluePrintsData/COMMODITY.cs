namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("COMMODITY")]
    public partial class COMMODITY
    {
        public COMMODITY()
        {
            ESTIMATION_ITEM = new HashSet<ESTIMATION_ITEM>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid GUID { get; set; }

        public Guid? GUID_PARENT { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid GUID_COMMODITYCODE { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_SUPPLY { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? HOURS_INSTALL { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_FREIGHT { get; set; }

        [StringLength(1000)]
        public string ITEM_DESC { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual ICollection<ESTIMATION_ITEM> ESTIMATION_ITEM { get; set; }
    }
}
