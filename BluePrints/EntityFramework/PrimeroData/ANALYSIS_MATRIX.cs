namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ANALYSIS_MATRIX
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CODE_SET_SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TRAN_SEQNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(1)]
        public string TRAN_TYPE { get; set; }

        public int CODE_SEQNO { get; set; }
    }
}