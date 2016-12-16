namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROGRESS_ITEM
    {
        public PROGRESS_ITEM()
        {
            COMMODITY_GROUP_DIRECT = new HashSet<COMMODITY_GROUP_DIRECT>();
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROGRESS { get; set; }

        public Guid GUID_ORIBASEITEM { get; set; }

        public decimal EARNED_UNITS { get; set; }

        public DateTime EARNED_DATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ICollection<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECT { get; set; }

        public virtual PROGRESS PROGRESS { get; set; }
    }
}
