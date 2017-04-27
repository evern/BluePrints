namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class CAMPAIGN_WAVE_CONTACT_LISTS
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
        public int CONTACT_LIST_SEQNO { get; set; }
    }
}