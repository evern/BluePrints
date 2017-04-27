namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class FACT_GLOSSARY
    {
        [Key]
        [StringLength(200)]
        public string FIELDNAME { get; set; }

        public string NOTES { get; set; }

        public int? FIELDLEVEL { get; set; }
    }
}