namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class CR_LIST_NAME
    {
        [Key]
        public int LIST_NO { get; set; }

        [StringLength(50)]
        public string LIST_NAME { get; set; }
    }
}