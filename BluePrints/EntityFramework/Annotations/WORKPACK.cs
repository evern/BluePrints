namespace BluePrints.Data
{
    using BluePrints.Data.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [ConstraintAttributes("GUID_PROJECT, INTERNAL_NAME1, INTERNAL_NAME2")]
    public partial class WORKPACK
    {
        public WORKPACK()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            ESTIMATION_ITEM = new HashSet<ESTIMATION_ITEM>();
            ESTIMATION_ITEM1 = new HashSet<ESTIMATION_ITEM>();
            WORKPACK_ASSIGNMENT = new HashSet<WORKPACK_ASSIGNMENT>();
            STARTDATE = DateTime.Now;
            ENDDATE = DateTime.Now;
            REVIEWSTARTDATE = DateTime.Now;
            REVIEWENDDATE = DateTime.Now;
        }
    }
}
