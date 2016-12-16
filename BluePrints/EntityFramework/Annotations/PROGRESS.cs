namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROGRESS
    {
        public PROGRESS()
        {
            PROGRESS_ITEM = new HashSet<PROGRESS_ITEM>();
            PROGRESS_START = DateTime.Now;
            DATA_DATE = DateTime.Now;
        }
    }
}
