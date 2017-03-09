namespace BluePrints.Data
{
    using Common.ViewModel;
    using Helpers;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class COMMODITY_CODE : IHaveGUID
    {
        public override string ToString()
        {
            return FULLCODE;
        }
    }
}