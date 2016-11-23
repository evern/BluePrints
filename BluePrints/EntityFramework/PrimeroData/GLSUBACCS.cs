namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLSUBACCS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SUBACCNO { get; set; }

        [StringLength(40)]
        public string NAME { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }

        [StringLength(1)]
        public string ALLOWJOURNAL { get; set; }

        public DateTime? CREATED_DATE { get; set; }
    }
}
