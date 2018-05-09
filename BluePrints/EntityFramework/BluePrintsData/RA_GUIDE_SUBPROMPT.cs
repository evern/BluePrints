namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RA_GUIDE_SUBPROMPT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_GUIDE_PROMPT { get; set; }

        [Required]
        [StringLength(500)]
        public string GUIDE_SUBPROMPT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual RA_GUIDE_PROMPT RA_GUIDE_PROMPT { get; set; }
    }
}
