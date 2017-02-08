namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class WORKPACK_ASSIGNMENT
    {
        [Key]
        public Guid GUID { get; set; }

        public bool ISMODIFIEDBASELINE { get; set; }

        public Guid GUID_WORKPACK { get; set; }

        [Required]
        [StringLength(50)]
        public string P6_ACTIVITYID { get; set; }

        public decimal LOW_VALUE { get; set; }

        public decimal HIGH_VALUE { get; set; }

        public int PRIORITY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual WORKPACK WORKPACK { get; set; }
    }
}