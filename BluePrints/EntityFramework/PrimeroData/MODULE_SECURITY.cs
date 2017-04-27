namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MODULE_SECURITY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APP_ID { get; set; }

        public int? MAX_USERS { get; set; }

        public int? VERSION_NO { get; set; }

        public int? SECURITY_CODE { get; set; }

        public DateTime? EXPDATE { get; set; }

        [StringLength(255)]
        public string ACCESSCODE { get; set; }

        [StringLength(255)]
        public string MODULE_INFO { get; set; }
    }
}