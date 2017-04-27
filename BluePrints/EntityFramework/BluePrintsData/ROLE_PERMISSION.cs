namespace BluePrints.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class ROLE_PERMISSION
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ROLE { get; set; }

        [Required]
        [StringLength(50)]
        public string PERMISSION { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ROLE ROLE { get; set; }
    }
}
