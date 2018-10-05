namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_CODE : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IOriginalGuidEntityKey
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public STOCK_CODE()
        {
            ESTIMATE_ITEM = new HashSet<ESTIMATE_ITEM>();
            ESTIMATE_ITEM1 = new HashSet<ESTIMATE_ITEM>();
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

        public Guid OriginalEntityKey => GUID_ORIGINAL;

        public void SetOriginalEntityKey(Guid newGuid) { GUID_ORIGINAL = newGuid; }

        public string Office
        {
            get
            {
                if (this.PROJECT != null)
                    return this.PROJECT.NUMBER + " " + this.PROJECT.OFFICE.NAME;

                return BluePrintsResources.GlobalOffice;
            }
        }
    }
}