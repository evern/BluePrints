namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POS_COUNT
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SHIFTNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PTNO { get; set; }

        [StringLength(12)]
        public string PTDESC { get; set; }

        public double SHIFT_VALUE { get; set; }

        public double COUNT1_VALUE { get; set; }

        public double COUNT2_VALUE { get; set; }

        public double COUNT2_ADJUST { get; set; }

        public double? FLOAT_VALUE { get; set; }
    }
}