namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLREPORTHEADER")]
    public partial class GLREPORTHEADER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REPORT_SEQNO { get; set; }

        [StringLength(30)]
        public string HEADERTYPE { get; set; }

        [StringLength(30)]
        public string HEADER1 { get; set; }

        [StringLength(30)]
        public string HEADER2 { get; set; }

        [StringLength(30)]
        public string HEADER3 { get; set; }

        [StringLength(30)]
        public string HEADER4 { get; set; }

        [StringLength(30)]
        public string HEADER5 { get; set; }

        [StringLength(30)]
        public string HEADER6 { get; set; }

        [StringLength(30)]
        public string HEADER7 { get; set; }

        [StringLength(30)]
        public string HEADER8 { get; set; }

        [StringLength(30)]
        public string HEADER9 { get; set; }

        [StringLength(30)]
        public string HEADER10 { get; set; }

        [StringLength(30)]
        public string HEADER11 { get; set; }

        [StringLength(30)]
        public string HEADER12 { get; set; }

        [StringLength(30)]
        public string HEADER13 { get; set; }

        [StringLength(30)]
        public string HEADER14 { get; set; }

        [StringLength(30)]
        public string HEADER15 { get; set; }

        [StringLength(30)]
        public string HEADER16 { get; set; }

        [StringLength(30)]
        public string HEADER17 { get; set; }

        [StringLength(30)]
        public string HEADER18 { get; set; }

        [StringLength(30)]
        public string HEADER19 { get; set; }

        [StringLength(30)]
        public string HEADER20 { get; set; }

        [StringLength(30)]
        public string HEADER21 { get; set; }

        [StringLength(30)]
        public string HEADER22 { get; set; }

        [StringLength(30)]
        public string HEADER23 { get; set; }

        [StringLength(30)]
        public string HEADER24 { get; set; }

        [StringLength(30)]
        public string HEADER25 { get; set; }

        [StringLength(30)]
        public string HEADER26 { get; set; }

        [StringLength(30)]
        public string HEADER27 { get; set; }

        [StringLength(30)]
        public string HEADER28 { get; set; }

        [StringLength(30)]
        public string HEADER29 { get; set; }

        [StringLength(30)]
        public string HEADER30 { get; set; }

        [StringLength(30)]
        public string HEADER31 { get; set; }

        [StringLength(30)]
        public string HEADER32 { get; set; }

        [StringLength(30)]
        public string HEADER33 { get; set; }

        [StringLength(30)]
        public string HEADER34 { get; set; }

        [StringLength(30)]
        public string HEADER35 { get; set; }

        [StringLength(30)]
        public string HEADER36 { get; set; }

        [StringLength(30)]
        public string HEADER37 { get; set; }

        [StringLength(30)]
        public string HEADER38 { get; set; }

        [StringLength(30)]
        public string HEADER39 { get; set; }

        [StringLength(30)]
        public string HEADER40 { get; set; }

        [StringLength(30)]
        public string HEADER41 { get; set; }

        [StringLength(30)]
        public string HEADER42 { get; set; }

        [StringLength(30)]
        public string HEADER43 { get; set; }

        [StringLength(30)]
        public string HEADER44 { get; set; }

        [StringLength(30)]
        public string HEADER45 { get; set; }

        [StringLength(30)]
        public string HEADER46 { get; set; }

        [StringLength(30)]
        public string HEADER47 { get; set; }

        [StringLength(30)]
        public string HEADER48 { get; set; }

        [StringLength(30)]
        public string HEADER49 { get; set; }

        [StringLength(30)]
        public string HEADER50 { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }
    }
}
