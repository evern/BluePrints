namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VARIATION_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_VARIATION { get; set; }

        public Guid GUID_ORIBASEITEM { get; set; }

        public decimal VARIATION_UNITS { get; set; }

        public VariationAction ACTION { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual VARIATION VARIATION { get; set; }
    }
}
