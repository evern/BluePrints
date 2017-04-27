namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PRICE_NAMES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRICENO { get; set; }

        [StringLength(20)]
        public string NAME { get; set; }

        [StringLength(100)]
        public string PRICE_CALC_SQL { get; set; }

        public int? CURRENCYNO { get; set; }

        [StringLength(1)]
        public string SYSTEM_PRICETAX_EXCEPTION { get; set; }
    }
}