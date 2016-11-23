namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class OBJECT_DATA
    {
        [StringLength(20)]
        public string ID { get; set; }

        public DateTime? DATE_CREATED { get; set; }

        public int? CLASS { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        [StringLength(30)]
        public string MIMECODE { get; set; }

        [StringLength(30)]
        public string FILENAME { get; set; }

        [Column(TypeName = "image")]
        public byte[] OBJDATA { get; set; }
    }
}
