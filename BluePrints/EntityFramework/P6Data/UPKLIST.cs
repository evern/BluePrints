namespace BluePrints.P6Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UPKLIST")]
    public partial class UPKLIST
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int session_id { get; set; }

        public int? context_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int pk_id { get; set; }

        public int? pk2_id { get; set; }

        public virtual USESSION USESSION { get; set; }
    }
}