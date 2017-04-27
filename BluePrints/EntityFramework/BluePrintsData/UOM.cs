namespace BluePrints.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UOM")]
    public partial class UOM
    {
        [Key]
        public Guid GUID { get; set; }

        [Column("UOM")]
        [Required]
        [StringLength(50)]
        public string UOM1 { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }
    }
}
