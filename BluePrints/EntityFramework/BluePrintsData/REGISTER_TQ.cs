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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public REGISTER_TQ()
        {
            REGISTER_TQ_ATTACHMENT = new HashSet<REGISTER_TQ_ATTACHMENT>();
            Documents = new List<REGISTER_TQ_ATTACHMENT>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid GUID_DISCIPLINE { get; set; }

        [Required]
        [StringLength(150)]
        public string NUMBER { get; set; }

        [StringLength(100)]
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

        [StringLength(4000)]
        public string DESCRIPTION { get; set; }

        [StringLength(4000)]
        public string PROPOSED_SOLUTION { get; set; }

        public RegisterTQ_Impact? IMPACT { get; set; }

        [StringLength(100)]
        public string APPROVER { get; set; }

        [StringLength(300)]
        public string RELATED_TQ { get; set; }

        public DateTime? DATE_RESPONDED { get; set; }

        [StringLength(4000)]
        public string RESPONSE { get; set; }

        public RegisterTQ_ResponseStatus? RESPONSE_STATUS { get; set; }

        public RegisterTQ_OpenClose? OPENCLOSE { get; set; }

        [StringLength(4000)]
        public string NEW_COMMENTS { get; set; }

        [StringLength(500)]
        public string TQ_PATH { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_TQ_ATTACHMENT> REGISTER_TQ_ATTACHMENT { get; set; }
    }
}
