namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PAYMENT_GROUP
    {
        [Key]
        public int PGNO { get; set; }

        [StringLength(12)]
        public string PGDESC { get; set; }

        public int? GLCODE { get; set; }

        public int? GLSUBCODE { get; set; }

        [StringLength(1)]
        public string INFLOAT { get; set; }

        [StringLength(1)]
        public string BANKABLE { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(1)]
        public string CALC_BANKFEE { get; set; }

        public int? BANKFEE_GLCODE { get; set; }

        public int? BANKFEE_GLSUBCODE { get; set; }

        public int BANKFEE_METHOD { get; set; }

        public double BANKFEE_RATE { get; set; }

        public double BANKFEE_MAX { get; set; }
    }
}