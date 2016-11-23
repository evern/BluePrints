namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MENU_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int HDRSEQNO { get; set; }

        public int APPID { get; set; }

        [StringLength(50)]
        public string CAPTION { get; set; }

        public int PROCID { get; set; }

        [StringLength(100)]
        public string PROCPARAMS { get; set; }

        public int LINECLASS { get; set; }

        [StringLength(20)]
        public string LINECLASSCOORDINATES { get; set; }

        [StringLength(400)]
        public string LINECLASSPROPERTIES { get; set; }

        public int SORTORDER { get; set; }

        public int PARENTID { get; set; }

        public int SHORTCUT { get; set; }

        [Column(TypeName = "image")]
        public byte[] MENUIMAGE { get; set; }

        [Column(TypeName = "image")]
        public byte[] MENUIMAGE1 { get; set; }

        [Column(TypeName = "image")]
        public byte[] MENUIMAGE2 { get; set; }

        [Column(TypeName = "image")]
        public byte[] MENUIMAGE3 { get; set; }
    }
}
