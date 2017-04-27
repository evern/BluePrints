namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class REASON_CLASS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CLASSNO { get; set; }

        [Required]
        [StringLength(100)]
        public string CLASSNAME { get; set; }

        [StringLength(1)]
        public string ACTIVE { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(1)]
        public string COPYTO_HIST { get; set; }

        [StringLength(25)]
        public string REF1_NAME { get; set; }

        public int? REF1_LEVEL { get; set; }
    }
}