namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AUDITOR_TERMINALS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BRANCHNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string TERMINALID { get; set; }

        [StringLength(255)]
        public string COMPUTERID { get; set; }

        [StringLength(1)]
        public string ENABLED { get; set; }

        public int? SAVEID { get; set; }

        [StringLength(1)]
        public string EXCEPTIONLOGGING { get; set; }
    }
}