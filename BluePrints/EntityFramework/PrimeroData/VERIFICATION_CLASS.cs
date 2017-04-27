namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class VERIFICATION_CLASS
    {
        [Key]
        [StringLength(5)]
        public string V_CLASS { get; set; }

        [StringLength(120)]
        public string DESCRIPT { get; set; }

        [StringLength(1024)]
        public string HELPTEXT { get; set; }

        [StringLength(60)]
        public string PROCNAME { get; set; }

        public int? VLEVEL { get; set; }

        [StringLength(60)]
        public string SOURCE { get; set; }
    }
}