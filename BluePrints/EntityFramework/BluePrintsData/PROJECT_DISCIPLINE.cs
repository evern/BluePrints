namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROJECT_DISCIPLINE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid GUID_DISCIPLINE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
