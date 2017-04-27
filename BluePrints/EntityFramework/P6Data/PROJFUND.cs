namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PROJFUND")]
    public partial class PROJFUND
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_fund_id { get; set; }

        public int fund_id { get; set; }

        public int proj_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fund_cost { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fund_wt { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual FUNDSRC FUNDSRC { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}