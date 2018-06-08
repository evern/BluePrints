namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_HOLD_REF
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_HOLD { get; set; }

        public Guid GUID_BASELINE_ITEM { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual BASELINE_ITEM BASELINE_ITEM { get; set; }

        public virtual REGISTER_HOLD REGISTER_HOLD { get; set; }
    }
}
