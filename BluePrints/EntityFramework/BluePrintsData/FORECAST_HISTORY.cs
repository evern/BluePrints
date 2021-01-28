using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FORECAST_HISTORY
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public decimal? ORIGINAL_REVENUE { get; set; }

        public decimal? ORIGINAL_COSTS { get; set; }

        public decimal? APPROVED_VARIATION { get; set; }

        public decimal? UNAPPROVED_VARIATION { get; set; }

        public decimal? TOTAL_UNAPPROVED_VARIATION { get; set; }

        public decimal? TOTAL_EAC { get; set; }

        public decimal? CONTINGENCY { get; set; }

        public decimal? CASHFLOW { get; set; }

        public DateTime EAC_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
