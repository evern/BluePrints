using BluePrints.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    public partial class VARIATION_CONSTRUCTION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VARIATION_CONSTRUCTION()
        {
            VARIATION_CONSTRUCTION_ITEM = new HashSet<VARIATION_CONSTRUCTION_ITEM>();
            VARIATION_CONSTRUCTION_IMPACT = new HashSet<VARIATION_CONSTRUCTION_IMPACT>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [StringLength(50)]
        public string CLIENT_NUMBER { get; set; }

        [Required]
        [StringLength(50)]
        public string NUMBER { get; set; }

        public VariationConstructionType TYPE { get; set; }

        public int IMPACT { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        public DateTime SUBMISSION_DATE { get; set; }

        public VariationConstructionStatus STATUS { get; set; }

        public decimal APPROVED_VALUE { get; set; }

        [StringLength(100)]
        public string REFERENCE { get; set; }

        [StringLength(100)]
        public string DOCUMENTATION { get; set; }

        [StringLength(50)]
        public string SCHEDULE_IMPACT { get; set; }

        [StringLength(1000)]
        public string NOTES { get; set; }

        public DateTime? REQUEST_DATE { get; set; }

        [StringLength(100)]
        public string REQUESTED_BY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VARIATION_CONSTRUCTION_ITEM> VARIATION_CONSTRUCTION_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VARIATION_CONSTRUCTION_IMPACT> VARIATION_CONSTRUCTION_IMPACT { get; set; }

    }
}
