namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ISSUHIST")]
    public partial class ISSUHIST
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int issue_id { get; set; }

        public int proj_id { get; set; }

        [Column(TypeName = "text")]
        public string issue_history { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }

        public virtual PROJECT PROJECT { get; set; }

        public virtual PROJISSU PROJISSU { get; set; }
    }
}