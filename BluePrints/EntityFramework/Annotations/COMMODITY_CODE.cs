namespace BluePrints.Data
{
    using Helpers;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class COMMODITY_CODE
    {
        public override string ToString()
        {
            return FULLCODE;
        }
    }
}