namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("GUID_PROJECT, NAME")]
    public partial class VARIATION : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VARIATION()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            ESTIMATE_ITEM = new HashSet<ESTIMATE_ITEM>();
            VARIATION_ITEM = new HashSet<VARIATION_ITEM>();
            TYPE = Common.VariationType.External;
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