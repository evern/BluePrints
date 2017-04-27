namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DISPMETHOD")]
    public partial class DISPMETHOD
    {
        [Key]
        public int SEQ_NO { get; set; }

        [StringLength(30)]
        public string DESCRIPTION { get; set; }
    }
}