namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MENU_PROCEDURES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PROCNO { get; set; }

        [StringLength(50)]
        public string PROCNAME { get; set; }

        public int? MENU_TYPE { get; set; }

        [StringLength(500)]
        public string MENU_NOTES { get; set; }

        public int LICENCEPROC { get; set; }

        [StringLength(5000)]
        public string DEFAULT_HELP_TEXT { get; set; }
    }
}
