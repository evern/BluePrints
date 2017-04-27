namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class INV_NUMBERS
    {
        [Key]
        public int INVNO { get; set; }

        public DateTime DATE_ISSUED { get; set; }

        [Required]
        [StringLength(30)]
        public string USERID { get; set; }
    }
}