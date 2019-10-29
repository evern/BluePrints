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

    [Table("FORECAST_JOB")]
    public partial class FORECAST_JOB
    {
        public FORECAST_JOB()
        {
            FORECAST_JOB_HOUR = new HashSet<FORECAST_JOB_HOUR>();
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

        [StringLength(50)]
        public string VARIATION_CODE { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [StringLength(50)]
        public string STOCK_ITEM { get; set; }

        [StringLength(500)]
        public string REFERENCE { get; set; }

        [StringLength(500)]
        public string NOTE { get; set; }

        [StringLength(500)]
        public string UOM { get; set; }

        public bool IS_FLOATING_RATE { get; set; }

        public decimal? FORECAST_RATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FORECAST_JOB_HOUR> FORECAST_JOB_HOUR { get; set; }
    }
}
