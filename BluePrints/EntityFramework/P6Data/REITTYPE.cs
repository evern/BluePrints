namespace BluePrints.P6Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("REITTYPE")]
    public partial class REITTYPE
    {
        public REITTYPE()
        {
            RELITEMS = new HashSet<RELITEMS>();
            RELITEMS1 = new HashSet<RELITEMS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int related_item_type_id { get; set; }

        [Required]
        [StringLength(32)]
        public string related_item { get; set; }

        [StringLength(30)]
        public string table_name { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual ICollection<RELITEMS> RELITEMS { get; set; }

        public virtual ICollection<RELITEMS> RELITEMS1 { get; set; }
    }
}
