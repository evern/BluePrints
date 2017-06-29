namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class COMMODITY_CODE : IGuidEntityKey, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public COMMODITY_CODE()
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
    }
}