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

    public partial class TENDER_PROFILE_ITEM : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        public TENDER_PROFILE_ITEM()
        {
            BELLCURVESHAPE = Common.BellCurveShape.Balanced;
        }

        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
        }

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
                if (this.TENDER_PROFILE != null)
                    return this.TENDER_PROFILE.PROJECT.NUMBER + " " + this.TENDER_PROFILE.PROJECT.OfficeName;

                return BluePrintsResources.GlobalOffice;
            }
        }
    }
}