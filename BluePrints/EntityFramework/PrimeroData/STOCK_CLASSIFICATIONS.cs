namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_CLASSIFICATIONS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CLASSNO { get; set; }

        [Required]
        [StringLength(100)]
        public string CLASSNAME { get; set; }
    }
}