namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class BANK_REC_AUTO_MATCH
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GLSEQNO { get; set; }

        [StringLength(50)]
        public string GLCHEQUENO { get; set; }

        [StringLength(30)]
        public string GLDETAILS { get; set; }

        public int? BRSEQNO { get; set; }

        [StringLength(50)]
        public string BRCHEQUENO { get; set; }

        [StringLength(30)]
        public string BRTRANSDATE { get; set; }

        [StringLength(255)]
        public string BRDETAILS { get; set; }

        [StringLength(1)]
        public string ISRECONCILE { get; set; }

        public double? BRAMOUNT { get; set; }

        public double? GLAMOUNT { get; set; }

        public DateTime? GLTRANSDATE { get; set; }
    }
}