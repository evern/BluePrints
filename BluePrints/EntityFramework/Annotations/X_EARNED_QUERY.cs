namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    
    public partial class X_EARNED_QUERY : EntityBase
    {
        public decimal ReportingEarnedUnits => EARNED_UNITS <= BluePrintsConstants.DurationBasedTotalUnits ? 0 : EARNED_UNITS;

        public decimal EarnedPrice => RATE == null ? 0 : (decimal)RATE * ReportingEarnedUnits;

        public string Office => BluePrintsResources.GlobalOffice;
    }
}