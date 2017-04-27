namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class Courier_Location_Depot
    {
        [Key]
        public int SeqNo { get; set; }

        [Required]
        [StringLength(128)]
        public string LocName { get; set; }

        [Required]
        [StringLength(5)]
        public string CourDepCode { get; set; }
    }
}