namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PERIODS_DEFN_NEXTFINYR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(20)]
        public string PERIODNAME { get; set; }

        [StringLength(1)]
        public string CALMONTHS { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? STOPDATE { get; set; }

        [StringLength(8)]
        public string PERIOD_SHORTNAME { get; set; }

        [StringLength(2)]
        public string REPORTCODE { get; set; }

        public int FIN_QTR { get; set; }
    }
}
