namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.ViewModel.Reporting;
    using Common;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NAME")]
    public partial class PROGRESS : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IHaveP6Baselines
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROGRESS()
        {
            PROGRESS_ITEM = new HashSet<PROGRESS_ITEM>();
            PROGRESS_ETC = new HashSet<PROGRESS_ETC>();
            PROGRESS_START = DateTime.Now;
            DATA_DATE = DateTime.Now;
            INTERVAL_COUNT = 1;
            INTERVAL_TYPE = ProgressIntervalType.Weekly;
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;

        Guid IHaveP6Baselines.project_guid => GUID_PROJECT;

        string IHaveP6Baselines.P6_Baseline_Name => P6PROGRESS_NAME;

        string IHaveP6Baselines.P6_Mod_Baseline_Name => P6PROGRESS_NAME;

        Guid IGuidEntityKey.GUID { get => GUID; set => GUID = value; }
    }
}