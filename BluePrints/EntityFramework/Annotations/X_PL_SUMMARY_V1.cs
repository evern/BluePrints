namespace BluePrints.PrimeroData
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    
    public partial class X_PL_SUMMARY_V1 : EntityBase
    {
        public string Office => BluePrintsResources.GlobalOffice;

        public decimal Profit
        {
            get
            {
                if (ApprovedRevenue == null)
                    return 0;

                decimal approvedRevenueDecimal = (decimal)ApprovedRevenue;

                if (TotalCosts == null)
                    return approvedRevenueDecimal;

                decimal totalCostsDecimal = Convert.ToDecimal((double)TotalCosts);

                return approvedRevenueDecimal - totalCostsDecimal;
            }
        }

        public decimal MarginPercentage
        {
            get
            {
                if (ApprovedRevenue == null || ApprovedRevenue == 0)
                    return 0;

                decimal approvedRevenueDecimal = (decimal)ApprovedRevenue;

                return Profit / approvedRevenueDecimal;
            }
        }
    }
}