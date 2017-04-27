namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class EXONET_CACHE_TABLES
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string TABLENAME { get; set; }
    }
}