namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PAYROLL_EMPLOYEES
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EMPLOYEE_CODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string LAST_NAME { get; set; }

        [StringLength(50)]
        public string FIRST_NAME { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }
    }
}