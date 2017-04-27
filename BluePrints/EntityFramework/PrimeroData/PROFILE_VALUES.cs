namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROFILE_VALUES
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PROFILEID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string FIELDNAME { get; set; }

        [StringLength(250)]
        public string FIELDVALUE { get; set; }
    }
}