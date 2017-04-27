namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class PROSPECT_CONTACTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? PROSPECT_SEQNO { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        [Required]
        [StringLength(1)]
        public string DEFCONTACT { get; set; }

        [Required]
        [StringLength(1)]
        public string DEFACCOUNT { get; set; }
    }
}