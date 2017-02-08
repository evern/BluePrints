namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SCENARIO")]
    public partial class SCENARIO
    {
        public SCENARIO()
        {
            SCENPROJ = new HashSet<SCENPROJ>();
            SCENROLE = new HashSet<SCENROLE>();
            SCENUSER = new HashSet<SCENUSER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int scenario_id { get; set; }

        [Required]
        [StringLength(255)]
        public string scenario_name { get; set; }

        public int? user_id { get; set; }

        [StringLength(30)]
        public string table_name { get; set; }

        public int? fk_id { get; set; }

        [StringLength(30)]
        public string scenario_type { get; set; }

        public int? view_pref_id { get; set; }

        [StringLength(30)]
        public string view_type { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual VIEWPREF VIEWPREF { get; set; }

        public virtual ICollection<SCENPROJ> SCENPROJ { get; set; }

        public virtual ICollection<SCENROLE> SCENROLE { get; set; }

        public virtual ICollection<SCENUSER> SCENUSER { get; set; }
    }
}