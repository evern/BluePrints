namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class INV_FIELD_NAMES
    {
        [Key]
        public int INVTYPE { get; set; }

        [StringLength(20)]
        public string FIELDNAME { get; set; }
    }
}