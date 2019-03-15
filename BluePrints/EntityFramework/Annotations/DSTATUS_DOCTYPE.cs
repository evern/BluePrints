namespace BluePrints.Data
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

    public partial class DSTATUS_DOCTYPE : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
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
                if (this.DELIVERABLES_STATUS == null && this.DELIVERABLES_STATUS.PROJECT == null)
                    return BluePrintsResources.GlobalOffice;

                return this.DELIVERABLES_STATUS.PROJECT.NUMBER + " " + this.DELIVERABLES_STATUS.PROJECT.OfficeName;
            }
        }
    }
}