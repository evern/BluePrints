namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class GL_CLOSING_STOCK
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BS_ACCNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BS_SUBACCNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BRANCHNO { get; set; }

        public int PL_ACCNO { get; set; }

        public int PL_SUBACCNO { get; set; }

        public double CLOSING_STOCK { get; set; }

        public double OPENING_STOCK { get; set; }

        public DateTime WHENENTERED { get; set; }

        public DateTime WHENLASTUPDATED { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? PERIODNO { get; set; }
    }
}