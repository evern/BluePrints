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
    using BluePrints.Common.Projections;
    using System.Linq;
    using BaseModel.DataModel;

    public partial class MINUTE_TITLE : EntityBase, IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate, IHaveSortOrder, IHaveExpandState
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MINUTE_TITLE()
        {
            MINUTE_AGENDA = new HashSet<MINUTE_AGENDA>();
            MINUTE_TITLE1 = new HashSet<MINUTE_TITLE>();
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

        [NotMapped]
        public string Full_Name
        {
            get
            {
                return DisplayNumber + " - " + NAME;
            }
        }

        [NotMapped]
        public string DisplayNumber
        {
            get
            {
                List<string> minute_number_collector = new List<string>();
                minute_number_constructor(MINUTE_TITLE2, minute_number_collector);

                string full_minute_number = string.Empty;
                for(int i=minute_number_collector.Count - 1; i >= 0; i--)
                {
                    full_minute_number += minute_number_collector[i] + ".";
                }

                full_minute_number += NUMBER;
                return full_minute_number;
            }
        }

        [NotMapped]
        public bool IsLast
        {
            get
            {
                return MINUTE_TITLE1 == null || MINUTE_TITLE1.Count == 0;
            }
        }

        [NotMapped]
        public Guid EntityKey { get => GUID; set => GUID = value; }

        private void minute_number_constructor(MINUTE_TITLE parent_minute_title, List<string> minute_title_collector)
        {
            if(parent_minute_title != null)
            {
                minute_title_collector.Add(parent_minute_title.NUMBER);
                minute_number_constructor(parent_minute_title.MINUTE_TITLE2, minute_title_collector);
            }
        }

        [NotMapped]
        public int Summary_Total_Agendas { get; set; }
        
        [NotMapped]
        public int Summary_Due_Agendas { get; set; }

        [NotMapped]
        public int Summary_Closed_Agendas { get; set; }
    }
}