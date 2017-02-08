namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("COUNTRY")]
    public partial class COUNTRY
    {
        [Key]
        [StringLength(5)]
        public string COUNTRY_CODE { get; set; }

        [StringLength(30)]
        public string COUNTRY_NAME { get; set; }

        [StringLength(5)]
        public string TAXNAME { get; set; }

        [StringLength(15)]
        public string TAXNO_NAME { get; set; }

        [StringLength(1)]
        public string TAX_IN_PP { get; set; }
    }
}