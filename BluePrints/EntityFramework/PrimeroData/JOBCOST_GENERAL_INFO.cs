namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOBCOST_GENERAL_INFO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? SOFTWARE_VERSION { get; set; }

        public int? DATAVERSION { get; set; }

        [StringLength(1000)]
        public string QUOTE_TERMS { get; set; }

        public int? STOCKLOC { get; set; }

        public int? JOBCAT { get; set; }

        [StringLength(1)]
        public string SUBSADDUP { get; set; }

        [StringLength(1)]
        public string JOBSTAT { get; set; }

        public int? JOBTYPE { get; set; }

        [Required]
        [StringLength(1)]
        public string MOVE_STOCK { get; set; }

        [StringLength(500)]
        public string COSTGLSQL { get; set; }

        [StringLength(500)]
        public string SALESGLSQL { get; set; }

        public int? DEFDAYS { get; set; }

        public int? DEFHOURS { get; set; }

        [StringLength(30)]
        public string OURREF { get; set; }

        [StringLength(1)]
        public string PRINTDESC { get; set; }

        [StringLength(1)]
        public string OPTION_1 { get; set; }

        [StringLength(1)]
        public string OPTION_2 { get; set; }

        [StringLength(1)]
        public string OPTION_3 { get; set; }

        [StringLength(1)]
        public string OPTION_4 { get; set; }

        [StringLength(1)]
        public string OPTION_5 { get; set; }

        [StringLength(1)]
        public string OPTION_6 { get; set; }

        [StringLength(1)]
        public string OPTION_7 { get; set; }

        [StringLength(1)]
        public string OPTION_8 { get; set; }

        [StringLength(1)]
        public string OPTION_9 { get; set; }

        [StringLength(1)]
        public string OPTION_10 { get; set; }

        [StringLength(1)]
        public string JOBNO_PROMPT { get; set; }

        [StringLength(1)]
        public string SCHEDULER { get; set; }

        [StringLength(255)]
        public string TIMESELECTSQL { get; set; }

        [StringLength(255)]
        public string COSTSELECTSQL { get; set; }

        public int? STOCKCOST { get; set; }

        public int? GLACC_TO_USE { get; set; }

        public int? WORKSHEETTYPE { get; set; }

        [StringLength(1)]
        public string SHOWMAIN { get; set; }

        [StringLength(1)]
        public string WIPNOTSOH { get; set; }

        [StringLength(23)]
        public string DEFNONSTOCKCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string USE_SALES_ORD_NO { get; set; }

        public int QUOTECOST { get; set; }

        public int DEFFOLLOWUPDAYS { get; set; }
    }
}