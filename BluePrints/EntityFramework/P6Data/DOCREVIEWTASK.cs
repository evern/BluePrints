namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DOCREVIEWTASK")]
    public partial class DOCREVIEWTASK
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int review_task_id { get; set; }

        public int doc_review_id { get; set; }

        public int? user_id { get; set; }

        public DateTime? review_date { get; set; }

        [StringLength(4000)]
        public string comments { get; set; }

        public int? priority { get; set; }

        [StringLength(15)]
        public string status { get; set; }

        [StringLength(4000)]
        public string attachment_uuid { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual DOCREVIEW DOCREVIEW { get; set; }

        public virtual USERS USERS { get; set; }
    }
}