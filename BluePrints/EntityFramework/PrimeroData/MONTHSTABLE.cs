namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MONTHSTABLE")]
    public partial class MONTHSTABLE
    {
        [Key]
        public int MONTH { get; set; }

        [StringLength(3)]
        public string MONTHNAMESHORT { get; set; }

        [StringLength(20)]
        public string MONTHNAMELONG { get; set; }
    }
}