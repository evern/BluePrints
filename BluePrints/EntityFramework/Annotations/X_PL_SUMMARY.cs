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
    
    public partial class X_PL_SUMMARY : EntityBase
    {
        public string Office => BluePrintsResources.GlobalOffice;
    }
}