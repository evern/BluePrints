namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PAYROLL_PAYRATES
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PAYRATE_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        public double PAYRATE { get; set; }

        [StringLength(15)]
        public string DESCRIPTION { get; set; }
    }
}