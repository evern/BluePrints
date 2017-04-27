namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class DR_PRICEGROUPS
    {
        [Key]
        public int GROUPNO { get; set; }

        [Required]
        [StringLength(30)]
        public string GROUPNAME { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }
    }
}