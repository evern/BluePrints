namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class MINUTE_COMMENT : BindableBase, IGuidEntityKey, IGuidParentEntityKey, IHaveCreatedDate, ICanUpdate
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

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}