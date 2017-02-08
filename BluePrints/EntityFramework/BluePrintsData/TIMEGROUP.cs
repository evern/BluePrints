namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TIMEGROUP")]
    public partial class TIMEGROUP
    {
        public TIMEGROUP()
        {
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
            ESTIMATION_INDIRECT_ITEM = new HashSet<ESTIMATION_INDIRECT_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(50)]
        public string NAME { get; set; }

        public DateTime START { get; set; }

        public DateTime FINISH { get; set; }

        public int TYPE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ICollection<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }

        public virtual ICollection<ESTIMATION_INDIRECT_ITEM> ESTIMATION_INDIRECT_ITEM { get; set; }
    }
}