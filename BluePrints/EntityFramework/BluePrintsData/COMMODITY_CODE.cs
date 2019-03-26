using BluePrints.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    public partial class COMMODITY_CODE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public COMMODITY_CODE()
        {
            ESTIMATE_ITEM = new HashSet<ESTIMATE_ITEM>();
            STOCK_CODE = new HashSet<STOCK_CODE>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        [Required]
        [StringLength(50)]
        public string CODE { get; set; }

        [StringLength(23)]
        public string DEFAULT_STOCKCODE { get; set; }

        [StringLength(500)]
        public string NAME { get; set; }

        [StringLength(1000)]
        public string DESCRIPTION { get; set; }

        [StringLength(100)]
        public string UOM { get; set; }
       
        public PhaseType PHASE_TYPE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTIMATE_ITEM> ESTIMATE_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STOCK_CODE> STOCK_CODE { get; set; }
    }
}
