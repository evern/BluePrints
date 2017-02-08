namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CAMPAIGN_WAVE_AUDIT
    {
        [Key]
        [Column(Order = 0)]
        public int SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CAMPAIGN_WAVE_SEQNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONTACT_SEQNO { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime LOGDATETIME { get; set; }

        public int? COMMUNICATION_METHOD { get; set; }

        [StringLength(15)]
        public string EXECUTE_TYPE { get; set; }
    }
}