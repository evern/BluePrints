namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RA_STUDY_DATA
    {
        [Key]
        public Guid GUID { get; set; }

        public int NUMBER { get; set; }

        public Guid GUID_NODE { get; set; }

        public Guid? GUID_GUIDE_PROMPT { get; set; }

        public Guid? GUID_GUIDE_SUBPROMPT { get; set; }

        public Guid? GUID_ACTION_BY { get; set; }

        [Required]
        [StringLength(500)]
        public string TITLE { get; set; }

        [StringLength(2000)]
        public string POSSIBLE_CAUSES { get; set; }

        [StringLength(2000)]
        public string CONSEQUENCES { get; set; }

        [StringLength(2000)]
        public string SAFEGUARDS { get; set; }

        [StringLength(2000)]
        public string RECOMMENDATIONS { get; set; }

        public bool DONE { get; set; }

        [StringLength(2000)]
        public string COMMENTS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual RA_GUIDE_PROMPT RA_GUIDE_PROMPT { get; set; }

        public virtual RA_GUIDE_SUBPROMPT RA_GUIDE_SUBPROMPT { get; set; }

        public virtual RA_STUDY_NODE RA_STUDY_NODE { get; set; }

        public virtual USER USER { get; set; }
    }
}
