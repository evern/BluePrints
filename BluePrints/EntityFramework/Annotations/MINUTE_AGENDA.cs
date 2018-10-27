namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.ObjectModel;
    using BaseModel.DataModel;

    public partial class MINUTE_AGENDA : EntityBase, IGuidEntityKey, ICanSync, IGuidParentEntityKey, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MINUTE_AGENDA()
        {
            RAISE_DATE = DateTime.Now;
            DUE_DATE = DateTime.Now;
            MINUTE_AGENDA1 = new HashSet<MINUTE_AGENDA>();
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public Guid? ParentEntityKey { get => GUID_PARENT; set => GUID_PARENT = value; }

        [NotMapped]
        public Guid EntityKey { get => GUID; set => GUID = value; }

        [NotMapped]
        public bool IsExpanded { get; set; }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}