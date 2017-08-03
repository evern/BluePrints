namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common.Projections;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using BluePrints.Common.ViewModel.Reporting;
    using BluePrints.Common.Resources;

    [ConstraintAttributes("CODE")]
    public partial class STOCK_GROUP : IGuidEntityKey, IHaveCreatedDate
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
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }
    }
}