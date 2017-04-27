namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class CONTACT_LIST_TYPE
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPT { get; set; }
    }
}