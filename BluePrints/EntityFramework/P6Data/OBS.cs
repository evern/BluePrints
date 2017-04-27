namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class OBS
    {
        public OBS()
        {
            OBSPROJ = new HashSet<OBSPROJ>();
            PROJISSU = new HashSet<PROJISSU>();
            PROJTHRS = new HashSet<PROJTHRS>();
            PROJWBS = new HashSet<PROJWBS>();
            USEROBS = new HashSet<USEROBS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int obs_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(100)]
        public string obs_name { get; set; }

        public int? parent_obs_id { get; set; }

        [StringLength(22)]
        public string guid { get; set; }

        [Column(TypeName = "text")]
        public string obs_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<OBSPROJ> OBSPROJ { get; set; }

        public virtual ICollection<PROJISSU> PROJISSU { get; set; }

        public virtual ICollection<PROJTHRS> PROJTHRS { get; set; }

        public virtual ICollection<PROJWBS> PROJWBS { get; set; }

        public virtual ICollection<USEROBS> USEROBS { get; set; }
    }
}