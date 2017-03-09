namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using Attributes;
    using Common.ViewModel;

    [ConstraintAttributes("INTERNAL_NUM")]
    public partial class PHASE : IHaveGUID
    {
    }
}