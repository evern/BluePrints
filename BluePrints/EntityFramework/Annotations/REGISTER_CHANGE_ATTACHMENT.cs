namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using BluePrints.Common.ViewModel.Reporting;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class REGISTER_CHANGE_ATTACHMENT : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IAmAttachmentPath
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string EntityGroup => string.Empty;

        public string Office => this.REGISTER_CHANGE.PROJECT.NUMBER + " " + this.REGISTER_CHANGE.PROJECT.OfficeName;

        [NotMapped]
        public string AttachmentPath { get => ATTACHMENT_PATH; set => ATTACHMENT_PATH = value; }

        [NotMapped]
        public string AttachmentName { get => ATTACHMENT_NAME; set => ATTACHMENT_NAME = value; }

        public override string ToString()
        {
            return ATTACHMENT_NAME;
        }
    }
}