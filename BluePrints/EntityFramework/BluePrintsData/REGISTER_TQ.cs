namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_TQ
    {
        [Key]
        public Guid GUID { get; set; }

        [StringLength(150)]
        public string NUMBER { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public Guid GUID_DISCIPLINE { get; set; }

        public Guid? GUID_RAISEDBY { get; set; }

        public DateTime? DATE_RAISED { get; set; }

        [StringLength(100)]
        public string ADDRESSED_TO { get; set; }

        public DateTime? DATE_REQUESTED { get; set; }

        public DateTime? DATE_RESPONSE { get; set; }

        [StringLength(100)]
        public string SUBJECT { get; set; }

        [StringLength(4000)]
        public string COMMENTS { get; set; }

        public DateTime? DATE_RESPONDED { get; set; }

        public int? RESPONSE_DAYS { get; set; }

        [StringLength(4000)]
        public string RESPONSE { get; set; }

        public int? STATUS { get; set; }

        public int? OPENCLOSE { get; set; }

        [StringLength(4000)]
        public string NEW_COMMENTS { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual REGISTER_TQ REGISTER_TQ1 { get; set; }

        public virtual REGISTER_TQ REGISTER_TQ2 { get; set; }
    }
}
