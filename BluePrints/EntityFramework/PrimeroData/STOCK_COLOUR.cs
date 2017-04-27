namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_COLOUR
    {
        [Key]
        public int COLOURID { get; set; }

        [Required]
        [StringLength(5)]
        public string COLOURCODE { get; set; }

        [StringLength(30)]
        public string COLOURNAME { get; set; }

        public int? SWATCHID { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public int? SORTORDER { get; set; }
    }
}