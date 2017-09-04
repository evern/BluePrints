namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common.ViewModel.Reporting;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using BluePrints.Common;
    using DevExpress.Mvvm;
    using BluePrints.Common.Base;

    [ConstraintAttributes("REVISION")]
    [BulkEditDisabledAttributes("P6BASELINE_NAME, P6MODBASELINE_NAME")]
    public partial class BASELINE : BluePrintsEntityBase, IGuidEntityKey, IHaveCreatedDate, IHaveP6Baselines, IAmBaseline
    {
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

        public string P6_Baseline_Name => P6BASELINE_NAME;

        public string P6_Mod_Baseline_Name => P6MODBASELINE_NAME;

        public Guid project_guid => GUID_PROJECT;

        [NotMapped]
        public BaselineStatus Baseline_Status { get => STATUS; set => STATUS = value; }

        [NotMapped]
        public string Revision { get => REVISION; set => REVISION = value; }
    }
}