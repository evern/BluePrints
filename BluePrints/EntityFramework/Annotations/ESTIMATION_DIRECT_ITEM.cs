namespace BluePrints.Data
{
    using Common.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATION_DIRECT_ITEM : IHaveGUID
    {
        public decimal TOTAL_QUANTITY
        {
            get { return ESTIMATED_QUANTITY + VAR_QUANTITY; }
        }
    }
}