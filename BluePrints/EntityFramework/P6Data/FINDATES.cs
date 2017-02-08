namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FINDATES
    {
        public FINDATES()
        {
            PROJECT = new HashSet<PROJECT>();
            TASKFIN = new HashSet<TASKFIN>();
            TRSRCFIN = new HashSet<TRSRCFIN>();
            WBSRSRC_QTY = new HashSet<WBSRSRC_QTY>();
            WBSRSRC_QTY1 = new HashSet<WBSRSRC_QTY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fin_dates_id { get; set; }

        [Required]
        [StringLength(60)]
        public string fin_dates_name { get; set; }

        public DateTime start_date { get; set; }

        public DateTime end_date { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJECT> PROJECT { get; set; }

        public virtual ICollection<TASKFIN> TASKFIN { get; set; }

        public virtual ICollection<TRSRCFIN> TRSRCFIN { get; set; }

        public virtual ICollection<WBSRSRC_QTY> WBSRSRC_QTY { get; set; }

        public virtual ICollection<WBSRSRC_QTY> WBSRSRC_QTY1 { get; set; }
    }
}