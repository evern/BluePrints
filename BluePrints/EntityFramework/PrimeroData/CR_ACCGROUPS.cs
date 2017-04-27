namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class CR_ACCGROUPS
    {
        [Key]
        public int ACCGROUP { get; set; }

        [StringLength(30)]
        public string GROUPNAME { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }
    }
}