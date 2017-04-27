namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_OTHER_REPORTS
    {
        [Key]
        public int REPORTNO { get; set; }

        [StringLength(40)]
        public string REPORTDESC { get; set; }

        [StringLength(200)]
        public string REPORT_PARAMS { get; set; }
    }
}