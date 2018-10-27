namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.ViewModel.Reporting;
    using Common;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROGRESS : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
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

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}