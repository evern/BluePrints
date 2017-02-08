namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROJSHAR")]
    public partial class PROJSHAR
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int proj_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int session_id { get; set; }

        public int access_level { get; set; }

        [Required]
        [StringLength(1)]
        public string load_status { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual USESSION USESSION { get; set; }
    }
}