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

        public decimal OriginalRevenue => ORI_REVENUE == null ? 0 : (decimal)ORI_REVENUE;

        public decimal VarRevenue => VAR_REVENUE == null ? 0 : (decimal)VAR_REVENUE;

        public decimal UnapprovedVarRevenue => UNAPPROVED_VAR_REVENUE == null ? 0 : (decimal)UNAPPROVED_VAR_REVENUE;

        public decimal ApprovedRevenue
        {
            get
            {
                return OriginalRevenue + VarRevenue;
            }
        }

        public decimal ForecastRevenue
        {
            get
            {
                return OriginalRevenue + VarRevenue + UnapprovedVarRevenue;
            }
        }

        public decimal Profit
        {
            get
            {
                if (TotalCosts == null)
                    return ApprovedRevenue;

                decimal totalCostsDecimal = Convert.ToDecimal((double)TotalCosts);
                return ApprovedRevenue - totalCostsDecimal;
            }
        }

        public decimal MarginPercentage
        {
            get
            {
                if (ApprovedRevenue == 0)
                    return 0;

                return Profit / ApprovedRevenue;
            }
        }
    }
}