namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class JOB_COSTTYPES
    {
        [NotMapped]
        public bool IsValid { get; set; }
    }
}