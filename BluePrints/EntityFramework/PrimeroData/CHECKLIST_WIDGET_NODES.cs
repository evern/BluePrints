namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHECKLIST_WIDGET_NODES
    {
        [Key]
        public int SEQNO { get; set; }

        public int WIDGETID { get; set; }

        public int? PARENT_SEQNO { get; set; }

        public int? SORT_ORDER { get; set; }

        [StringLength(50)]
        public string CAPTION { get; set; }

        public int? PROCNO { get; set; }

        [StringLength(100)]
        public string PROC_DATA { get; set; }

        public int STATE_PRIORITY { get; set; }

        public int? MODIFIEDBY { get; set; }

        public DateTime? MODIFIEDDATE { get; set; }

        [StringLength(100)]
        public string STATUS_FIELDNAME { get; set; }

        [StringLength(100)]
        public string STATUS_SPNAME { get; set; }

        [StringLength(1)]
        public string EXPAND_NODE { get; set; }

        [StringLength(5000)]
        public string HELP_TEXT { get; set; }
    }
}
