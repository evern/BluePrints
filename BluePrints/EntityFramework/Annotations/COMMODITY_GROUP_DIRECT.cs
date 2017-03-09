namespace BluePrints.Data
{
    using Attributes;
    using Common.ViewModel;
    using Helpers;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("GUID_PARENT, GUID_COMMODITYCODE")]
    public partial class COMMODITY_GROUP_DIRECT : IHaveGUID
    {
    }
}