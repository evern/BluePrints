namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class DASHBOARDS_STAFF
    {
        [Key]
        public int SEQNO { get; set; }

        public int? STAFFNO { get; set; }

        public int? DASHBOARDSEQNO { get; set; }
    }
}