namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UEVNTREG")]
    public partial class UEVNTREG
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int user_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string app_name { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(255)]
        public string operation_name { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(255)]
        public string action_name { get; set; }

        public int? action_level { get; set; }

        [StringLength(255)]
        public string user_name { get; set; }

        [StringLength(4000)]
        public string event_reg_data { get; set; }

        public virtual USERS USERS { get; set; }
    }
}