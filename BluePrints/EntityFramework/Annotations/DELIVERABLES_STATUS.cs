namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DELIVERABLES_STATUS : BindableBase, IGuidEntityKey, IHaveCreatedDate, ICanUpdate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DELIVERABLES_STATUS()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            FOR_DELIVERABLE = true;
            FOR_NCR = true;
            FOR_TASK = true;
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

        public virtual void Update()
        {
            RaisePropertiesChanged();
        }
    }
}