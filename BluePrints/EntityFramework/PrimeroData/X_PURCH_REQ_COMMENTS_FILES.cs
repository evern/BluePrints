namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class X_PURCH_REQ_COMMENTS_FILES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? PURCH_REQ_COMMENT_SEQNO { get; set; }

        public decimal? FILESIZE { get; set; }

        [StringLength(255)]
        public string FILENAME { get; set; }

        [StringLength(4)]
        public string FILETYPE { get; set; }

        public bool? IS_IMAGE { get; set; }
    }
}