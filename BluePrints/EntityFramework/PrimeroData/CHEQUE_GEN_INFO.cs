namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHEQUE_GEN_INFO
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PPVERSION { get; set; }

        [StringLength(1)]
        public string SUMDC { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHQMIN { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHQMAX { get; set; }

        [StringLength(1)]
        public string PRINTREMIT { get; set; }

        [StringLength(1)]
        public string POPEMAIL { get; set; }

        [StringLength(1)]
        public string FCWARNING { get; set; }

        [StringLength(1)]
        public string EMAILREMIT { get; set; }

        public int? GLACC { get; set; }

        public int? SUBACC { get; set; }

        [StringLength(1)]
        public string MULTIPLE_BANK_DC { get; set; }

        public int? USESEPCHQREP { get; set; }

        public int? CHQPERPAGE { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(1)]
        public string USEMAILSHOT { get; set; }
    }
}