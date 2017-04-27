namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class MENU_ASSIGNMENTS
    {
        [Key]
        [StringLength(50)]
        public string USERNAME { get; set; }

        public int? MENU_NO { get; set; }

        public int? STAFFNO { get; set; }
    }
}