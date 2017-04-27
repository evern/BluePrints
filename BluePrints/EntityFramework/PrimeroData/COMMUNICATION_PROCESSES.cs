namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class COMMUNICATION_PROCESSES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPT { get; set; }
    }
}