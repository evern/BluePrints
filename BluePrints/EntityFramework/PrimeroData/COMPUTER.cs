namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("COMPUTER")]
    public partial class COMPUTER
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(100)]
        public string COMPUTERID { get; set; }

        [StringLength(100)]
        public string CLIENTNAME { get; set; }

        [Required]
        [StringLength(40)]
        public string COMPUTERNAME { get; set; }

        public int COMPUTERPROFILEID { get; set; }

        public int USERPROFILEID { get; set; }

        public int SECURITYPROFILEID { get; set; }

        [StringLength(15)]
        public string EFTCAID { get; set; }

        public int? EFTTRANID { get; set; }

        [StringLength(1)]
        public string EFTTRANSTATE { get; set; }
    }
}