namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATION_ITEM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid GUID { get; set; }

        public Guid GUID_ORIGINAL { get; set; }

        public Guid GUID_PARENT { get; set; }

        public Guid GUID_ESTIMATION { get; set; }

        public Guid? GUID_VARIATION { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_SUPPLYWORKPACK { get; set; }

        public Guid? GUID_INSTALLWORKPACK { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid GUID_COMMODITY { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        public decimal ESTIMATED_QUANTITY { get; set; }

        public decimal VAR_QUANTITY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? CANCELLED { get; set; }

        public Guid? CANCELLEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual ESTIMATION ESTIMATION { get; set; }

        public virtual WORKPACK SUPPLYWORKPACK { get; set; }

        public virtual WORKPACK INSTALLWORKPACK { get; set; }
    }
}
