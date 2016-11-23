namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DOCREVIEW")]
    public partial class DOCREVIEW
    {
        public DOCREVIEW()
        {
            DOCREVIEWTASK = new HashSet<DOCREVIEWTASK>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int doc_review_id { get; set; }

        public int doc_id { get; set; }

        public int? user_id { get; set; }

        [StringLength(255)]
        public string review_name { get; set; }

        [StringLength(255)]
        public string review_descr { get; set; }

        [StringLength(15)]
        public string review_type { get; set; }

        public DateTime? due_date { get; set; }

        [StringLength(15)]
        public string status { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual DOCUMENT DOCUMENT { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<DOCREVIEWTASK> DOCREVIEWTASK { get; set; }
    }
}
