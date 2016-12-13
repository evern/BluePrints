namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATION_ITEM
    {
        public decimal TOTAL_QUANTITY
        {
            get
            {
                return ESTIMATED_QUANTITY + VAR_QUANTITY;
            }
        }

        public decimal SUPPLY_RATE
        {
            get
            {
                return 0;
            }
        }

        public decimal SUPPLY_COST
        {
            get
            {
                return SUPPLY_RATE * TOTAL_QUANTITY;
            }
        }

        public decimal FREIGHT_RATE
        {
            get
            {
                return 0;
            }
        }

        public decimal FREIGHT_COST
        {
            get
            {
                return FREIGHT_RATE * TOTAL_QUANTITY;
            }
        }

        public decimal INSTALL_HOURS
        {
            get
            {
                return 0;
            }
        }
    }
}
