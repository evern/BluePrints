namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class TRANSACTION_APPROVAL : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public bool IsNotApproved => APPROVEDON == null;

        public string Office => BluePrintsResources.GlobalOffice;

        public int? ViewJOBNO => NEW_JOBNO == null ? OLD_JOBNO : NEW_JOBNO;
        public int? ViewCOST_GROUP_NO => NEW_COST_GROUP_NO == null ? OLD_COST_GROUP_NO : NEW_COST_GROUP_NO;
        public int? ViewCOST_TYPE_NO => NEW_COST_TYPE_NO == null ? OLD_COST_TYPE_NO : NEW_COST_TYPE_NO;
        public string ViewSTOCK_CODE => NEW_STOCK_CODE == null ? OLD_STOCK_CODE : NEW_STOCK_CODE;
        public string ViewVARIATION_CODE => NEW_VARIATION_CODE == null ? OLD_VARIATION_CODE : NEW_VARIATION_CODE;
    }
}