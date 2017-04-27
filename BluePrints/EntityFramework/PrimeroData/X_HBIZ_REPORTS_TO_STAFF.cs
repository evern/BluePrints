namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class X_HBIZ_REPORTS_TO_STAFF
    {
        [Key]
        public int SEQNO { get; set; }

        public int STAFFNO { get; set; }

        public int REPORTS_TO_STAFFNO { get; set; }

        public bool DEFAULT { get; set; }
    }
}