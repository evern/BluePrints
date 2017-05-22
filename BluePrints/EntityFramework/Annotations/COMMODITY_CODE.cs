namespace BluePrints.Data
{
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class COMMODITY_CODE : IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate, IHaveSortOrder, IHaveExpandState
    {
        [NotMapped]
        public string Temp_Id { get; set; }

        [NotMapped]
        public string Temp_Parent_Id { get; set; }

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
                if (UseGroupParent)
                    return GUID_GROUP_PARENT;

                return GUID_PARENT;
            }
            set
            {
                if(UseGroupParent)
                {
                    GUID_GROUP_PARENT = value;
                }
                else
                {
                    if (value != null)
                        GUID_PARENT = (Guid)value;
                    else
                        GUID_PARENT = Guid.Empty;
                }
            }
        }

        [NotMapped]
        public bool UseGroupParent { get; set; }

        public void SetUseGroupParent()
        {
            UseGroupParent = true;
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

        [NotMapped]
        public int SortOrder
        {
            get
            {
                return SORTORDER;
            }
            set
            {
                SORTORDER = value;
            }
        }

        [NotMapped]
        public int? OldSortOrder { get; set; }

        [NotMapped]
        public bool IsExpanded
        {
            get
            {
                return ISEXPANDED;
            }
            set
            {
                ISEXPANDED = value;
            }
        }

        public override string ToString()
        {
            return FULLCODE;
        }
    }
}