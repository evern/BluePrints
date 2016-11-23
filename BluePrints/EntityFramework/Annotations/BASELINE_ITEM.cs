namespace BluePrints.Data
{
    using BluePrints.Common;
    using BluePrints.Data.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM
    {
        public decimal TOTAL_HOURS
        {
            get
            {
                return ESTIMATED_HOURS + DC_HOURS;
            }
        }
    }
}
