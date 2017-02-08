namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("VIEWPREF")]
    public partial class VIEWPREF
    {
        public VIEWPREF()
        {
            SCENARIO = new HashSet<SCENARIO>();
            VWPREFDASH = new HashSet<VWPREFDASH>();
            VWPREFDATA = new HashSet<VWPREFDATA>();
            VWPREFUSER = new HashSet<VWPREFUSER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int view_pref_id { get; set; }

        [Required]
        [StringLength(255)]
        public string view_pref_name { get; set; }

        [Required]
        [StringLength(20)]
        public string view_pref_type { get; set; }

        public int? user_id { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<SCENARIO> SCENARIO { get; set; }

        public virtual USERS USERS { get; set; }

        public virtual ICollection<VWPREFDASH> VWPREFDASH { get; set; }

        public virtual ICollection<VWPREFDATA> VWPREFDATA { get; set; }

        public virtual ICollection<VWPREFUSER> VWPREFUSER { get; set; }
    }
}