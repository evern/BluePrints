namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class X_HBIZ_PURCH_REQ_STATUS_VALUES
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(255)]
        public string STATE { get; set; }
    }
}