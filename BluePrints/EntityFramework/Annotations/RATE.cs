namespace BluePrints.Data
{
    using Common;
    using Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("GUID_DEPARTMENT, GUID_DISCIPLINE")]
    public partial class RATE
    {
    }
}