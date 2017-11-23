namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WORKPACK")]
    public partial class WORKPACK
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_SUBJOB { get; set; }

        public Guid GUID_DISCIPLINE { get; set; }

        public decimal DISCIPLINE_NUM { get; set; }

        public string NAME { get; set; }

        public string TITLE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEM { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual SUBJOB SUBJOB { get; set; }
    }
}
