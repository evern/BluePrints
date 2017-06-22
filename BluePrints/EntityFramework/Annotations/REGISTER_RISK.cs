namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_RISK : IGuidEntityKey, IEntityNumber, IHaveCreatedDate
    {
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
        public string EntityNumber
        {
            get { return NUMBER; }
            set { NUMBER = value; }
        }

        [NotMapped]
        public int RiskRankingCategory
        {
            get
            {
                if (RISK_RANKING == null)
                    return 0;

                if (((int)RISK_RANKING) <= 6)
                    return 1;

                if (((int)RISK_RANKING) <= 11)
                    return 2;

                if (((int)RISK_RANKING) <= 19)
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
    }
}