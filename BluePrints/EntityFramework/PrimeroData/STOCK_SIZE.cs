namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_SIZE
    {
        [Key]
        public int SIZEID { get; set; }

        [Required]
        [StringLength(5)]
        public string SIZECODE { get; set; }

        [StringLength(30)]
        public string SIZENAME { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public int? SORTORDER { get; set; }
    }
}