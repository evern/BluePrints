namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROFILE_CONSTRAINTS
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [StringLength(1)]
        public string CONSTRAINTTYPE { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }

        [StringLength(255)]
        public string CONSTRAINTSQL { get; set; }
    }
}