namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOB_TYPES
    {
        [Key]
        public int TYPENO { get; set; }

        [StringLength(40)]
        public string TYPEDESC { get; set; }

        [StringLength(3)]
        public string SHORTCODE { get; set; }

        public int? GANTTBAR_COLOUR { get; set; }
    }
}