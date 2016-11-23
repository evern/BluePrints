namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("VWPREFDATA")]
    public partial class VWPREFDATA
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int view_pref_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string view_pref_key { get; set; }

        [StringLength(4000)]
        public string view_pref_value { get; set; }

        [Column(TypeName = "text")]
        public string view_pref_value_blob { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual VIEWPREF VIEWPREF { get; set; }
    }
}
