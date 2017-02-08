namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MANIFESTS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MANIFESTNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(15)]
        public string CARRIERCODE { get; set; }

        [StringLength(40)]
        public string DETAILS { get; set; }
    }
}