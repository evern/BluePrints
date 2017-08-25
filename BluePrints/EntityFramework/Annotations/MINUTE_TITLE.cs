namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class MINUTE_TITLE : BindableBase, IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate, ICanUpdate, IHaveSortOrder, IHaveExpandState
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
        public Guid? ParentEntityKey
        {
            get
            {
                return GUID_PARENT;
            }
            set
            {
                GUID_PARENT = value;
            }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public int SortOrder { get => SORTORDER; set => SORTORDER = value; }

        [NotMapped]
        public int? OldSortOrder { get; set; }

        [NotMapped]
        public bool IsExpanded { get => ISEXPANDED; set => ISEXPANDED = value; }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}