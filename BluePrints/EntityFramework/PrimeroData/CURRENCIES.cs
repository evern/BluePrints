namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CURRENCIES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CURRENCYNO { get; set; }

        [StringLength(3)]
        public string CURRCODE { get; set; }

        [StringLength(30)]
        public string CURRNAME { get; set; }

        public double? BUYRATE { get; set; }

        public double? SELLRATE { get; set; }

        [StringLength(5)]
        public string CURRSYMBOL { get; set; }

        public int ALERTPC { get; set; }
    }
}
