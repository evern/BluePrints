namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SHIPMENT_COST_DETAILS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int RECEIPTNO { get; set; }

        [StringLength(15)]
        public string COSTCODE { get; set; }

        [StringLength(40)]
        public string DETAILS { get; set; }

        public double? FC_COST { get; set; }

        public double? EXCHRATE { get; set; }

        public double? COST { get; set; }
    }
}
