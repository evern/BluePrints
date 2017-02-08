namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCKTAKE_CTRL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOCNO { get; set; }

        [StringLength(30)]
        public string LOCNAME { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public DateTime? TIME_INITIALISED { get; set; }

        public DateTime? TIME_COUNTSHEETS { get; set; }

        public DateTime? TIME_KEYED { get; set; }

        public DateTime? TIME_UPLOADED { get; set; }

        [StringLength(100)]
        public string CUSTOM_FILTER { get; set; }
    }
}