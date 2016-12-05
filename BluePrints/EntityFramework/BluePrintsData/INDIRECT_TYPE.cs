namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("INDIRECT_TYPE")]
    public partial class INDIRECT_TYPE
    {
        public INDIRECT_TYPE()
        {
            COMMODITY_CODE = new HashSet<COMMODITY_CODE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(50)]
        public string CODE { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }


        public virtual ICollection<COMMODITY_CODE> COMMODITY_CODE { get; set; }
    }
}
