namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("UOM1")]
    public partial class UOM : IHaveGUID
    {
    }
}