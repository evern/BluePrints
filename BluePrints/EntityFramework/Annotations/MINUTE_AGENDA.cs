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

    public partial class MINUTE_AGENDA : BindableBase, IHaveDetail<MINUTE_COMMENT>, IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate, ICanUpdate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MINUTE_AGENDA()
        {
            RAISE_DATE = DateTime.Now;
            DUE_DATE = DateTime.Now;
            MINUTE_COMMENT = new HashSet<MINUTE_COMMENT>();
            MINUTE_AGENDA1 = new HashSet<MINUTE_AGENDA>();
            DetailEntities = new ObservableCollection<MINUTE_COMMENT>();
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public Guid? ParentEntityKey { get => GUID_MINUTE_TITLE; set => GUID_MINUTE_TITLE = value; }

        [NotMapped]
        public ObservableCollection<MINUTE_COMMENT> DetailEntities { get; set; }

        [NotMapped]
        public Guid EntityKey { get => GUID; set => GUID = value; }

        [NotMapped]
        public bool IsExpanded { get; set; }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}