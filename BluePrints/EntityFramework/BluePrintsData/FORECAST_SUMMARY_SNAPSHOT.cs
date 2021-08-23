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

    public partial class FORECAST_SUMMARY_SNAPSHOT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public decimal? ORIGINAL_REVENUE { get; set; }

        public decimal? APPROVED_VARIATION { get; set; }

        public decimal? UNAPPROVED_VARIATION { get; set; }

        public decimal? TOTAL_UNAPPROVED_VARIATION { get; set; }

        public DateTime DATA_DATE { get; set; }

        public DateTime END_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }
    }
}
