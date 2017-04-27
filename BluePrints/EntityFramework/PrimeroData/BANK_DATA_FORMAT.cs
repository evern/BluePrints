namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class BANK_DATA_FORMAT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int DATA_TYPE { get; set; }

        [StringLength(50)]
        public string DATA_FORMAT { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }
    }
}