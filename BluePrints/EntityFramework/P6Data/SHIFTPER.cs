namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SHIFTPER")]
    public partial class SHIFTPER
    {
        public SHIFTPER()
        {
            RSRCRATE = new HashSet<RSRCRATE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int shift_period_id { get; set; }

        public int shift_id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal shift_start_hr_num { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RSRCRATE> RSRCRATE { get; set; }

        public virtual SHIFT SHIFT { get; set; }
    }
}