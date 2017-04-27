namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DATAEXPORT_CTRL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DATAEXPORT_ID { get; set; }

        public int EXPORTSEQNO { get; set; }
    }
}