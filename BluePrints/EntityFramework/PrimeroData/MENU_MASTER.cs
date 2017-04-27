namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MENU_MASTER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MENU_NO { get; set; }

        [Required]
        [StringLength(32)]
        public string MENU_NAME { get; set; }
    }
}