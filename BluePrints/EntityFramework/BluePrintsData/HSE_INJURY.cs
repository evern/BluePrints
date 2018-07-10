namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class HSE_INJURY
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_HSE { get; set; }

        [Required]
        public int STAFFNO { get; set; }

        public DateTime DOI { get; set; }

        [StringLength(1000)]
        public string DETAILS { get; set; }

        public DateTime? NEXT_REVIEW { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual HSE HSE { get; set; }
    }
}
