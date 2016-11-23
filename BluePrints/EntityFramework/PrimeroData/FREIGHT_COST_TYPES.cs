namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FREIGHT_COST_TYPES
    {
        [Key]
        [StringLength(15)]
        public string COSTCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public int? SPREAD_TYPE { get; set; }

        public int? ACCNO { get; set; }

        public int? GLSUBACC { get; set; }

        [Required]
        [StringLength(1)]
        public string CAN_SPREAD { get; set; }
    }
}
