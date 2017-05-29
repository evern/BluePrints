namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_HOLD
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid? GUID_AREA { get; set; }

        [StringLength(150)]
        public string NUMBER { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        [StringLength(1000)]
        public string REFERENCE { get; set; }

        public Guid RAISED_BY { get; set; }

        [StringLength(1000)]
        public string ACTION { get; set; }

        public DateTime DATE_RAISED { get; set; }
        
        public DateTime? DATE_CLOSED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
