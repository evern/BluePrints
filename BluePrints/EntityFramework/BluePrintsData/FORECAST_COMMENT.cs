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

    [Table("FORECAST_COMMENT")]
    public partial class FORECAST_COMMENT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [StringLength(50)]
        public string SUBJOB_CODE { get; set; }

        [StringLength(50)]
        public string DISCIPLINE_CODE { get; set; }

        [StringLength(50)]
        public string COMMODITY_CODE { get; set; }

        [StringLength(50)]
        public string VARIATION_CODE { get; set; }

        public DateTime FORECAST_DATE { get; set; }

        [StringLength(500)]
        public string COMMENT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
