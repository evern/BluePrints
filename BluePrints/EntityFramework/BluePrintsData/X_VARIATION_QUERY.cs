namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_VARIATION_QUERY
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(100)]
        public string INTERNAL_NUM { get; set; }

        public DateTime? SUBMITTED { get; set; }

        public DateTime? APPROVED { get; set; }

        public DateTime? CLIENT_APPROVED { get; set; }

        public bool ADJUSTMENT_TO_BUDGET { get; set; }

        public decimal VARIATION_UNITS { get; set; }

        public Guid? GUID_SUBJOB { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_DOCTYPE { get; set; }
    }
}
