namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class P6_ASSIGNMENT
    {
        [Key]
        public Guid GUID { get; set; }

        public bool ISMODIFIEDBASELINE { get; set; }

        public Guid GUID_ORIGINAL { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public PhaseType TYPE { get; set; }

        [Required]
        [StringLength(50)]
        public string P6_ACTIVITYID { get; set; }

        public decimal LOW_VALUE { get; set; }

        public decimal HIGH_VALUE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
