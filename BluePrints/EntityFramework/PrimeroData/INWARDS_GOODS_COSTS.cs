namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class INWARDS_GOODS_COSTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int INWARDS_GOODS_SEQNO { get; set; }

        [StringLength(15)]
        public string COSTCODE { get; set; }

        [StringLength(40)]
        public string DETAILS { get; set; }

        public int SPREAD_TYPE { get; set; }

        public double FC_COST { get; set; }

        public double EXCHRATE { get; set; }

        public double COST { get; set; }

        public int? SHIPMENTNO { get; set; }

        public int? ACCNO { get; set; }

        public double? INVOICE_NOW { get; set; }

        public int? GLBATCH { get; set; }

        public double? FX_VAR { get; set; }

        public double? INV_FC_COST { get; set; }

        public double? INV_EXCHRATE { get; set; }

        public double? INV_COST { get; set; }

        [Required]
        [StringLength(1)]
        public string COMPLETE { get; set; }

        [Required]
        [StringLength(1)]
        public string GLPOSTED { get; set; }

        [Required]
        [StringLength(1)]
        public string CAN_SPREAD { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? AGE { get; set; }
    }
}
