namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CREDIT_STATUSES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STATUSNO { get; set; }

        [StringLength(30)]
        public string STATUSDESC { get; set; }

        public int CREDIT_FACTOR { get; set; }

        [StringLength(1)]
        public string ACTIVE_DR { get; set; }

        [StringLength(1)]
        public string ACTIVE_CR { get; set; }

        [StringLength(60)]
        public string BAL_WARNING_SQL { get; set; }

        [StringLength(100)]
        public string WARNING_TEXT { get; set; }
    }
}
