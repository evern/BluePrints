namespace BluePrints.Data
{
    using BaseModel.Misc;
    using Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROGRESS : IGuidEntityKey, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROGRESS()
        {
            PROGRESS_ITEM = new HashSet<PROGRESS_ITEM>();
            PROGRESS_START = DateTime.Now;
            DATA_DATE = DateTime.Now;
            INTERVAL_COUNT = 1;
            INTERVAL_TYPE = ProgressIntervalType.Weekly;
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