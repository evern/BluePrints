namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("USER")]
    public partial class USER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public USER()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            BASELINE_ITEM_WORK = new HashSet<BASELINE_ITEM_WORK>();
            PROJECT = new HashSet<PROJECT>();
            REGISTER_ISSUE = new HashSet<REGISTER_ISSUE>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_ROLE { get; set; }

        public Guid? GUID_DEPARTMENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }
        
        public string FIRST_NAME { get; set; }

        public string LAST_NAME { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(100)]
        public string DEPARTMENT { get; set; }

        [StringLength(100)]
        public string TITLE { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public decimal? UTILIZATION { get; set; }

        public int? EXO_STAFF_ID { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM_WORK> BASELINE_ITEM_WORK { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PROJECT> PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REGISTER_ISSUE> REGISTER_ISSUE { get; set; }

        public virtual DEPARTMENT DEPARTMENT1 { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual ROLE ROLE { get; set; }
    }
}
