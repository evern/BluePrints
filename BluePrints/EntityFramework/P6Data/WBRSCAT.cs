namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WBRSCAT")]
    public partial class WBRSCAT
    {
        public WBRSCAT()
        {
            WBSRSRC = new HashSet<WBSRSRC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbrs_cat_id { get; set; }

        [Required]
        [StringLength(36)]
        public string wbrs_cat_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<WBSRSRC> WBSRSRC { get; set; }
    }
}
