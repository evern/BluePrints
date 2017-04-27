namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class DASHBOARDS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public int DASHBOARDTYPE { get; set; }

        public string SETTINGS { get; set; }

        public int REFRESH_FREQ { get; set; }

        public int ACCESS_RIGHTS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime CREATEDDATE { get; set; }

        public int MODIFIEDBY { get; set; }

        public DateTime? MODIFIEDDATE { get; set; }

        public int MODULE_VISIBILITY { get; set; }
    }
}