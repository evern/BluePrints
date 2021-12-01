namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using BluePrints.Common.ViewModel.Reporting;
    using Common;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("NAME")]
    public partial class PROJECT_CONTRACTOR : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROJECT_CONTRACTOR()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get
            {
                return CREATED;
            }
            set
            {
                CREATED = value;
            }
        }

        public string Office => this.PROJECT == null ? string.Empty : this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}