namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AUDITOR_EVENTS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EVENTID { get; set; }

        public int? CLASSID { get; set; }

        [StringLength(50)]
        public string CLASSNAME { get; set; }

        [StringLength(255)]
        public string EVENTNAME { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }
    }
}