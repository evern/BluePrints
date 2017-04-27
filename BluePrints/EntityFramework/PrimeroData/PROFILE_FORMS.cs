namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class PROFILE_FORMS
    {
        [Key]
        public int PROFILEID { get; set; }

        public int STAFFNO { get; set; }

        [Required]
        [StringLength(100)]
        public string LAYOUTNAME { get; set; }

        [Required]
        [StringLength(1)]
        public string LASTUSEDLAYOUT { get; set; }

        [Required]
        public string MODULENAME { get; set; }

        [Required]
        public string WIDGETDATA { get; set; }

        [Required]
        public string LAYOUTDATA { get; set; }

        public string ZOOMSETTINGS { get; set; }

        public string WIDGETSETTINGS { get; set; }
    }
}