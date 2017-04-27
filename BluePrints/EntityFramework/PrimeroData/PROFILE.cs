namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PROFILE")]
    public partial class PROFILE
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string PROFILENAME { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }

        public int? PROFILETYPE { get; set; }

        [Required]
        [StringLength(100)]
        public string PATHREF { get; set; }

        public int HASHVAL { get; set; }

        public int PARENTID { get; set; }
    }
}