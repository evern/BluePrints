namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHEQUE_AUDIT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHQNO { get; set; }

        [StringLength(1)]
        public string PRINTOK { get; set; }

        public double? AMOUNT { get; set; }

        public int ACCNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(1)]
        public string GLPOSTED { get; set; }

        public int? GLCODE1 { get; set; }

        public int? GLCODE2 { get; set; }

        public int? GLCODE3 { get; set; }

        public int? GLSUB1 { get; set; }

        public int? GLSUB2 { get; set; }

        public int? GLSUB3 { get; set; }

        public int? GLBRANCH1 { get; set; }

        public int? GLBRANCH2 { get; set; }

        public int? GLBRANCH3 { get; set; }

        public double? GLAMOUNT1 { get; set; }

        public double? GLAMOUNT2 { get; set; }

        public double? GLAMOUNT3 { get; set; }

        [Required]
        [StringLength(1)]
        public string MANUAL_CHQ { get; set; }

        [StringLength(500)]
        public string DETAILS { get; set; }

        public int? GLCODE4 { get; set; }

        public double? GLAMOUNT4 { get; set; }

        public int? GLCODE5 { get; set; }

        public double? GLAMOUNT5 { get; set; }

        public int? GLCODE6 { get; set; }

        public double? GLAMOUNT6 { get; set; }

        public int? GLCODE7 { get; set; }

        public double? GLAMOUNT7 { get; set; }
    }
}
