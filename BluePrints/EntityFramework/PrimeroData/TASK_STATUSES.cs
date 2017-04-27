namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class TASK_STATUSES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }
    }
}