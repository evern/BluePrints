namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Helpers;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_LL : EntityBase, IGuidEntityKey, ICanSync, IEntityNumber, IHaveCreatedDate
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
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string EntityGroup => string.Empty;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;

        public int CompareTo(REGISTER_LL other)
        {
            int dummyFieldLength = 0;
            long thisNumber = 0;
            long otherNumber = 0;
            string dummyString;
            dummyString = StringFormatUtils.ParseStringIntoComponents(this.EntityNumber, out dummyFieldLength, out thisNumber);
            dummyString = StringFormatUtils.ParseStringIntoComponents(other.EntityNumber, out dummyFieldLength, out otherNumber);

            if (thisNumber < otherNumber)
            {
                return 1;
            }
            else if (thisNumber > otherNumber)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}