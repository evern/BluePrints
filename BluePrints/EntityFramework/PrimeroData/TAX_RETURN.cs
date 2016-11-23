namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TAX_RETURN
    {
        [Key]
        [StringLength(15)]
        public string TAXRETURNCODE { get; set; }

        public int BASIS { get; set; }

        public int FREQUENCY { get; set; }

        public DateTime FROMDATE { get; set; }

        public DateTime TODATE { get; set; }

        [StringLength(30)]
        public string TAXREGNO { get; set; }

        public DateTime? DUEDATE { get; set; }

        [Required]
        [StringLength(1)]
        public string INCLUDE_PREVIOUS { get; set; }

        public int? CALC_ID { get; set; }

        [StringLength(1)]
        public string USED_PERIODS { get; set; }

        [StringLength(1)]
        public string GROSS_FROM_TAX { get; set; }

        public int DR_FROM_PERIOD_SEQNO { get; set; }

        public int? DR_FROM_AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? DR_FROM_PERIOD { get; set; }

        public int DR_TO_PERIOD_SEQNO { get; set; }

        public int? DR_TO_AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? DR_TO_PERIOD { get; set; }

        public int CR_FROM_PERIOD_SEQNO { get; set; }

        public int? CR_FROM_AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? CR_FROM_PERIOD { get; set; }

        public int CR_TO_PERIOD_SEQNO { get; set; }

        public int? CR_TO_AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? CR_TO_PERIOD { get; set; }
    }
}
