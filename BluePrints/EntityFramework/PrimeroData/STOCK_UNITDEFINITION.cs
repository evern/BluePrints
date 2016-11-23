namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_UNITDEFINITION
    {
        [Key]
        [StringLength(10)]
        public string UNITCODE { get; set; }

        [StringLength(30)]
        public string UNITDESCRIPTION { get; set; }
    }
}
