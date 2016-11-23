namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_HBIZ_NOTIFICATIONS
    {
        public int ID { get; set; }

        public int STAFFNO { get; set; }

        public int MODULE { get; set; }

        [StringLength(255)]
        public string LINK { get; set; }

        [StringLength(255)]
        public string SUBJECT { get; set; }

        [Column(TypeName = "text")]
        public string MESSAGE { get; set; }

        public bool IS_READ { get; set; }

        public bool IS_EMAILED { get; set; }

        public int? FROM_STAFFNO { get; set; }

        public DateTime? DATE_EMAILED { get; set; }

        public DateTime? DATE_CREATED { get; set; }

        public int? ITEM_SEQNO { get; set; }
    }
}
