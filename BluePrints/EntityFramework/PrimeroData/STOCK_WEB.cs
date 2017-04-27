namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class STOCK_WEB
    {
        [Key]
        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(4096)]
        public string SALES_HTML { get; set; }

        [StringLength(80)]
        public string PICTURE_URL { get; set; }
    }
}