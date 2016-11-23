namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROFILE_CONSTRAINTS
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [StringLength(1)]
        public string CONSTRAINTTYPE { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }

        [StringLength(255)]
        public string CONSTRAINTSQL { get; set; }
    }
}
