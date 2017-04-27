namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class SALESORD_STATUS
    {
        [Key]
        public int SEQNO { get; set; }

        public int STATUSNO { get; set; }

        [Required]
        [StringLength(30)]
        public string DESCRIPTION { get; set; }
    }
}