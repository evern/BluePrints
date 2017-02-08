namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ANALYSIS_TYPES
    {
        [Key]
        [StringLength(1)]
        public string TRAN_TYPE { get; set; }

        [StringLength(50)]
        public string TRAN_TABLE { get; set; }

        [StringLength(100)]
        public string DESCRIPT { get; set; }
    }
}