namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SMTP_ACCOUNTS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(30)]
        public string DESCRIPTION { get; set; }

        public int ACC_TYPE { get; set; }

        [StringLength(10)]
        public string REPORTCODE { get; set; }

        [StringLength(100)]
        public string SMTP_SERVER { get; set; }

        [StringLength(60)]
        public string SMTP_USER { get; set; }

        [StringLength(1)]
        public string AUTHENTICATE { get; set; }

        [StringLength(30)]
        public string PASS_WORD { get; set; }

        [StringLength(30)]
        public string FROM_NAME { get; set; }

        [StringLength(60)]
        public string FROM_EMAIL { get; set; }

        [StringLength(60)]
        public string REPLY_EMAIL { get; set; }

        [StringLength(60)]
        public string BCC_EMAIL { get; set; }

        [StringLength(120)]
        public string FILE_DIR { get; set; }

        public int PORT { get; set; }

        [Required]
        [StringLength(1)]
        public string USETLS { get; set; }

        [Required]
        [StringLength(1)]
        public string USESSL { get; set; }
    }
}
