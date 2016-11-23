namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TAX_RATES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public double? RATE { get; set; }

        [StringLength(30)]
        public string NAME { get; set; }

        [StringLength(6)]
        public string SHORTNAME { get; set; }

        public double? BASE { get; set; }

        public int? GLACC { get; set; }

        public int? GLSUBACC { get; set; }

        [StringLength(5)]
        public string KEY_POINT { get; set; }
    }
}
