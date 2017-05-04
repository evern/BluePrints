namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("GUID_PROJECT, INTERNAL_NAME1, INTERNAL_NAME2")]
    public partial class WORKPACK : IGuidEntityKey
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WORKPACK()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
            ESTIMATION_DIRECT_ITEM1 = new HashSet<ESTIMATION_DIRECT_ITEM>();
            ESTIMATION_INDIRECT_ITEM = new HashSet<ESTIMATION_INDIRECT_ITEM>();
            WORKPACK_ASSIGNMENT = new HashSet<WORKPACK_ASSIGNMENT>();
            STARTDATE = DateTime.Now;
            ENDDATE = DateTime.Now;
            REVIEWSTARTDATE = DateTime.Now;
            REVIEWENDDATE = DateTime.Now;
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