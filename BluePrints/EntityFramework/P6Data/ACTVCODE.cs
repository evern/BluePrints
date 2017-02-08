namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ACTVCODE")]
    public partial class ACTVCODE
    {
        public ACTVCODE()
        {
            TASKACTV = new HashSet<TASKACTV>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int actv_code_id { get; set; }

        public int actv_code_type_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(60)]
        public string short_name { get; set; }

        public int? parent_actv_code_id { get; set; }

        [StringLength(120)]
        public string actv_code_name { get; set; }

        [StringLength(6)]
        public string color { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ACTVTYPE ACTVTYPE { get; set; }

        public virtual ICollection<TASKACTV> TASKACTV { get; set; }
    }
}