namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_ACCGROUPS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCGROUP { get; set; }

        [StringLength(30)]
        public string GROUPNAME { get; set; }

        public int? REPCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string DRCR { get; set; }

        public int SECTION { get; set; }

        public int BALANCE_SHEET_TYPE { get; set; }

        [Required]
        [StringLength(1)]
        public string IS_RECONCILABLE { get; set; }

        public int ACCNO_FROM { get; set; }

        public int ACCNO_TO { get; set; }
    }
}
