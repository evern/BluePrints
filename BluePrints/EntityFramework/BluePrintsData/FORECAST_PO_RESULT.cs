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

    public partial class FORECAST_PO_RESULT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_FORECAST_PO { get; set; }

        [Required]
        [StringLength(50)]
        public string PONO { get; set; }

        public DateTime FORECAST_DATE { get; set; }

        public decimal FORECAST_AMOUNT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual FORECAST_PO FORECAST_PO { get; set; }
    }
}
