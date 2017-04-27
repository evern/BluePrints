namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("FUNDSRC")]
    public partial class FUNDSRC
    {
        public FUNDSRC()
        {
            PROJFUND = new HashSet<PROJFUND>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fund_id { get; set; }

        public int seq_num { get; set; }

        [Required]
        [StringLength(100)]
        public string fund_name { get; set; }

        public int? parent_fund_id { get; set; }

        [Column(TypeName = "text")]
        public string fund_descr { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<PROJFUND> PROJFUND { get; set; }
    }
}