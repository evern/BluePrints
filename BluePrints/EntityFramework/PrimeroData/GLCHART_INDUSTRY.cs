namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class GLCHART_INDUSTRY
    {
        [Key]
        public int INDUSTRYNO { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }
    }
}