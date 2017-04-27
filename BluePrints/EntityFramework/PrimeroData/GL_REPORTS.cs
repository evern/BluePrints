namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class GL_REPORTS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(40)]
        public string REPORTNAME { get; set; }

        public int? PAGEBREAK { get; set; }

        [StringLength(30)]
        public string FONTNAME { get; set; }

        public int? FONTSIZE { get; set; }

        [StringLength(10)]
        public string ORIENTATION { get; set; }

        [StringLength(20)]
        public string USERHEADING { get; set; }

        [StringLength(20)]
        public string PERIODBLURB { get; set; }

        [StringLength(20)]
        public string PAGENOLOCATION { get; set; }

        [StringLength(1)]
        public string REPEATHEADER { get; set; }

        [StringLength(1)]
        public string PERIODNOINHEADER { get; set; }

        [StringLength(1)]
        public string PERIODYRINHEADER { get; set; }

        [StringLength(1)]
        public string SHORTPERIODS { get; set; }

        public int? REPORTUNITS { get; set; }

        [StringLength(1)]
        public string SUPPRESSZEROS { get; set; }

        public short? DECIMALPLACES { get; set; }

        [StringLength(1)]
        public string COMMAFORMAT { get; set; }

        [StringLength(1)]
        public string SHOWVARIANCEASPC { get; set; }

        [StringLength(1)]
        public string LISTSUBACCOUNTS { get; set; }

        [StringLength(30)]
        public string REPORTTABLENAME { get; set; }

        [StringLength(1)]
        public string USECLARITY { get; set; }

        [StringLength(1)]
        public string RENDERINEXCEL { get; set; }

        [StringLength(1)]
        public string DEFPLROLLINGCYEARONLY { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        [Required]
        [StringLength(1)]
        public string VALIDATEATRUNTIME { get; set; }
    }
}