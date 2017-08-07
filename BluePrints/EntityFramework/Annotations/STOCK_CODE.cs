namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_CODE : BindableBase, IGuidEntityKey, IHaveCreatedDate, IOriginalGuidEntityKey, ICanUpdate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public STOCK_CODE()
        {
            ESTIMATION_DIRECT_ITEM = new HashSet<ESTIMATION_DIRECT_ITEM>();
            RATE_SUPPLY = 0;
            HOURS_INSTALL = 0;
        }

        [NotMapped]
        public Guid EntityKey
        {
            get { return GUID; }
            set { GUID = value; }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string ProjectNumber
        {
            get
            {
                if (PROJECT == null)
                    return string.Empty;

                return PROJECT.NUMBER;
            }
        }

        public virtual void Update()
        {
            RaisePropertiesChanged();
        }

        public Guid OriginalEntityKey => GUID_ORIGINAL;

        public void SetOriginalEntityKey(Guid newGuid) { GUID_ORIGINAL = newGuid; }
    }
}