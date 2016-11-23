namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BANK_DATA_FORMAT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int DATA_TYPE { get; set; }

        [StringLength(50)]
        public string DATA_FORMAT { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }
    }
}
