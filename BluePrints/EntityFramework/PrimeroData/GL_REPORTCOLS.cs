namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_REPORTCOLS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REPORTNO { get; set; }

        [StringLength(30)]
        public string COLNAME { get; set; }

        [StringLength(30)]
        public string COLTYPE { get; set; }

        [StringLength(30)]
        public string FIELDNAME { get; set; }

        [StringLength(10)]
        public string FORMATCODE { get; set; }

        public int? PERIODNO { get; set; }

        public int? FROMPERIOD { get; set; }

        public int? COLBRANCH { get; set; }

        public int? SOURCEROWSEQNO { get; set; }

        public int? SOURCECOLSEQNO { get; set; }

        public int BUDGETID { get; set; }

        [StringLength(1)]
        public string GROUPBYPERIOD { get; set; }

        public int FRAME_LENGTH { get; set; }

        [StringLength(1)]
        public string SORT_OLDTONEW { get; set; }
    }
}
