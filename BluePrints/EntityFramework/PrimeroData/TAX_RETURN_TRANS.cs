namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TAX_RETURN_TRANS
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string TAXRETURNCODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(1)]
        public string DRCR { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TRANSTYPE { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(1)]
        public string ALLOCATED { get; set; }

        public int? SEQNO { get; set; }
    }
}