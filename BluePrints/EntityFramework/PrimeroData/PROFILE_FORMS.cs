namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

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
