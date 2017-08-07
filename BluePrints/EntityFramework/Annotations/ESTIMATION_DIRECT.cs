using BaseModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BluePrints.Common;
using DevExpress.Mvvm;

namespace BluePrints.Data
{
    public partial class ESTIMATION_DIRECT : BindableBase, IGuidEntityKey, IHaveCreatedDate, IHaveP6Baselines, IAmBaseline, ICanUpdate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ESTIMATION_DIRECT()
        {
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
            MARGIN = 0;
            CONTINGENCY = 0;
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

        public string P6_Baseline_Name => P6BASELINE_NAME;

        public string P6_Mod_Baseline_Name => P6MODBASELINE_NAME;

        public Guid project_guid => GUID_PROJECT;

        [NotMapped]
        public BaselineStatus Baseline_Status { get => STATUS; set => STATUS = value; }

        [NotMapped]
        public string Revision { get => REVISION; set => REVISION = value; }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}
