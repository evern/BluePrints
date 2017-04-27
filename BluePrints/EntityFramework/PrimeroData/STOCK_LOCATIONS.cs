namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class STOCK_LOCATIONS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOCNO { get; set; }

        [StringLength(8)]
        public string LCODE { get; set; }

        [StringLength(30)]
        public string LNAME { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(1)]
        public string EXCLUDE_FROMVALUATION { get; set; }

        [StringLength(1)]
        public string EXCLUDE_FROMFREE_STOCK { get; set; }

        [StringLength(1)]
        public string EXCLUDE_FROMSALES { get; set; }

        [StringLength(30)]
        public string DELADDR1 { get; set; }

        [StringLength(30)]
        public string DELADDR2 { get; set; }

        [StringLength(30)]
        public string DELADDR3 { get; set; }

        [StringLength(30)]
        public string DELADDR4 { get; set; }

        [StringLength(30)]
        public string DELADDR5 { get; set; }

        [StringLength(30)]
        public string DELADDR6 { get; set; }
    }
}