namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PAYMENT_TYPES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PTNO { get; set; }

        [StringLength(12)]
        public string PTDESC { get; set; }

        [StringLength(1)]
        public string PTKEY { get; set; }

        [StringLength(80)]
        public string IMG_FILE { get; set; }

        [StringLength(1)]
        public string WEB_SHOW { get; set; }

        [StringLength(1)]
        public string LIVE_TRANS { get; set; }

        [StringLength(6)]
        public string SHORTNAME { get; set; }

        public int? PTGROUP { get; set; }

        public int? CURRENCYNO { get; set; }

        public int? LISTSEQ { get; set; }

        [StringLength(1)]
        public string REFUND { get; set; }

        [StringLength(1)]
        public string OVER_TEND { get; set; }

        public double? MIN_TEND { get; set; }

        public double? MAX_TEND { get; set; }

        public double? MAX_PAYOUT { get; set; }

        public int? EFTPOS { get; set; }

        public int? MEMBER_TYPE { get; set; }

        [StringLength(20)]
        public string ACC_MASK { get; set; }

        public int? ROUND_AMT { get; set; }

        [StringLength(1)]
        public string ROUND_UP { get; set; }

        public double? FEE_AMT { get; set; }

        public double? FEE_MAX { get; set; }

        [StringLength(40)]
        public string FEE_STOCKITEM { get; set; }

        public int? IMAGE_FILE_INDEX { get; set; }

        [StringLength(1)]
        public string ACTIVE_DR { get; set; }

        [StringLength(1)]
        public string ACTIVE_CR { get; set; }

        [StringLength(1)]
        public string ACTIVE_POS { get; set; }

        [StringLength(1)]
        public string CHEQUE_TYPE { get; set; }

        [StringLength(1)]
        public string DIRECT_DEBIT_TYPE { get; set; }

        [StringLength(1)]
        public string DIRECT_CREDIT_TYPE { get; set; }

        public int CASHOUTPTNO { get; set; }

        [StringLength(1)]
        public string BANKFEE { get; set; }

        [StringLength(1)]
        public string ZERO_TEND { get; set; }

        public int? GATEWAY { get; set; }

        [StringLength(1)]
        public string POS_SALE { get; set; }

        [StringLength(1)]
        public string POS_RECEIPT { get; set; }

        [StringLength(1)]
        public string POS_CREDIT { get; set; }

        [StringLength(1)]
        public string POS_REFUND { get; set; }

        [StringLength(1)]
        public string POS_QUOTE { get; set; }

        [StringLength(1)]
        public string POS_LAYBY { get; set; }

        public int? REFERENCELEVEL { get; set; }

        [StringLength(1)]
        public string INCASHDRAWER { get; set; }

        [StringLength(1)]
        public string MPOWERED_TYPE { get; set; }
    }
}