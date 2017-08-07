namespace BluePrints.Data
{
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class REGISTER : BindableBase, IGuidEntityKey, IHaveCreatedDate, ICanUpdate
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
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}