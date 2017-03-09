namespace BluePrints.Data
{
    using Common;
    using Common.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROGRESS : IHaveGUID
    {
        public PROGRESS()
        {
            PROGRESS_ITEM = new HashSet<PROGRESS_ITEM>();
            PROGRESS_START = DateTime.Now;
            DATA_DATE = DateTime.Now;
            INTERVAL_COUNT = 1;
            INTERVAL_TYPE = ProgressIntervalType.Weekly;
        }
    }
}