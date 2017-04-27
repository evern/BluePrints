namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROFILE_GRIDS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PROFILEID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPUTERSEQNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(200)]
        public string REGKEY { get; set; }

        [Required]
        public byte[] DATA { get; set; }
    }
}