namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DELIVERABLES_STATUS : IGuidEntityKey
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DELIVERABLES_STATUS()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            FOR_DELIVERABLE = true;
            FOR_NCR = true;
            FOR_TASK = true;
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
    }
}