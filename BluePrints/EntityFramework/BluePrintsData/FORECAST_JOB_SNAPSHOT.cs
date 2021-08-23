using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    using BluePrints.Common;
    using BluePrints.Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FORECAST_JOB_SNAPSHOT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FORECAST_JOB_SNAPSHOT()
        {
            FORECAST_JOB_HOUR_SNAPSHOT = new HashSet<FORECAST_JOB_HOUR_SNAPSHOT>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [Required]
        [StringLength(50)]
        public string SUBJOB_CODE { get; set; }

        [Required]
        [StringLength(50)]
        public string DISCIPLINE_CODE { get; set; }

        [Required]
        [StringLength(50)]
        public string COMMODITY_CODE { get; set; }

        [Required]
        [StringLength(250)]
        public string VARIATION_CODE { get; set; }

        public decimal TENDER_BUDGET { get; set; }

        public decimal PROJECT_BUDGET { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FORECAST_JOB_HOUR_SNAPSHOT> FORECAST_JOB_HOUR_SNAPSHOT { get; set; }
    }
}
