namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ANALYSIS_CODES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CODESEQNO { get; set; }

        [StringLength(30)]
        public string CODENAME { get; set; }
    }
}