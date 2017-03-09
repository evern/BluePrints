namespace BluePrints.Data
{
    using Common;
    using Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using Common.ViewModel;

    [ConstraintAttributes("GUID_BASELINE, INTERNAL_NUM")]
    public partial class BASELINE_ITEM : IHaveGUID
    {
        public decimal TOTAL_HOURS
        {
            get { return ESTIMATED_HOURS + DC_HOURS; }
        }
    }
}