namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ADJUSTMENT_TYPES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ATNO { get; set; }

        [StringLength(12)]
        public string ATDESC { get; set; }
    }
}