namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class REGISTER_TQ_ATTACHMENT : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        public REGISTER_TQ_ATTACHMENT()
        {

        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string EntityGroup => string.Empty;

        public override string ToString()
        {
            return ATTACHMENT_NAME;
        }

        public string Office => this.REGISTER_TQ.PROJECT.NUMBER + " " + this.REGISTER_TQ.PROJECT.OfficeName;
    }
}