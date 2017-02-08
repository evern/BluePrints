namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class COURIER_MANIFEST
    {
        [Key]
        public int SEQNO { get; set; }

        public DateTime TRANSDATE { get; set; }

        public int? COURNO { get; set; }

        public int? ORDERNO { get; set; }

        public int? CARTONS { get; set; }

        [StringLength(100)]
        public string NOTES { get; set; }

        [StringLength(50)]
        public string TICKETNO { get; set; }

        public int? WEIGHT { get; set; }

        public double? CM1 { get; set; }

        public double? CM2 { get; set; }

        public double? CM3 { get; set; }

        [StringLength(50)]
        public string SERVICE_TYPE { get; set; }

        [StringLength(50)]
        public string ADDNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string ADDRESS1 { get; set; }

        [Required]
        [StringLength(50)]
        public string ADDRESS2 { get; set; }

        [Required]
        [StringLength(50)]
        public string ADDRESS3 { get; set; }

        [Required]
        [StringLength(50)]
        public string ADDRESS4 { get; set; }

        [Required]
        [StringLength(50)]
        public string ADDRESS5 { get; set; }

        public int? UNIQUEID { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public double? FREIGHT_CHARGE { get; set; }

        public int? DISPMETHOD { get; set; }

        public DateTime? WHENCOURIER { get; set; }

        public int? COURIER_MODE { get; set; }

        [StringLength(1)]
        public string LINK_TYPE { get; set; }

        public int? LINK_ORDER { get; set; }
    }
}