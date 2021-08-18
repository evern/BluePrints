namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Helpers;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_ISSUE : EntityBase, IGuidEntityKey, ICanSync, IEntityNumber, IHaveCreatedDate
    {
        [NotMapped]
        public string EntityNumber
        {
            get { return NUMBER; }
            //set sort number to null so it refreshes the next time get is called
            set { NUMBER = value; entitySortNumber = null; }
        }

        long? entitySortNumber;
        public long EntitySortNumber
        {
            get
            {
                if (entitySortNumber == null)
                {
                    long sortNumber = 0;
                    int dummyFieldLength = 0;
                    string dummyString;
                    dummyString = StringFormatUtils.ParseStringIntoComponents(this.EntityNumber, out dummyFieldLength, out sortNumber);
                    entitySortNumber = sortNumber;
                }

                return (long)entitySortNumber;
            }
        }

        [NotMapped]
        public bool IsActionedOnDrawing
        {
            get
            {
                return ACTIONED_ON_DWG != null && ACTIONED_ON_DWG == Common.ActionedOnDrawing.Yes;
            }
        }

        [NotMapped]
        public REGISTER_CHANGE RegisterChange { get; set; }

        public void SetRegisterChange(IEnumerable<REGISTER_CHANGE> REGISTER_CHANGECollection)
        {
            RegisterChange = REGISTER_CHANGECollection.FirstOrDefault(x => x.GUID == GUID_CHANGE);
        }

        [NotMapped]
        public REGISTER_HOLD RegisterHold { get; set; }

        public void SetRegisterHold(IEnumerable<REGISTER_HOLD> REGISTER_HoldCollection)
        {
            RegisterHold = REGISTER_HoldCollection.FirstOrDefault(x => x.GUID == GUID_HOLD);
        }
        
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string EntityGroup => string.Empty;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}