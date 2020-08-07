using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VARIATION_CONSTRUCTION_IMPACT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_CONSTRUCTION_VARIATION { get; set; }

        public VariationConstructionImpact IMPACT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual VARIATION_CONSTRUCTION VARIATION_CONSTRUCTION { get; set; }
    }
}
