namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class HSE_INCIDENT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_HSE { get; set; }

        public DateTime DATE { get; set; }

        [Required]
        [StringLength(50)]
        public string NUMBER { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public IncidentClassification? CLASSIFICATION { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual HSE HSE { get; set; }
    }
}
