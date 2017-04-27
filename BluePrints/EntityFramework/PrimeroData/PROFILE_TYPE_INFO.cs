namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROFILE_TYPE_INFO
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string TYPENAME { get; set; }

        [Required]
        [StringLength(30)]
        public string DATATYPE { get; set; }

        [StringLength(100)]
        public string TYPESQL { get; set; }
    }
}