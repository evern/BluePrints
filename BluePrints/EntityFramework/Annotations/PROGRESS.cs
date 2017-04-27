namespace BluePrints.Data
{
    using Common;
    using Common.ViewModel;
    using System;
    using System.Collections.Generic;

    public partial class PROGRESS : IHaveGUID
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
    }
}