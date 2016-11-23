namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PRMQUEUE")]
    public partial class PRMQUEUE
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string queue_name { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(32)]
        public string msg_key { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime enqueue_date { get; set; }

        [StringLength(255)]
        public string enqueue_user { get; set; }

        [StringLength(10)]
        public string status_code { get; set; }

        public int? priority { get; set; }

        [StringLength(255)]
        public string sender_name { get; set; }

        public DateTime? dequeue_date { get; set; }

        [StringLength(255)]
        public string dequeue_user { get; set; }

        public int? retry_count { get; set; }

        [StringLength(255)]
        public string msg_type { get; set; }

        [StringLength(255)]
        public string msg_sub_type { get; set; }

        [StringLength(255)]
        public string key1 { get; set; }

        [StringLength(255)]
        public string key2 { get; set; }

        [StringLength(4000)]
        public string err_msg { get; set; }

        [StringLength(4000)]
        public string queue_payload { get; set; }

        [Column(TypeName = "text")]
        public string queue_payload_extended { get; set; }
    }
}
