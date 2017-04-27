namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROFILE_FIELDS
    {
        [Key]
        [StringLength(40)]
        public string FIELDNAME { get; set; }

        public int PROFILETYPE { get; set; }

        [StringLength(250)]
        public string DEFAULTVALUE { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }

        public int VERSION { get; set; }

        public int? TYPEINFOID { get; set; }

        public int? CONSTRAINTID { get; set; }

        [StringLength(255)]
        public string PROGNAME { get; set; }

        [StringLength(255)]
        public string MODULENAME { get; set; }

        [StringLength(255)]
        public string FUNCTIONNAME { get; set; }

        public int? FIELDLEVEL { get; set; }

        [Column(TypeName = "text")]
        public string NOTES { get; set; }

        public int? EXODBVERSION { get; set; }

        [StringLength(255)]
        public string KEYWORDS { get; set; }

        [StringLength(255)]
        public string FIELDPROPERTIES { get; set; }

        public int? DBSCOPE { get; set; }
    }
}