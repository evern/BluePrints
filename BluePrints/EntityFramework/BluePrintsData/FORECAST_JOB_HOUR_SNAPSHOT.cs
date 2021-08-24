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

    [Table("FORECAST_JOB_HOUR_SNAPSHOT")]

    public partial class FORECAST_JOB_HOUR_SNAPSHOT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_FORECAST_JOB_SNAPSHOT { get; set; }

        [Required]
        [StringLength(50)]
        public string STOCK_CODE { get; set; }

        public ForecastSnapshotValueType SNAPSHOT_TYPE { get; set; }

        public DateTime FORECAST_DATE { get; set; }

        public decimal FORECAST_QTY { get; set; }

        public decimal FORECAST_COSTS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual FORECAST_JOB_SNAPSHOT FORECAST_JOB_SNAPSHOT { get; set; }
    }
}
