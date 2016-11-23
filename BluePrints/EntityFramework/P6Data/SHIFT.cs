namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SHIFT")]
    public partial class SHIFT
    {
        public SHIFT()
        {
            RSRC = new HashSet<RSRC>();
            SHIFTPER = new HashSet<SHIFTPER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int shift_id { get; set; }

        [Required]
        [StringLength(60)]
        public string shift_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RSRC> RSRC { get; set; }

        public virtual ICollection<SHIFTPER> SHIFTPER { get; set; }
    }
}
