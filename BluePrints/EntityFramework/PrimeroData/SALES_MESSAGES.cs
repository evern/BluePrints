namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SALES_MESSAGES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(40)]
        public string MESSAGE1 { get; set; }

        [StringLength(40)]
        public string MESSAGE2 { get; set; }

        [StringLength(40)]
        public string MESSAGE3 { get; set; }

        [StringLength(40)]
        public string MESSAGE4 { get; set; }
    }
}
