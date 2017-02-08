namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class USER_DEF_BANK_COLS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int BANKNO { get; set; }

        public int POS { get; set; }

        public int COLNO { get; set; }

        [StringLength(50)]
        public string COLNAME { get; set; }

        public int DATATYPE { get; set; }

        public int DATAFORMAT { get; set; }

        [StringLength(3)]
        public string PADCHAR { get; set; }

        public int WIDTH { get; set; }

        [StringLength(50)]
        public string TEXT { get; set; }

        public int ALIGN { get; set; }
    }
}