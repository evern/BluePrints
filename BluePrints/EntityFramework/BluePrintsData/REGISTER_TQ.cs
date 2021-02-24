namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_TQ
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid GUID_DISCIPLINE { get; set; }

        [Required]
        [StringLength(150)]
        public string NUMBER { get; set; }

        [StringLength(300)]
        public string RAISEDBY { get; set; }

        public DateTime? DATE_RAISED { get; set; }

        [StringLength(100)]
        public string ADDRESSED_TO { get; set; }

        public DateTime? DATE_REQUESTED { get; set; }

        public DateTime? DATE_RESPONSE { get; set; }

        [StringLength(100)]
        public string SUBJECT { get; set; }

        [StringLength(4000)]
        public string COMMENTS { get; set; }

        public DateTime? DATE_RESPONDED { get; set; }

        public int? RESPONSE_DAYS { get; set; }

        [StringLength(4000)]
        public string RESPONSE { get; set; }

        [StringLength(500)]
        public string TQ_PATH { get; set; }

        public RegisterTQ_Status? STATUS { get; set; }

        public RegisterTQ_OpenClose? OPENCLOSE { get; set; }

        [StringLength(4000)]
        public string NEW_COMMENTS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
