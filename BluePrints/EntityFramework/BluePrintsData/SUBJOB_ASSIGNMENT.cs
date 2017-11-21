namespace BluePrints.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class SUBJOB_ASSIGNMENT
    {
        [Key]
        public Guid GUID { get; set; }

        public bool ISMODIFIEDBASELINE { get; set; }

        public Guid GUID_SUBJOB { get; set; }

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

        public virtual SUBJOB SUBJOB { get; set; }
    }
}
