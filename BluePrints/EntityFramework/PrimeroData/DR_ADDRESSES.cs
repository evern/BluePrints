namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class DR_ADDRESSES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

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

        public int? COURIER_DEPOT_SEQNO { get; set; }
    }
}