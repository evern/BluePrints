namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using BluePrints.Common.Base;
    using BaseModel.DataModel;
    using BluePrints.Common.Resources;
    using BluePrints.Common.Projections;

    public partial class TENDER_PROFILE : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office
        {
            get
            {
                if(this.PROJECT != null)
                    return  this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;

                return BluePrintsResources.GlobalOffice;
            }
        }
    }
}