namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class FREIGHT_COST_TYPES
    {
        [Key]
        [StringLength(15)]
        public string COSTCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public int? SPREAD_TYPE { get; set; }

        public int? ACCNO { get; set; }

        public int? GLSUBACC { get; set; }

        [Required]
        [StringLength(1)]
        public string CAN_SPREAD { get; set; }
    }
}