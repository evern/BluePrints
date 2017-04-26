namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BASELINE_ITEM_WORK
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_BASELINE_ITEM_ORIGINAL { get; set; }

        public Guid GUID_USER { get; set; }

        public decimal WEIGHTING { get; set; }

        public virtual USER USER { get; set; }
    }
}
