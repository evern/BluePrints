namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CONTACT_LIST
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string TITLE { get; set; }

        [StringLength(1000)]
        public string DESCRIPT { get; set; }

        public int? CONTACT_LIST_TYPE { get; set; }

        [StringLength(1)]
        public string IS_PRIVATE { get; set; }

        [StringLength(1)]
        public string IS_ACTIVE { get; set; }

        public int CREATED_BY { get; set; }

        public DateTime CREATEDATE { get; set; }

        public DateTime LAST_MODIFIED { get; set; }

        public string SETTINGS { get; set; }

        public int OWNER { get; set; }
    }
}