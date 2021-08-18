namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Helpers;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_RISK : EntityBase, IGuidEntityKey, ICanSync, IEntityNumber, IHaveCreatedDate
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
        public int RiskRankingCategory
        {
            get
            {
                if (RISK_RANKING == null)
                    return 0;

                if (((int)RISK_RANKING) <= 4)
                    return 1;

                if (((int)RISK_RANKING) <= 10)
                    return 2;

                if (((int)RISK_RANKING) <= 16)
                    return 3;

                return 4;
            }
        }

        [NotMapped]
        public int ResidueRankingCategory
        {
            get
            {
                if (RESIDUE_RISK_RANKING == null)
                    return 0;

                if (((int)RESIDUE_RISK_RANKING) <= 6)
                    return 1;

                if (((int)RESIDUE_RISK_RANKING) <= 11)
                    return 2;

                if (((int)RESIDUE_RISK_RANKING) <= 19)
                    return 3;

                return 4;
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
    }
}