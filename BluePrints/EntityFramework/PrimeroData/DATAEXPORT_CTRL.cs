namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DATAEXPORT_CTRL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DATAEXPORT_ID { get; set; }

        public int EXPORTSEQNO { get; set; }
    }
}
