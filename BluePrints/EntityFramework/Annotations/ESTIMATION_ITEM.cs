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
                if (COMMODITY == null)
                    return 0;
                else if (COMMODITY.RATE_SUPPLY == null)
                    return 0;
                else
                    return (decimal)COMMODITY.RATE_SUPPLY;
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
                if (COMMODITY == null)
                    return 0;
                else if (COMMODITY.RATE_FREIGHT == null)
                    return 0;
                else
                    return (decimal)COMMODITY.RATE_FREIGHT;
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
                if (COMMODITY == null)
                    return 0;
                else if (COMMODITY.HOURS_INSTALL == null)
                    return 0;
                else
                    return (decimal)COMMODITY.HOURS_INSTALL;
            }
        }
    }
}
