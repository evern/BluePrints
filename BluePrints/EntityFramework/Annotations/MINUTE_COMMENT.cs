namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class MINUTE_COMMENT : BluePrintsEntityBase, IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate
    {
        public MINUTE_COMMENT()
        {
            DATE_RAISED = DateTime.Now;
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

        [NotMapped]
        public Guid? ParentEntityKey { get => GUID_AGENDA; set => GUID_AGENDA = value; }
        
    }
}