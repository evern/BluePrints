namespace BluePrints.P6Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("NEXTKEY")]
    public partial class NEXTKEY
    {
        [Key]
        [StringLength(30)]
        public string key_name { get; set; }

        public int key_seq_num { get; set; }
    }
}