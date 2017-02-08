namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ANALYSIS_CODES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CODESEQNO { get; set; }

        [StringLength(30)]
        public string CODENAME { get; set; }
    }
}