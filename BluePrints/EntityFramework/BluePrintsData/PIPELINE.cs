using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;

    [Table("PIPELINE")]
    public partial class PIPELINE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PIPELINE()
        {
            PIPELINE_PROFILE_ITEM = new HashSet<PIPELINE_PROFILE_ITEM>();
        }

        [Key]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(100)]
        public string NUMBER { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        [Required]
        [StringLength(100)]
        public string CLIENT { get; set; }

        public int TYPE { get; set; }

        public int DIVISION { get; set; }

        public int COMMODITY { get; set; }

        public int CONTRACT { get; set; }

        public int STATUS { get; set; }

        public DateTime? START_DATE { get; set; }

        public int DURATION { get; set; }

        public decimal GROSS_PROFIT { get; set; }

        public decimal TOTAL_VALUE { get; set; }

        public decimal SCOPE_PCT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PIPELINE_PROFILE_ITEM> PIPELINE_PROFILE_ITEM { get; set; }
    }
}
