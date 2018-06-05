namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DAYWORK_EQUIPMENT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public DateTime WORKDATE { get; set; }

        [Required]
        [StringLength(500)]
        public string ITEM { get; set; }

        [Column(TypeName = "numeric")]
        public decimal QUANTITY { get; set; }

        [Column(TypeName = "numeric")]
        public decimal PRICE { get; set; }

        public bool IS_MATERIAL { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
