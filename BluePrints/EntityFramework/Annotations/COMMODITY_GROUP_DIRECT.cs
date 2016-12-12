namespace BluePrints.Data
{
    using BluePrints.Data.Attributes;
    using BluePrints.Data.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("GUID_PROJECT, GUID_COMMODITYCODE")]
    public partial class COMMODITY_GROUP_DIRECT
    {
        public bool ISQUANTIFIABLE
        {
            get { return this.GUID_COMMODITYCODE != null; }
        }
    }
}
