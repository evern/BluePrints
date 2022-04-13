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

    [Table("FORECAST_JOB_HOUR_AUDIT")]
    public partial class FORECAST_JOB_HOUR_AUDIT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_FORECAST_JOB_AUDIT { get; set; }

        public DateTime FORECAST_DATE { get; set; }

        public decimal? FORECAST_HOUR { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual FORECAST_JOB_AUDIT FORECAST_JOB_AUDIT { get; set; }
    }
}
