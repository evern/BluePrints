namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("CODE")]
    public partial class DISCIPLINE : IHaveGUID
    {
    }
}