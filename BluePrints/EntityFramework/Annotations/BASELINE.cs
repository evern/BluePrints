namespace BluePrints.Data
{
    using Common;
    using Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("REVISION")]
    [BulkEditDisabledAttributes("P6BASELINE_NAME, P6MODBASELINE_NAME")]
    public partial class BASELINE
    {
    }
}