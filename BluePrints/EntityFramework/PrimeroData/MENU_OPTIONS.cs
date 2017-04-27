namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class MENU_OPTIONS
    {
        [Key]
        public int SEQNO { get; set; }

        public int? MENU_NO { get; set; }

        public int? COLUMN_NO { get; set; }

        public int? SORT_ORDER { get; set; }

        [StringLength(50)]
        public string CAPTION { get; set; }

        public int? PROCNO { get; set; }

        [StringLength(100)]
        public string PROC_DATA { get; set; }

        public int? SUB_COLUMN { get; set; }

        public int? MENU_TYPE { get; set; }

        public int? SHORTCUT { get; set; }

        public int IMAGEINDEX { get; set; }
    }
}