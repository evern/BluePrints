namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("NAME")]
    public partial class ROLE : BindableBase, IGuidEntityKey, IHaveCreatedDate, ICanUpdate
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