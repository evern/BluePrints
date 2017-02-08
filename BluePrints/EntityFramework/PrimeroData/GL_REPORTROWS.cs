namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_REPORTROWS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REPORTNO { get; set; }

        [StringLength(20)]
        public string REPORTACTION { get; set; }

        [StringLength(100)]
        public string CAPTION { get; set; }

        public int? ACC1 { get; set; }

        public int? ACC2 { get; set; }

        [StringLength(1)]
        public string DRCR { get; set; }

        public int? BRANCHNO { get; set; }

        [StringLength(250)]
        public string SQL_LOGIC { get; set; }

        [StringLength(1)]
        public string RESETSUBTOT { get; set; }

        [StringLength(1)]
        public string RESETTOT { get; set; }

        [StringLength(1)]
        public string RESETGRTOT { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }
    }
}