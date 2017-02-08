namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BANKS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int BANKNO { get; set; }

        [StringLength(50)]
        public string NAME { get; set; }

        public int? DEFBANK { get; set; }

        public int? HASHEADER { get; set; }

        public int? HASFOOTER { get; set; }

        public int? BANKFORMAT { get; set; }

        [StringLength(50)]
        public string PATH { get; set; }

        [StringLength(8)]
        public string FILENAME { get; set; }

        [StringLength(3)]
        public string EXTENSION { get; set; }

        [StringLength(1)]
        public string TRNSFR_TYPE { get; set; }

        [StringLength(30)]
        public string USER_BANK { get; set; }

        [StringLength(30)]
        public string USER_NUMBER { get; set; }

        [StringLength(30)]
        public string PAYER_BANK_ACC { get; set; }

        public int BANKTEMPLATE_SEQNO { get; set; }

        [StringLength(10)]
        public string BSB_NUMBER { get; set; }

        [StringLength(40)]
        public string COMPANY_TRADING_NAME { get; set; }
    }
}