namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class WBSRSRC_QTY
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int wbsrsrc_id { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime week_start { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime month_start { get; set; }

        [Column(TypeName = "numeric")]
        public decimal qty { get; set; }

        public int? fin_dates_id1 { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_qty1 { get; set; }

        public int? fin_dates_id2 { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fin_qty2 { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual FINDATES FINDATES { get; set; }

        public virtual FINDATES FINDATES1 { get; set; }

        public virtual WBSRSRC WBSRSRC { get; set; }
    }
}
