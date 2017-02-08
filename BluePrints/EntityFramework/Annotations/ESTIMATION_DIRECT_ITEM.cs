namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATION_DIRECT_ITEM
    {
        public decimal TOTAL_QUANTITY
        {
            get { return ESTIMATED_QUANTITY + VAR_QUANTITY; }
        }
    }
}