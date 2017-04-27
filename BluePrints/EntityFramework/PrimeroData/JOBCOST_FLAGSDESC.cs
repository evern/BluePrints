namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class JOBCOST_FLAGSDESC
    {
        [Key]
        [StringLength(8)]
        public string FLAGCODE { get; set; }

        [StringLength(60)]
        public string FLAGDESC { get; set; }
    }
}