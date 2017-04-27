namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class ANALYSIS_TYPES
    {
        [Key]
        [StringLength(1)]
        public string TRAN_TYPE { get; set; }

        [StringLength(50)]
        public string TRAN_TABLE { get; set; }

        [StringLength(100)]
        public string DESCRIPT { get; set; }
    }
}