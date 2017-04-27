namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_CATEGORIES
    {
        [Key]
        public int CATNO { get; set; }

        [StringLength(40)]
        public string CATDESC { get; set; }

        [StringLength(15)]
        public string DISP_COLOUR { get; set; }

        [StringLength(3)]
        public string SHORTCODE { get; set; }

        public int? GANTTBAR_COLOUR { get; set; }
    }
}