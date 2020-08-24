namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Helpers;
    using BluePrints.PrimeroData;
    using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class VARIATION_CONSTRUCTION_IMPACT : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public string Office => this.VARIATION_CONSTRUCTION.PROJECT.NUMBER + " " + this.VARIATION_CONSTRUCTION.PROJECT.OfficeName;

        [NotMapped]
        public string ImpactDescription => EnumHelper< VariationConstructionImpact>.GetDisplayValue(IMPACT);
    }
}