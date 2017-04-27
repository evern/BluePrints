namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class MENU_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        public int MENUNO { get; set; }

        public int HDRCLASS { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(1)]
        public string ISENABLED { get; set; }
    }
}