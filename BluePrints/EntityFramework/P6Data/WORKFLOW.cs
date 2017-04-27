namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WORKFLOW")]
    public partial class WORKFLOW
    {
        public WORKFLOW()
        {
            WKFLUSER = new HashSet<WKFLUSER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int work_flow_id { get; set; }

        public int proj_id { get; set; }

        [Required]
        [StringLength(255)]
        public string workflow_name { get; set; }

        [Required]
        [StringLength(512)]
        public string external_key { get; set; }

        public int stage_num { get; set; }

        [Required]
        [StringLength(15)]
        public string status { get; set; }

        [Required]
        [StringLength(1)]
        public string existing_project_flag { get; set; }

        [Required]
        [StringLength(255)]
        public string stage_name { get; set; }

        [Required]
        [StringLength(1)]
        public string stage_modified_flag { get; set; }

        public DateTime initiated_date { get; set; }

        public int? workspace_id { get; set; }

        public int? user_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<WKFLUSER> WKFLUSER { get; set; }

        public virtual WORKSPACE WORKSPACE { get; set; }
    }
}