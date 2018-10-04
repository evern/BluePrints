namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using DevExpress.Mvvm;
    using BluePrints.Common.Base;
    using BaseModel.DataModel;

    [ConstraintAttributes("NAME")]
    public partial class WORKPACK : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WORKPACK()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            DISCIPLINE_NUM = 1;
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
    }
}